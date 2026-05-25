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
            // Single-finger pan disabled — single touch is owned by Hootybird piece interaction.
            // Two-finger zoom is handled via PuzzlePanelInteraction events (HandleZoom).
        }
    }
}
