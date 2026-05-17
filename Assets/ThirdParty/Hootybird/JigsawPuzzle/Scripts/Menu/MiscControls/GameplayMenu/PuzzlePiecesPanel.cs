using HootyBird.JigsawPuzzleEngine.Gameplay;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Panel with puzzle pieces available to drag into the puzzle board.
    /// </summary>
    public class PuzzlePiecesPanel : MenuWidget
    {
        [SerializeField]
        private RectTransform piecesParent;
        [SerializeField]
        private RectTransform movingPiecesParent;
        [SerializeField]
        private PuzzlePanel puzzlePanel;
        [SerializeField]
        private Puzzle puzzle;
        [SerializeField]
        private bool horizontal = true;

        private float pieceSize;
        private List<MovingPieceData> movingPieces;

        public RectTransform RectTransform { get; private set; }

        protected override void Awake()
        {
            base.Awake();

            RectTransform = GetComponent<RectTransform>();

            movingPieces = new List<MovingPieceData>();
            puzzle.OnPuzzlePieceCreated += OnPuzzlePieceCreated;
            puzzle.OnPuzzleClearedValues += OnPuzzleClearedValue;

            puzzlePanel.OnScaleUpdate += OnPuzzlePanelScaleUpdated;
        }

        /// <summary>
        /// Invoked by <see cref="PuzzleOverlay.OnPuzzleInitialized(bool)"/> when puzzle is ready.
        /// </summary>
        /// <param name="pieces">Pieces to move under panel.</param>
        public void Initialize(List<PuzzlePiece> pieces)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            gameObject.SetActive(false);
            // Only move pieces that have the correct state.
            foreach (PuzzlePiece piece in pieces.Where(piece => piece.State == PuzzlePieceState.None))
            {
                piece.transform.SetParent(piecesParent, false);

                PuzzlePieceInteraction puzzlePieceInteraction = piece.GetComponent<PuzzlePieceInteraction>();
                puzzlePieceInteraction.EventTrigger.enabled = false;
            }
            gameObject.SetActive(true);

            pieceSize = pieces.First().RectTransform.rect.width;
            UnityEngine.Debug.Log($"Puzzle pieces placement in pieces panel took {sw.ElapsedMilliseconds}ms");
        }

        private PuzzlePiecesPanelPlaceholder AddPlaceholder(float size)
        {
            GameObject go = new GameObject("placeholder");
            go.transform.SetParent(piecesParent, false);
            go.transform.localScale = Vector3.one;

            go.AddComponent<RectTransform>();
            PuzzlePiecesPanelPlaceholder placeholder = go.AddComponent<PuzzlePiecesPanelPlaceholder>(); 
            placeholder.SetSize(size);

            return placeholder;
        }

        private void OnPuzzlePieceCreated(PuzzlePiece puzzlePiece)
        {
            PuzzlePieceInteraction puzzlePieceInteraction = puzzlePiece.GetComponent<PuzzlePieceInteraction>();

            puzzlePieceInteraction.OnPiecePointerDown += OnPiecePointerDown;
            puzzlePieceInteraction.OnPieceDrag += OnPieceDrag;
            puzzlePieceInteraction.OnPiecePointerUp += OnPiecePointerUp;

            // Drop events are triggered by this panel.
            puzzlePieceInteraction.TriggerDropEvent = false;
        }

        private void OnPiecePointerDown(PointerEventData eventData, PuzzlePiece piece)
        {
            // Ignore for clusters.
            if (piece.Cluster != null)
            {
                return;
            }

            CreateMovingPieceData(
                eventData,
                piece,
                Vector2.zero);
        }

        private void OnPieceDrag(PointerEventData eventData, PuzzlePiece piece)
        {
            // Ignore for clusters.
            if (piece.Cluster != null)
            {
                return;
            }

            // Put under moving pieces parent.
            if (piece.transform.parent != movingPiecesParent)
            {
                piece.transform.SetParent(movingPiecesParent, true);
            }

            OnPieceDragged(eventData);
        }

        private void OnPiecePointerUp(PointerEventData eventData, PuzzlePiece piece)
        {
            // Manually trigger drop event for clusters.
            if (piece.Cluster != null)
            {
                puzzle.CheckPuzzlePiece(piece);
            }
            else
            {
                OnPieceDrop(eventData);
            }
        }

        private void OnPuzzlePanelScaleUpdated(float scaleValue)
        {
            // Update all moving pieces scale.
            foreach (MovingPieceData data in movingPieces)
            {
                if (!data.puzzlePieceInteraction.IsDraggingPiece)
                {
                    continue;
                }

                data.puzzlePiece.transform.localScale = puzzlePanel.transform.localScale;
            }
        }

        private void TryUpdatePlaceholderForMoveingPiece(MovingPieceData data)
        {
            if (!data.placeholder.gameObject.activeInHierarchy)
            {
                data.placeholder.SetActive(true);
            }

            Vector3[] corners = new Vector3[4];
            for (int childIndex = 0; childIndex < piecesParent.childCount; childIndex++)
            {
                (piecesParent.GetChild(childIndex) as RectTransform).GetWorldCorners(corners);

                if (horizontal)
                {
                    if (data.puzzlePiece.transform.position.x > corners[0].x &&
                        data.puzzlePiece.transform.position.x < corners[3].x)
                    {
                        data.placeholder.transform.SetSiblingIndex(childIndex);
                        break;
                    }
                }
                else
                {
                    if (data.puzzlePiece.transform.position.y > corners[0].y &&
                        data.puzzlePiece.transform.position.y < corners[2].y)
                    {
                        data.placeholder.transform.SetSiblingIndex(childIndex);
                        break;
                    }
                }
            }
        }

        private void OnPuzzleClearedValue()
        {
            movingPieces.Clear();
        }

        #region Pointer events, invoked by EventTrigger.

        public void EventTrigger_OnPointerDown(BaseEventData baseEventData)
        {
            PointerEventData pointerEventData = (PointerEventData)baseEventData;

            OnPiecePressedDown(pointerEventData);
        }

        public void EventTrigger_OnDrag(BaseEventData baseEventData)
        {
            PointerEventData pointerEventData = (PointerEventData)baseEventData;

            MovingPieceData target = movingPieces.Find(data => data.eventData == pointerEventData);
            // Only continue if onDown event started from puzzle piece.
            if (target == null)
            {
                return;
            }

            bool pointerInPanel = RectTransformUtility.RectangleContainsScreenPoint(
                RectTransform, 
                pointerEventData.position, 
                Camera.main);

            // If pointer moved outside piecesPanel, start dragging assigned puzzle piece.
            if (!pointerInPanel)
            {
                // Update parent.
                target.puzzlePiece.transform.SetParent(movingPiecesParent, true);

                // Enable puzzle piece interactions.
                target.puzzlePieceInteraction.EventTrigger.enabled = true;

                // Redirect pointerEvents to puzzle piece.
                pointerEventData.pointerDrag = target.puzzlePiece.gameObject;
                pointerEventData.pointerPress = target.puzzlePiece.gameObject;
            }
        }

        public void EventTrigger_OnPointerUp(BaseEventData baseEventData)
        {
            PointerEventData pointerEventData = (PointerEventData)baseEventData;
            
            OnPieceDrop(pointerEventData);
        }

        private void OnPiecePressedDown(PointerEventData pointerEventData)
        {
            // Check if pointer hit any (ignore already manipulated pieces) puzzle piece.
            foreach (PuzzlePiece piece in piecesParent
                .GetComponentsInChildren<PuzzlePiece>()
                .Except(movingPieces.Select(data => data.puzzlePiece)))
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(piece.RectTransform, pointerEventData.position, Camera.main))
                {
                    CreateMovingPieceData(
                        pointerEventData,
                        piece,
                        pointerEventData.position - RectTransformUtility.WorldToScreenPoint(Camera.main, piece.transform.position));

                    return;
                }
            }
        }

        private void OnPieceDragged(PointerEventData pointerEventData)
        {
            MovingPieceData target = movingPieces.Find(data => data.eventData == pointerEventData);
            // Only continue if onDown event started from puzzle piece.
            if (target == null)
            {
                return;
            }

            bool pointerInPanel = RectTransformUtility.RectangleContainsScreenPoint(
                RectTransform,
                pointerEventData.position,
                Camera.main);

            if (target.puzzlePieceInteraction.IsDraggingPiece)
            {
                // Update puzzle piece scale depending on pointer position.
                if (pointerInPanel && !target.underPanel)
                {
                    target.puzzlePiece.transform.localScale = Vector3.one;
                    target.underPanel = true;
                }
                else if (!pointerInPanel && target.underPanel)
                {
                    target.puzzlePiece.transform.localScale = puzzlePanel.transform.localScale;
                    target.underPanel = false;
                }

                // Update placeholder with target puzzlePiece position.
                if (pointerInPanel)
                {
                    TryUpdatePlaceholderForMoveingPiece(target);
                }
                else
                {
                    target.placeholder.gameObject.SetActive(false);
                }
            }
        }

        private void OnPieceDrop(PointerEventData pointerEventData)
        {
            MovingPieceData target = movingPieces.Find(data => data.eventData == pointerEventData);

            if (target == null)
            {
                return;
            }

            if (target.puzzlePieceInteraction.IsDraggingPiece)
            {
                Vector2 pos = pointerEventData.position - target.puzzlePieceScreenPosOffset;

                // Drop piece on a puzzle board if screenPos is not above PiecesPanel, and above puzzle board.
                if (!RectTransformUtility.RectangleContainsScreenPoint(RectTransform, pos, Camera.main) &&
                    RectTransformUtility.RectangleContainsScreenPoint(puzzlePanel.RectTransform, pos, Camera.main))
                {
                    target.puzzlePiece.transform.SetParent(puzzle.RectTransform, true);
                    target.puzzlePiece.State = PuzzlePieceState.Overlay;

                    puzzle.CheckPuzzlePiece(target.puzzlePiece);
                }
                else
                {
                    // Put under piecesPanel.
                    target.puzzlePiece.transform.SetParent(piecesParent, false);
                    target.puzzlePiece.transform.SetSiblingIndex(target.placeholder.transform.GetSiblingIndex());

                    // Disable event trigger.
                    target.puzzlePieceInteraction.EventTrigger.enabled = false;
                }

                // Restore scale.
                target.puzzlePiece.transform.localScale = Vector3.one;
            }

            // Clear target.
            Destroy(target.placeholder.gameObject);
            movingPieces.Remove(target);
        }

        #endregion

        private void CreateMovingPieceData(
            PointerEventData pointerEventData,
            PuzzlePiece piece, 
            Vector2 screenPosOffset)
        {
            MovingPieceData pieceData = new MovingPieceData()
            {
                puzzlePiece = piece,
                puzzlePieceScreenPosOffset = screenPosOffset,
                eventData = pointerEventData,
                underPanel = true,
                placeholder = AddPlaceholder(pieceSize),
                puzzlePieceInteraction = piece.GetComponent<PuzzlePieceInteraction>(),
            };
            pieceData.placeholder.gameObject.SetActive(false);

            movingPieces.Add(pieceData);
        }

        private class MovingPieceData
        {
            public Vector2 puzzlePieceScreenPosOffset;
            public PuzzlePiece puzzlePiece;
            public PuzzlePieceInteraction puzzlePieceInteraction;
            public PointerEventData eventData;
            public bool underPanel;
            public PuzzlePiecesPanelPlaceholder placeholder;
        }
    }
}