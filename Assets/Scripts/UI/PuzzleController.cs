using System.Collections.Generic;
using ChromaJigsaw.Audio;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;
using UnityEngine;
using UnityEngine.UI;

namespace ChromaJigsaw.UI
{
    // Attach to a GameObject in the Game scene.
    // Routes HootyBridge events to AudioManager SFX and handles Focus Mode HUD visibility.
    public class PuzzleController : MonoBehaviour
    {
        // Set before LoadScene(Game) to provide context for analytics and puzzle loading.
        public static string    SessionPackId    = "";
        public static string    SessionPuzzleId  = "";
        public static int       SessionPieceCount = 0;
        public static bool      SessionIsDaily    = false;
        public static Texture2D SessionTexture    = null;

        // Assign all HUD CanvasGroups (TopBar, any overlays) that should hide in Focus Mode.
        [SerializeField] private CanvasGroup[] _hudGroups;

        // Full-screen transparent button; tap toggles HUD when Focus Mode is active.
        [SerializeField] private Button _workspaceTapButton;

        private bool  _focusModeActive;
        private bool  _hudVisible = true;
        private float _puzzleStartTime;
        private int   _piecesPlaced;

        private void Start()
        {
            Debug.Log($"[PuzzleController] Start — SessionPieceCount={SessionPieceCount} SessionTexture={(SessionTexture == null ? "NULL" : SessionTexture.name)} SessionPuzzleId={SessionPuzzleId}");
            if (SessionPieceCount > 0 && SessionTexture != null)
                HootyBridge.Instance.LoadPuzzle(SessionTexture, SessionPieceCount, SessionPuzzleId);
            else
                Debug.LogWarning("[PuzzleController] SessionPieceCount=0 or SessionTexture=null — puzzle will NOT load. Did you navigate here correctly from Gallery?");
        }

        private void OnEnable()
        {
            if (HootyBridge.Instance != null)
            {
                HootyBridge.Instance.OnPiecePickedUp  += HandlePiecePickedUp;
                HootyBridge.Instance.OnPiecePlaced    += HandlePiecePlaced;
                HootyBridge.Instance.OnPieceSnapped   += HandlePieceSnapped;
                HootyBridge.Instance.OnPuzzleComplete += HandlePuzzleComplete;
            }

            _workspaceTapButton?.onClick.AddListener(HandleWorkspaceTap);
            AccessibilityService.OnFocusModeChanged += OnFocusModeChanged;
            if (SaveManager.Instance != null)
                OnFocusModeChanged(SaveManager.Instance.Data.focusMode);

            if (SessionPieceCount > 0)
            {
                _puzzleStartTime = Time.unscaledTime;
                _piecesPlaced    = 0;
                AnalyticsManager.Instance.LogPuzzleStarted(
                    SessionPackId, SessionPuzzleId, SessionPieceCount,
                    PieceCountToDifficulty(SessionPieceCount));
            }
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

            _workspaceTapButton?.onClick.RemoveListener(HandleWorkspaceTap);
            AccessibilityService.OnFocusModeChanged -= OnFocusModeChanged;
        }

        // ── SFX ──────────────────────────────────────────────────────────────

        private void HandlePiecePickedUp() => AudioManager.Instance.Play(SFXType.PiecePickup);
        private void HandlePiecePlaced()   => AudioManager.Instance.Play(SFXType.PiecePlaced);

        private void HandlePieceSnapped()
        {
            _piecesPlaced++;
            AudioManager.Instance.Play(SFXType.PieceSnap);
        }

        private void HandlePuzzleComplete(int _)
        {
            AudioManager.Instance.Play(SFXType.PuzzleComplete);
            float elapsed = Time.unscaledTime - _puzzleStartTime;
            AnalyticsManager.Instance.LogPuzzleCompleted(
                SessionPackId, SessionPuzzleId, SessionPieceCount, elapsed);
            if (SessionIsDaily)
            {
                DailyService.Instance.MarkComplete(SessionPuzzleId);
                AnalyticsManager.Instance.LogDailyCompleted(SessionPuzzleId, SessionPieceCount, elapsed);
            }
        }

        public void HandleAbandon()
        {
            float elapsed = Time.unscaledTime - _puzzleStartTime;
            AnalyticsManager.Instance.LogPuzzleAbandoned(
                SessionPackId, SessionPuzzleId, elapsed, _piecesPlaced);
        }

        // ── Focus Mode ────────────────────────────────────────────────────────

        private void OnFocusModeChanged(bool enabled)
        {
            _focusModeActive = enabled;
            _hudVisible      = !enabled;  // entering Focus Mode hides HUD immediately
            ApplyHudVisibility(_hudVisible);
        }

        public void HandleWorkspaceTap()
        {
            if (!_focusModeActive) return;
            _hudVisible = !_hudVisible;
            ApplyHudVisibility(_hudVisible);
            AnalyticsManager.Instance.LogEvent("focus_mode_toggled",
                new Dictionary<string, object> { { "enabled", !_hudVisible } });
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

        private static string PieceCountToDifficulty(int count) => count switch
        {
            12 => "easy",
            24 => "medium",
            48 => "hard",
            96 => "expert",
            _  => "unknown"
        };
    }
}
