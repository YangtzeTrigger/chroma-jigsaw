using UnityEngine;
using UnityEngine.UI;
using ChromaJigsaw.Audio;

namespace ChromaJigsaw.UI
{
    // Settings screen (The Atelier). Wire Inspector slots and assign to the Settings scene root.
    public class AtelierView : MonoBehaviour
    {
        [Header("Audio")]
        [SerializeField] private Slider _ambientVolumeSlider;  // controls Music + SFX together

        [Header("Accessibility")]
        [SerializeField] private Toggle _tactileFeedbackToggle;
        [SerializeField] private Toggle _focusModeToggle;

        private void Awake()
        {
            if (_ambientVolumeSlider != null)
            {
                _ambientVolumeSlider.onValueChanged.AddListener(OnAmbientVolumeChanged);
            }

            if (_tactileFeedbackToggle != null)
                _tactileFeedbackToggle.onValueChanged.AddListener(OnTactileFeedbackChanged);

            if (_focusModeToggle != null)
                _focusModeToggle.onValueChanged.AddListener(OnFocusModeChanged);
        }

        private void OnEnable()
        {
            // Sync sliders to saved values when the screen opens.
            if (_ambientVolumeSlider != null && AudioManager.Instance != null)
                _ambientVolumeSlider.SetValueWithoutNotify(AudioManager.Instance.GetSavedVolume(AudioChannel.Music));
        }

        private void OnAmbientVolumeChanged(float value)
        {
            AudioManager.Instance.SetVolume(AudioChannel.Music, value);
            AudioManager.Instance.SetVolume(AudioChannel.SFX,   value);
        }

        private void OnTactileFeedbackChanged(bool enabled)
        {
            // TODO: Android Vibrator API stub — wire in B-10 Accessibility
#if UNITY_ANDROID && !UNITY_EDITOR
            // AndroidJavaClass vibrator = new AndroidJavaClass("android.os.Vibrator");
            // vibrator.Call("vibrate", enabled ? 50L : 0L);
#endif
        }

        private void OnFocusModeChanged(bool enabled)
        {
            // TODO: wire to GameManager.FocusMode when HUD hide is implemented
        }
    }
}
