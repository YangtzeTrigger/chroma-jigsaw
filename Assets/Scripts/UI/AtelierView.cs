using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using ChromaJigsaw.Audio;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    // The Atelier — Settings screen. Lives in Settings.unity (scene index 3).
    // All player-facing labels use "Sensory Ambience" / "Visual Clarity" — never "accessibility".
    public class AtelierView : MonoBehaviour
    {
        [Header("Navigation")]
        [SerializeField] private Button _backButton;

        // ── Section 1: Sensory Ambience ──────────────────────────────────────
        [Header("Sensory Ambience")]
        [SerializeField] private Slider _ambientVolumeSlider;
        [SerializeField] private Toggle _tactileFeedbackToggle;
        [SerializeField] private Toggle _focusModeToggle;

        // ── Section 2: Visual Clarity ────────────────────────────────────────
        [Header("Visual Clarity")]
        [SerializeField] private Toggle _highContrastToggle;
        [SerializeField] private Toggle _grandInterfaceToggle;
        [SerializeField] private Slider _artworkBrightnessSlider;  // range 0.5–1.5

        // ── Section 3: The Zen Pass ──────────────────────────────────────────
        [Header("Zen Pass")]
        [SerializeField] private TextMeshProUGUI _zenPassStatusText;   // "Active Sanctuary Member" / "Discover the Sanctuary"
        [SerializeField] private TextMeshProUGUI _zenPassButtonLabel;  // "MANAGE" / "UPGRADE"
        [SerializeField] private Button          _zenPassButton;
        [SerializeField] private ZenPassView     _zenPassView;

        // ── Section 4: Archive & Atelier ─────────────────────────────────────
        [Header("Archive & Atelier")]
        [SerializeField] private Button _restorePurchasesButton;
        [SerializeField] private Button _privacyButton;
        [SerializeField] private Button _supportButton;

        private const string PrivacyUrl = "https://yourstudio.com/privacy";   // replaced in B-13
        private const string SupportUrl = "https://yourstudio.com/support";   // replaced in B-13

        private void Awake()
        {
            if (_artworkBrightnessSlider != null)
            {
                _artworkBrightnessSlider.minValue = 0.5f;
                _artworkBrightnessSlider.maxValue = 1.5f;
            }

            _backButton?.onClick.AddListener(OnBack);

            _ambientVolumeSlider?.onValueChanged.AddListener(OnAmbientVolumeChanged);
            _tactileFeedbackToggle?.onValueChanged.AddListener(OnTactileFeedbackChanged);
            _focusModeToggle?.onValueChanged.AddListener(OnFocusModeChanged);

            _highContrastToggle?.onValueChanged.AddListener(OnHighContrastChanged);
            _grandInterfaceToggle?.onValueChanged.AddListener(OnGrandInterfaceChanged);
            _artworkBrightnessSlider?.onValueChanged.AddListener(OnArtworkBrightnessChanged);

            _zenPassButton?.onClick.AddListener(OnZenPassTapped);
            _restorePurchasesButton?.onClick.AddListener(OnRestorePurchases);
            _privacyButton?.onClick.AddListener(() => Application.OpenURL(PrivacyUrl));
            _supportButton?.onClick.AddListener(() => Application.OpenURL(SupportUrl));
        }

        private void OnEnable()
        {
            AnalyticsManager.Instance.LogEvent("atelier_opened");
            var data = SaveManager.Instance.Data;

            // Sync sliders and toggles to saved values.
            _ambientVolumeSlider?.SetValueWithoutNotify(AudioManager.Instance.GetSavedVolume(AudioChannel.Music));
            _tactileFeedbackToggle?.SetIsOnWithoutNotify(data.tactileFeedback);
            _focusModeToggle?.SetIsOnWithoutNotify(data.focusMode);
            _highContrastToggle?.SetIsOnWithoutNotify(data.highContrast);
            _grandInterfaceToggle?.SetIsOnWithoutNotify(data.grandInterface);
            _artworkBrightnessSlider?.SetValueWithoutNotify(data.artworkBrightness);

            RefreshZenPassCard();
        }

        // ── Navigation ───────────────────────────────────────────────────────

        private void OnBack() => SceneManager.LoadScene(SceneNames.MainMenu);

        // ── Sensory Ambience ─────────────────────────────────────────────────

        private void OnAmbientVolumeChanged(float value)
        {
            AudioManager.Instance.SetVolume(AudioChannel.Music, value);
            AudioManager.Instance.SetVolume(AudioChannel.SFX,   value);
            AnalyticsManager.Instance.LogAccessibilityChanged("ambient_volume", value);
        }

        private void OnTactileFeedbackChanged(bool enabled)
        {
            AccessibilityService.Instance.SetTactileFeedback(enabled);
            AnalyticsManager.Instance.LogAccessibilityChanged("tactile_feedback", enabled);
        }

        private void OnFocusModeChanged(bool enabled)
        {
            AccessibilityService.Instance.SetFocusMode(enabled);
            AnalyticsManager.Instance.LogAccessibilityChanged("focus_mode", enabled);
        }

        // ── Visual Clarity ───────────────────────────────────────────────────

        private void OnHighContrastChanged(bool enabled)
        {
            AccessibilityService.Instance.SetHighContrast(enabled);
            AnalyticsManager.Instance.LogAccessibilityChanged("high_contrast", enabled);
        }

        private void OnGrandInterfaceChanged(bool enabled)
        {
            AccessibilityService.Instance.SetGrandInterface(enabled);
            AnalyticsManager.Instance.LogAccessibilityChanged("grand_interface", enabled);
        }

        private void OnArtworkBrightnessChanged(float value)
        {
            AccessibilityService.Instance.SetArtworkBrightness(value);
            AnalyticsManager.Instance.LogAccessibilityChanged("artwork_brightness", value);
        }

        // ── Zen Pass ─────────────────────────────────────────────────────────

        private void RefreshZenPassCard()
        {
            bool isZenPass = ZenPassService.Instance != null && ZenPassService.Instance.IsZenPass;

            if (_zenPassStatusText != null)
                _zenPassStatusText.text = isZenPass ? "Active Sanctuary Member" : "Discover the Sanctuary";

            if (_zenPassButtonLabel != null)
                _zenPassButtonLabel.text = isZenPass ? "MANAGE" : "UPGRADE";
        }

        private void OnZenPassTapped()
        {
            if (_zenPassView != null)
                _zenPassView.Show("settings", RefreshZenPassCard);
        }

        // ── Archive & Atelier ────────────────────────────────────────────────

        private void OnRestorePurchases()
        {
            IAPManager.Instance.RestorePurchases();
            AnalyticsManager.Instance.LogEvent("iap_restored",
                new Dictionary<string, object> { { "products_restored", 0 } });
        }
    }
}
