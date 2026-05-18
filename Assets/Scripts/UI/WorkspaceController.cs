using UnityEngine;
using UnityEngine.InputSystem;
using HootyBird.JigsawPuzzleEngine.Gameplay;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.UI
{
    // Attach to a GameObject in the Game scene.
    // Handles workspace zoom (via Hootybird's PuzzlePanelInteraction events)
    // and 1-finger pan (when no piece is being dragged).
    public class WorkspaceController : MonoBehaviour
    {
        [SerializeField] private float _minScale       = 0.5f;
        [SerializeField] private float _maxScale       = 3f;
        [SerializeField] private float _zoomSensitivity = 0.002f;

        private RectTransform          _workspaceRect;
        private PuzzlePanelInteraction _panelInteraction;

        private bool    _isZooming;
        private bool    _isPanning;
        private Vector2 _panStartScreenPos;
        private Vector2 _workspaceAnchoredOrigin;

        private void OnEnable()
        {
            if (HootyBridge.Instance != null)
                HootyBridge.Instance.OnPuzzleLoaded += HandlePuzzleLoaded;
        }

        private void OnDisable()
        {
            if (HootyBridge.Instance != null)
                HootyBridge.Instance.OnPuzzleLoaded -= HandlePuzzleLoaded;
            DetachPanelInteraction();
        }

        private void HandlePuzzleLoaded(Puzzle puzzle, PuzzlePanelInteraction panelInteraction)
        {
            DetachPanelInteraction();
            _workspaceRect    = puzzle.GetComponent<RectTransform>();
            _panelInteraction = panelInteraction;
            if (_panelInteraction == null) return;
            _panelInteraction.OnZoomStarted += HandleZoomStarted;
            _panelInteraction.OnZoom        += HandleZoom;
            _panelInteraction.OnZoomStopped += HandleZoomStopped;
        }

        private void DetachPanelInteraction()
        {
            if (_panelInteraction == null) return;
            _panelInteraction.OnZoomStarted -= HandleZoomStarted;
            _panelInteraction.OnZoom        -= HandleZoom;
            _panelInteraction.OnZoomStopped -= HandleZoomStopped;
            _panelInteraction = null;
        }

        private void HandleZoomStarted(Vector2 center)
        {
            _isZooming = true;
            _isPanning  = false; // cancel pan if zoom starts mid-gesture
        }

        private void HandleZoomStopped() => _isZooming = false;

        private void HandleZoom(float distanceDelta, Vector2 centerDelta)
        {
            if (_workspaceRect == null) return;
            float current  = _workspaceRect.localScale.x;
            float newScale = Mathf.Clamp(current + distanceDelta * _zoomSensitivity, _minScale, _maxScale);
            _workspaceRect.localScale = Vector3.one * newScale;
        }

        private void Update()
        {
            if (_workspaceRect == null || _isZooming) return;
            if (HootyBridge.Instance == null || HootyBridge.Instance.IsAnyPieceDragging) return;

            var touchscreen = Touchscreen.current;
            if (touchscreen == null) return;

            var primary = touchscreen.primaryTouch;

            if (primary.press.wasPressedThisFrame)
            {
                _isPanning             = true;
                _panStartScreenPos      = primary.position.ReadValue();
                _workspaceAnchoredOrigin = _workspaceRect.anchoredPosition;
            }
            else if (primary.press.wasReleasedThisFrame)
            {
                _isPanning = false;
            }
            else if (_isPanning && primary.press.isPressed)
            {
                Vector2 delta = primary.position.ReadValue() - _panStartScreenPos;
                _workspaceRect.anchoredPosition = _workspaceAnchoredOrigin + delta;
            }
        }
    }
}
