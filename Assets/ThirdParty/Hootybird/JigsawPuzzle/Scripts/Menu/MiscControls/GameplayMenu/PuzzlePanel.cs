using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Tween;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Panel used to control puzzle.
    /// Enables puzzle pinch in/out and puzzle panning.
    /// </summary>
    [RequireComponent(typeof(PuzzlePanelInteraction))]
    public class PuzzlePanel : MonoBehaviour
    {
        [SerializeField]
        private float zoomSpeed = .1f;
        [SerializeField]
        private float maxZoomMultiplier = 4f;
        [SerializeField]
        private float extraPadding = 1f;
        [SerializeField]
        private bool startZoomedOut = true;
        [SerializeField]
        private bool disableWhenDraggingMultiplePieces = true;
        [SerializeField]
        private Puzzle puzzle;

        private RectTransform parentRect;
        private RectTransform scrollRectTransform;
        private ScrollRect scrollRect; 
        private PuzzlePanelInteraction puzzlePanelInteraction;
        private ScaleTween scaleTween;
        private List<PuzzlePieceInteraction> puzzlePieceInteractions;

        private float baseZoomSpeed;
        private float minZoom;
        private float maxZoom;
        private Vector2 zoomPoint;
        private bool zoomStarted;
        private Vector3 previousPosition;
        private float previousScale;
        private bool worldRectUpdated;

        public RectTransform RectTransform { get; private set; }
        public Rect WorldRect { get; private set; }
        /// <summary>
        /// WorldRect calculated when puzzle panel was updated with new puzzle size.
        /// </summary>
        public Rect OriginalWorldRect { get; private set; }
        public Action<Rect> OnWorldRectUpdated { get; set; }
        public Action<float> OnScaleUpdate { get; set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
            parentRect = transform.parent.GetComponent<RectTransform>();
            scrollRect = GetComponentInParent<ScrollRect>();
            scrollRectTransform = scrollRect.GetComponent<RectTransform>();

            puzzle.OnPuzzleInitialized += OnPuzzleInitialized;
            puzzle.OnPuzzleSizeUpdated += OnPuzzleSizeUpdated;
            puzzle.OnPuzzlePieceCreated += OnPuzzlePieceCreated;

            puzzlePanelInteraction = GetComponent<PuzzlePanelInteraction>();
            if (puzzlePanelInteraction.enabled)
            {
                puzzlePanelInteraction.OnZoomStarted += OnZoomStarted;
                puzzlePanelInteraction.OnZoom += ZoomPanel;
                puzzlePanelInteraction.OnZoomStopped += OnZoomStopped;
            }

            scaleTween = GetComponent<ScaleTween>();

            baseZoomSpeed = Mathf.Min(Screen.width, Screen.height);
        }

        private void Update()
        {
            if (previousScale != transform.lossyScale.x || previousPosition != transform.position)
            {
                previousPosition = transform.position;
                previousScale = transform.lossyScale.x;

                UpdateWorldRect();
            }
        }

        private void LateUpdate()
        {
            worldRectUpdated = false;
        }

        /// <summary>
        /// Animation played when puzzle is complete.
        /// </summary>
        public void PuzzleComplete()
        {
            scaleTween.from = transform.localScale;
            scaleTween.PlayForward(true);
        }

        /// <summary>
        /// Invoked when pinch start event is triggered.
        /// </summary>
        /// <param name="pos"></param>
        private void OnZoomStarted(Vector2 pos)
        {
            // Can't start outside panel rect.
            if (!RectTransformUtility.RectangleContainsScreenPoint(RectTransform, pos, Camera.main))
            {
                return;
            }

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, pos, Camera.main, out Vector2 onRect))
            {
                // Remember pinch point, used for zooming into specific point in panel.
                zoomPoint = onRect;
            }

            zoomStarted = true;

            SetScrollRectState(false);
        }


        private void ZoomPanel(float zoomDelta, Vector2 posDelta)
        {
            if (!zoomStarted)
            {
                return;
            }

            float zoomDeltaAdjusted = zoomDelta / baseZoomSpeed * zoomSpeed;
            float prevcZoom = RectTransform.localScale.x;
            float zoomValue = Mathf.Clamp(RectTransform.localScale.x + zoomDeltaAdjusted, minZoom, maxZoom);
            RectTransform.localScale = new Vector3(zoomValue, zoomValue, 1f);

            // Since we're zooming into specific point, move panel towards it.
            RectTransform.anchoredPosition += -zoomPoint * (RectTransform.localScale.x - prevcZoom) + posDelta;

            OnScaleUpdate?.Invoke(zoomValue);
        }

        /// <summary>
        /// Invoked when zooming is done.
        /// </summary>
        private void OnZoomStopped()
        {
            if (!zoomStarted)
            {
                return;
            }

            SetScrollRectState(true);
            zoomStarted = false;
        }

        private void OnPuzzleInitialized(bool reinitialized)
        {
            SetScrollRectState(true);

            puzzlePanelInteraction.SetState(true);

            // Reset list of puzzle piece interactions.
            puzzlePieceInteractions = new List<PuzzlePieceInteraction>(
                puzzle.PuzzlePieces.Select(piece => piece.GetComponent<PuzzlePieceInteraction>()));
        }

        /// <summary>
        /// Set scroll rect state.
        /// </summary>
        /// <param name="state"></param>
        private void SetScrollRectState(bool state)
        {
            if (state)
            {
                scrollRect.enabled = true;
            }
            else
            {
                scrollRect.StopMovement();
                scrollRect.enabled = false;
            }
        }

        /// <summary>
        /// Invoked when puzzle rect size is changed. 
        /// Modifies panel size.
        /// Defines min/max zoom values.
        /// </summary>
        private void OnPuzzleSizeUpdated()
        {
            Vector2 puzzlePanelSize = new Vector2(
                puzzle.RectTransform.rect.width + puzzle.PuzzlePieceRectSize * extraPadding,
                puzzle.RectTransform.rect.height + puzzle.PuzzlePieceRectSize * extraPadding);

            float aspectRatio = puzzlePanelSize.x / puzzlePanelSize.y;
            float scrollRectAspectRatio = scrollRectTransform.rect.width / scrollRectTransform.rect.height;
            // Fit width.
            if (aspectRatio > scrollRectAspectRatio) 
            {
                minZoom = scrollRectTransform.rect.width / puzzlePanelSize.x;

                // Fit panel height to view.
                puzzlePanelSize.y = parentRect.rect.height / minZoom;
            }
            // Fit height.
            else
            {
                minZoom = scrollRectTransform.rect.height / puzzlePanelSize.y;

                // Fit panel width to view.
                puzzlePanelSize.x = parentRect.rect.width / minZoom;
            }

            // Update panelTween.
            scaleTween.to = new Vector3(minZoom, minZoom, 1f);

            // Update panel size.
            RectTransform.sizeDelta = puzzlePanelSize;
            RectTransform.anchoredPosition = Vector3.zero;

            float puzzleZoomReference = puzzlePanelSize.x / (10 * puzzle.PuzzlePieceRectSize);
            maxZoom = minZoom * maxZoomMultiplier * puzzleZoomReference;

            if (startZoomedOut)
            {
                RectTransform.localScale = new Vector3(minZoom, minZoom, 1f);
            }

            UpdateWorldRect();

            // Store worldRect after puzzle panel was initialized.
            OriginalWorldRect = new Rect(WorldRect);
        }

        private void UpdateWorldRect()
        {
            if (worldRectUpdated)
            {
                return;
            }

            Vector3[] corners = new Vector3[4];
            RectTransform.GetWorldCorners(corners);
            WorldRect = new Rect(corners[0].x, corners[0].y, (corners[2] - corners[0]).x, (corners[2] - corners[0]).y);

            worldRectUpdated = true;

            OnWorldRectUpdated?.Invoke(WorldRect);
        }


        private void OnPuzzlePieceCreated(PuzzlePiece piece)
        {
            PuzzlePieceInteraction interactions = piece.GetComponent<PuzzlePieceInteraction>();

            if (interactions && disableWhenDraggingMultiplePieces)
            {
                interactions.OnPieceDrag += OnPieceDrag;
                interactions.OnPiecePointerUp += OnPointerUpFromPiece;
            }
        }

        private void OnPieceDrag(PointerEventData eventData, PuzzlePiece piece)
        {
            if (puzzlePanelInteraction.State)
            {
                puzzlePanelInteraction.SetState(false);
            }
        }

        private void OnPointerUpFromPiece(PointerEventData eventData, PuzzlePiece piece)
        {
            if (puzzlePieceInteractions.Where(puzzlePanelInteraction => puzzlePanelInteraction.IsDraggingPiece).Count() == 1)
            {
                puzzlePanelInteraction.SetState(true);
            }
        }
    }
}
