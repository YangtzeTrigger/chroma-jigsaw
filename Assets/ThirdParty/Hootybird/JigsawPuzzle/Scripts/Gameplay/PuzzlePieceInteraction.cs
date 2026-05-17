using HootyBird.JigsawPuzzleEngine.Tools;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    [RequireComponent(typeof(PuzzlePiece))]
    public class PuzzlePieceInteraction : MonoBehaviour
    {
        [SerializeField]
        private bool triggerDropEvents = true;

        private PuzzlePiece piece;
        private Graphic graphic;
        private Puzzle puzzle;

        private float clickTimer;
        private PointerEventData activePointerEvent;
        private Vector2 clickPosition;

        public EventTrigger EventTrigger { get; private set; }

        public bool TriggerDropEvent
        {
            get => triggerDropEvents;
            set => triggerDropEvents = value;
        }

        public bool IsDraggingPiece { get; set; }
        public Action<PointerEventData, PuzzlePiece> OnPiecePointerDown { get; set; }
        public Action<PointerEventData, PuzzlePiece> OnPieceDrag { get; set; }
        public Action<PointerEventData, PuzzlePiece> OnPiecePointerUp { get; set; }

        /// <summary>
        /// Offset from the center of puzzle piece.
        /// </summary>
        private Vector2 puzzlePieceScreenPosOffset;

        private void Awake()
        {
            InitEventTrigger();

            piece = GetComponent<PuzzlePiece>();
            puzzle = GetComponentInParent<Puzzle>();
            graphic = GetComponent<Graphic>();

            piece.OnSnappedToPuzzleBoard += OnPieceSnappedToBoard;
            piece.OnPuzzlePieceReset += OnPuzzlePieceReset;
        }

        /// <summary>
        /// To make "enable" toggle visible in editor.
        /// </summary>
        private void OnEnable() { }

        private void InitEventTrigger()
        {
            EventTrigger = GetComponent<EventTrigger>();
            EventTrigger.enabled = true;

            EventTrigger.Entry pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
            pointerDown.callback.AddListener(EventTrigger_OnPoitnerDown);

            EventTrigger.Entry drag = new EventTrigger.Entry() { eventID = EventTriggerType.Drag };
            drag.callback.AddListener(EventTrigger_OnDrag);

            EventTrigger.Entry pointerUp = new EventTrigger.Entry() { eventID = EventTriggerType.PointerUp };
            pointerUp.callback.AddListener(EventTrigger_OnPointerUp);

            EventTrigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>()
            {
                pointerDown,
                drag,
                pointerUp,
            };
        }

        private void OnPieceSnappedToBoard(PuzzlePiece piece, PuzzlePieceEventOrigin eventOrigin)
        {
            EventTrigger.enabled = false;
            piece.RenderTarget.raycastTarget = false;
        }

        private void OnPuzzlePieceReset(PuzzlePiece piece)
        {
            IsDraggingPiece = false;
            EventTrigger.enabled = true;
            piece.RenderTarget.raycastTarget = true;
        }

        #region Event Trigger Events.

        public void EventTrigger_OnPoitnerDown(BaseEventData eventData)
        {
            // Ignore this event if puzzle piece is being dragged.
            if (IsDraggingPiece)
            {
                return;
            }

            PointerEventData pointerEventData = eventData as PointerEventData;

            // Bring to front.
            if (piece.Cluster != null)
            {
                // Make it so this one is first on a list.
                piece.transform.SetAsLastSibling();

                foreach (PuzzlePiece puzzlePiece in piece.Cluster.PuzzlePieces)
                {
                    if (puzzlePiece != piece)
                    {
                        puzzlePiece.transform.SetAsLastSibling();
                    }
                }
            }
            else
            {
                piece.transform.SetAsLastSibling();
            }

            // Save click time. Used for puzzle piece rotation support.
            switch (piece.State)
            {
                case PuzzlePieceState.Overlay:
                    // Store click data. Used for piece rotation functinality.
                    clickTimer = Time.time;
                    clickPosition = pointerEventData.position;

                    break;
            }

            puzzlePieceScreenPosOffset = Camera.main.ScreenToWorldPoint(pointerEventData.position) - piece.transform.position;
            OnPiecePointerDown?.Invoke(pointerEventData, piece);
        }

        public void EventTrigger_OnDrag(BaseEventData eventData)
        {
            PointerEventData pointerEventData = eventData as PointerEventData;

            if (!IsDraggingPiece)
            {
                IsDraggingPiece = true;
                activePointerEvent = pointerEventData;
            }
            // Already dragging this puzzle piece, so direct this event to nowhere.
            else if (pointerEventData != activePointerEvent)
            {
                pointerEventData.pointerDrag = null;
                pointerEventData.pointerPress = null;

                return;
            }

            Vector2 newPos = Camera.main.ScreenToWorldPoint(pointerEventData.position);
            piece.MoveTo(newPos - puzzlePieceScreenPosOffset);

            OnPieceDrag?.Invoke(pointerEventData, piece);
        }

        public void EventTrigger_OnPointerUp(BaseEventData eventData)
        {
            PointerEventData pointerEventData = eventData as PointerEventData;

            OnPiecePointerUp?.Invoke(pointerEventData, piece);

            if (IsDraggingPiece)
            {
                puzzlePieceScreenPosOffset = Vector2.zero;
                IsDraggingPiece = false;
                activePointerEvent = null;

                if (triggerDropEvents)
                {
                    puzzle.CheckPuzzlePiece(piece);
                }
            }
            else
            {
                // If click is fast enough, rotate puzzle piece.
                if (Time.time - clickTimer < Settings.PuzzleSettings.PuzzlePieceClickTimeThreshold &&
                    Vector2.Distance(pointerEventData.position, clickPosition) < Settings.PuzzleSettings.PuzzlePieceClickDistanceThreshold)
                {
                    // Rotates piece to next direction value. 
                    piece.RotateTo((PuzzlePieceDirection)(((int)piece.Direction + 1) % 4), eventOrigin: PuzzlePieceEventOrigin.Player);

                    // Check if piece can snap to board and join a cluster.
                    puzzle.CheckPuzzlePiece(piece);
                }
            }
        }

        #endregion
    }
}
