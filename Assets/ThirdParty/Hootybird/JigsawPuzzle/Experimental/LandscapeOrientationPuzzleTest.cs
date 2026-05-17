using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Menu;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Tools;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

namespace HootyBird.JigsawPuzzleEngine.Experimental
{
    public class LandscapeOrientationPuzzleTest : MonoBehaviour
    {
        [SerializeField]
        private Puzzle puzzle;
        [SerializeField]
        private PuzzleSettingsObject settings;
        [SerializeField]
        private RenderTexture puzzleTexture;
        [SerializeField]
        private PuzzlePiecesPanel piecesPanel;
        [SerializeField]
        private VideoPlayer player;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();

            player.started += PlayerStarted;
            puzzle.OnPuzzleInitialized += OnPuzzleInitialized;

            Application.targetFrameRate = Settings.InternalAppSettings.TargetFramerate;
        }

        public void InitializePuzzle()
        {
            puzzle.Initialize(PuzzleFactory.FromPuzzleSettings(settings.PuzzleSettings, 0), puzzleTexture);
        }

        private void PlayerStarted(VideoPlayer source)
        {
            InitializePuzzle();
        }

        private void OnPuzzleInitialized(bool reinitialized)
        {
            piecesPanel.Initialize(puzzle.PuzzlePieces.Take((int)(puzzle.PuzzlePieces.Count * .3f)).ToList());

            // Update puzzle pieces restriction rects.
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            Rect panelRect = new Rect(corners[0].x, corners[0].y, (corners[2] - corners[0]).x, (corners[2] - corners[0]).y);
            foreach (PuzzlePiece piece in puzzle.PuzzlePieces)
            {
                piece.SetRestrictionWorldRect(panelRect);
            }
        }
    }
}
