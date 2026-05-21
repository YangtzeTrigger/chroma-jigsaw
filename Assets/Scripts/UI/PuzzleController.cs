using ChromaJigsaw.Audio;
using ChromaJigsaw.Core;
using UnityEngine;

namespace ChromaJigsaw.UI
{
    // Attach to a GameObject in the Game scene.
    // Routes HootyBridge events to AudioManager SFX and handles Focus Mode HUD visibility.
    public class PuzzleController : MonoBehaviour
    {
        // Assign all HUD CanvasGroups (TopBar, any overlays) that should hide in Focus Mode.
        [SerializeField] private CanvasGroup[] _hudGroups;

        private bool _focusModeActive;
        private bool _hudVisible = true;

        private void OnEnable()
        {
            if (HootyBridge.Instance != null)
            {
                HootyBridge.Instance.OnPiecePickedUp  += HandlePiecePickedUp;
                HootyBridge.Instance.OnPiecePlaced    += HandlePiecePlaced;
                HootyBridge.Instance.OnPieceSnapped   += HandlePieceSnapped;
                HootyBridge.Instance.OnPuzzleComplete += HandlePuzzleComplete;
            }

            AccessibilityService.OnFocusModeChanged += OnFocusModeChanged;
            // Apply current saved state immediately.
            if (SaveManager.Instance != null)
                OnFocusModeChanged(SaveManager.Instance.Data.focusMode);
        }

        private void OnDisable()
        {
            if (HootyBridge.Instance != null)
            {
                HootyBridge.Instance.OnPiecePickedUp  -= HandlePiecePickedUp;
                HootyBridge.Instance.OnPiecePlaced    -= HandlePiecePlaced;
                HootyBridge.Instance.OnPieceSnapped   -= HandlePieceSnapped;
                HootyBridge.Instance.OnPuzzleComplete -= HandlePuzzleComplete;
            }

            AccessibilityService.OnFocusModeChanged -= OnFocusModeChanged;
        }

        // ── SFX ──────────────────────────────────────────────────────────────

        private void HandlePiecePickedUp()       => AudioManager.Instance.Play(SFXType.PiecePickup);
        private void HandlePiecePlaced()         => AudioManager.Instance.Play(SFXType.PiecePlaced);
        private void HandlePieceSnapped()        => AudioManager.Instance.Play(SFXType.PieceSnap);
        private void HandlePuzzleComplete(int _) => AudioManager.Instance.Play(SFXType.PuzzleComplete);

        // ── Focus Mode ────────────────────────────────────────────────────────

        private void OnFocusModeChanged(bool enabled)
        {
            _focusModeActive = enabled;
            _hudVisible      = !enabled;  // entering Focus Mode hides HUD immediately
            ApplyHudVisibility(_hudVisible);
        }

        // Call this from a transparent workspace tap button in the Game scene.
        public void HandleWorkspaceTap()
        {
            if (!_focusModeActive) return;
            _hudVisible = !_hudVisible;
            ApplyHudVisibility(_hudVisible);
        }

        private void ApplyHudVisibility(bool visible)
        {
            if (_hudGroups == null) return;
            foreach (var group in _hudGroups)
            {
                if (group == null) continue;
                group.alpha          = visible ? 1f : 0f;
                group.blocksRaycasts = visible;
                group.interactable   = visible;
            }
        }
    }
}
