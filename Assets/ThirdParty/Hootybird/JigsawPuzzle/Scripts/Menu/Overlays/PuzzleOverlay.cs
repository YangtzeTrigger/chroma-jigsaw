using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tween;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Main puzzle overlay. Controls puzzle flow from start to finish.
    /// </summary>
    public class PuzzleOverlay : MenuOverlay
    {
        [Header("Puzzle Controls")]
        [SerializeField]
        private Puzzle puzzle;
        [SerializeField]
        private PuzzlePiecesPanel puzzlePiecesPanel;
        [SerializeField]
        private PuzzlePanel puzzlePanel;
        [SerializeField]
        private PuzzleImage puzzleImage;

        [Space]
        [Header("Puzzle Overlay animated components")]
        [SerializeField]
        private TweenBase[] animations;

        [Space]
        [Header("UI Controls")]
        [SerializeField]
        private PuzzleProgressWidget puzzleProgressWidget;
        [SerializeField]
        private Button backButton;
        [SerializeField]
        private PuzzleImageToggle puzzleImageToggle;
        [SerializeField]
        private Button filterPuzzlePiecesButton;

        private GameplayMenuController gameplayMenuController;
        private PuzzlePanelInteraction puzzlePanelInteraction;
        private int piecesInSpot;
        private Rect worldRect;

        /// <summary>
        /// Puzzle pieces snapped to a board.
        /// </summary>
        public int PiecesInSpot
        {
            get => piecesInSpot;
            private set
            {
                piecesInSpot = value;
                puzzleProgressWidget.SetValue(PuzzleProgress);
            }
        }
        /// <summary>
        /// Current puzzle progress.
        /// </summary>
        public int PuzzleProgress
        {
            get
            {
                if (puzzle.PuzzleData != null)
                {
                    return Mathf.FloorToInt((float)PiecesInSpot / puzzle.PuzzleData.polygons.Length * 100f);
                }

                return 0;
            }
        }
        /// <summary>
        /// Puzzle object.
        /// </summary>
        public Puzzle Puzzle => puzzle;
        public Action OnPuzzleComplete { get; set; }

        protected override void Awake()
        {
            base.Awake();

            gameplayMenuController = MenuController as GameplayMenuController;

            puzzlePanelInteraction = puzzlePanel.GetComponent<PuzzlePanelInteraction>();
            puzzlePanelInteraction.OnZoomStarted += OnZoomStarted;

            puzzle.OnPuzzleInitialized += OnPuzzleInitialized;
            puzzle.OnPuzzlePieceCreated += OnPuzzlePieceCreated;
            puzzle.OnPuzzleClearedValues += OnPuzzleClearedValues;

            backButton.onClick.AddListener(OnBack);
            puzzleImageToggle.OnStateChanged += puzzleImage.SetState;
            filterPuzzlePiecesButton.onClick.AddListener(FilterPuzzlePieces);
        }

        protected override void Start()
        {
            base.Start();

            Vector3[] corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);
            worldRect = new Rect(corners[0].x, corners[0].y, (corners[2] - corners[0]).x, (corners[2] - corners[0]).y);
        }

        public void OnApplicationQuit()
        {
            if (puzzle.PuzzleData != null)
            {
                SaveGame();
            }
        }

        public void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (puzzle.PuzzleData != null)
                {
                    SaveGame();
                }
            }
        }

        /// <summary>
        /// Invoked when puzzle is initialized.
        /// </summary>
        /// <param name="reinitialized"></param>
        private void OnPuzzleInitialized(bool reinitialized)
        {
            if (!puzzle.StartAssembled)
            {
                // Update pieces panel.
                puzzlePiecesPanel.Initialize(puzzle.PuzzlePieces);
            }
            // And puzzle image that's under puzzle.
            puzzleImage.SetImage(puzzle.PuzzleTexture, puzzle.PuzzleTextureRect);

            foreach (TweenBase tween in animations)
            {
                tween.PlayForward(false);
            }
        }

        /// <summary>
        /// Subscribe to puzzle piece events.
        /// </summary>
        /// <param name="piece">Piece created.</param>
        private void OnPuzzlePieceCreated(PuzzlePiece piece)
        {
            piece.SetRestrictionWorldRect(worldRect);

            piece.OnSnappedToPuzzleCluster += OnSnappedToCluster;
            piece.OnRotated += OnPuzzlePieceRotated;
            piece.OnSnappedToPuzzleBoard += OnSnappedToGrid;
        }

        private void OnPuzzleClearedValues()
        {
            PiecesInSpot = 0;
        }

        public override void OnBack()
        {
            // When back is pressed, save and quit.
            SaveAndQuit();
        }

        private void OnZoomStarted(Vector2 pos)
        {

        }

        /// <summary>
        /// Invoked when filter pieces button is pressed.
        /// Makes small pieces and clusters appear ontop.
        /// </summary>
        private void FilterPuzzlePieces()
        {
            // Surface cluster pieces.
            foreach (PuzzlePiece piece in puzzle.PuzzlePieces.Where(piece => piece.Cluster != null))
            {
                piece.transform.SetAsLastSibling();
            }

            // Surface free floating pieces.
            foreach (PuzzlePiece piece in puzzle.PuzzlePieces.Where(piece => piece.State == PuzzlePieceState.Overlay))
            {
                piece.transform.SetAsLastSibling();
            }
        }

        /// <summary>
        /// Save and quit implementation.
        /// </summary>
        private void SaveAndQuit()
        {
            SaveGame();

            gameplayMenuController.OpenMainMenu();
            AudioService.Instance.PlaySfx("menu-back");
        }

        private void SaveGame()
        {
            // Only save if puzzle is no complete, and have any puzzle pieces on a board.
            if (PuzzleProgress < 100 && Puzzle.PuzzlePieces.Any(piece => piece.State != PuzzlePieceState.None))
            {
                SaveGameService.SaveGame(Puzzle);
            }
        }

        /// <summary>
        /// Invoked when puzzle piece is snapped to cluster.
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="eventOrigin"></param>
        private void OnSnappedToCluster(PuzzlePiece piece, PuzzlePieceEventOrigin eventOrigin)
        {
            // Make it so that clusters can't be moved outside puzzle panel.
            piece.SetRestrictionWorldRect(puzzlePanel.OriginalWorldRect);

            // Register sfx/vfx effects only for player invoked events.
            if (eventOrigin == PuzzlePieceEventOrigin.Player)
            {
                AudioService.Instance.PlaySfx("puzzle-piece-connected", .4f);
            }
        }

        /// <summary>
        /// Invoked when puzzle piece is rotated.
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="eventOrigin"></param>
        private void OnPuzzlePieceRotated(PuzzlePiece piece, PuzzlePieceEventOrigin eventOrigin)
        {
            if (eventOrigin == PuzzlePieceEventOrigin.Player)
            {
                AudioService.Instance.PlaySfx("puzzle-piece-rotate", .3f);
            }
        }

        /// <summary>
        /// Invoked when puzzle pieces is snapped to a board.
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="eventOrigin"></param>
        private void OnSnappedToGrid(PuzzlePiece piece, PuzzlePieceEventOrigin eventOrigin)
        {
            PiecesInSpot++;

            if (eventOrigin == PuzzlePieceEventOrigin.Player)
            {
                AudioService.Instance.PlaySfx("puzzle-piece-connected", .4f);

                if (PuzzleProgress == 100)
                {
                    OnPuzzleComplete?.Invoke();
                    PuzzleComplete();
                }
            }
        }

        /// <summary>
        /// Invoked when puzzle is complete.
        /// </summary>
        private void PuzzleComplete()
        {
            puzzlePanel.PuzzleComplete();

            foreach (TweenBase tween in animations)
            {
                tween.PlayBackward(false);
            }
        }
    }
}
