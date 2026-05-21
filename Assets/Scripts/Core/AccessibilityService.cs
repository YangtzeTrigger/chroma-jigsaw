using System;
using UnityEngine;

namespace ChromaJigsaw.Core
{
    public class AccessibilityService : MonoBehaviour
    {
        private static AccessibilityService _instance;
        public static AccessibilityService Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<AccessibilityService>();
                if (_instance == null) { var go = new GameObject("[AccessibilityService]"); _instance = go.AddComponent<AccessibilityService>(); }
                return _instance;
            }
        }

        // Subscribers receive the new value whenever a setting changes.
        public static event Action<bool>  OnFocusModeChanged;
        public static event Action<bool>  OnGrandInterfaceChanged;
        public static event Action<bool>  OnHighContrastChanged;
        public static event Action<float> OnArtworkBrightnessChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // Called by AppBootstrap after SaveManager loads — broadcasts saved state to all listeners.
        public void Initialise()
        {
            var d = SaveManager.Instance.Data;
            OnFocusModeChanged?.Invoke(d.focusMode);
            OnGrandInterfaceChanged?.Invoke(d.grandInterface);
            OnHighContrastChanged?.Invoke(d.highContrast);
            OnArtworkBrightnessChanged?.Invoke(d.artworkBrightness);
        }

        // ── Tactile Feedback ─────────────────────────────────────────────────

        public void SetTactileFeedback(bool enabled)
        {
            SaveManager.Instance.Data.tactileFeedback = enabled;
            SaveManager.Instance.Save();
        }

        // Gate all vibration calls here — one place, one flag check.
        public void Vibrate(long milliseconds = 50)
        {
            if (!SaveManager.Instance.Data.tactileFeedback) return;
#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                var activity    = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var vibrator    = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                vibrator.Call("vibrate", milliseconds);
            }
            catch { /* device may not have vibrator */ }
#endif
        }

        // ── Focus Mode ───────────────────────────────────────────────────────

        public void SetFocusMode(bool enabled)
        {
            SaveManager.Instance.Data.focusMode = enabled;
            SaveManager.Instance.Save();
            OnFocusModeChanged?.Invoke(enabled);
        }

        // ── High Contrast ────────────────────────────────────────────────────

        public void SetHighContrast(bool enabled)
        {
            SaveManager.Instance.Data.highContrast = enabled;
            SaveManager.Instance.Save();
            OnHighContrastChanged?.Invoke(enabled);
            // Full shader implementation in B-12 polish.
        }

        // ── Grand Interface ──────────────────────────────────────────────────

        public void SetGrandInterface(bool enabled)
        {
            SaveManager.Instance.Data.grandInterface = enabled;
            SaveManager.Instance.Save();
            OnGrandInterfaceChanged?.Invoke(enabled);
        }

        // ── Artwork Brightness ───────────────────────────────────────────────

        public void SetArtworkBrightness(float value)
        {
            value = Mathf.Clamp(value, 0.5f, 1.5f);
            SaveManager.Instance.Data.artworkBrightness = value;
            SaveManager.Instance.Save();
            OnArtworkBrightnessChanged?.Invoke(value);
        }
    }
}
