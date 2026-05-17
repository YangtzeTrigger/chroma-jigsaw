using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;
using HootyBird.JigsawPuzzleEngine.Tween;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Toggle to control different setting.
    /// </summary>
    public class SettingsOptionWidget : MenuWidget
    {
        [SerializeField]
        private SettingsOptions setting;
        [SerializeField]
        private TweenBase toggleTween;
        [SerializeField]
        private Button infoButton;

        private Button button;

        protected override void Awake()
        {
            base.Awake();

            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            SettingsService.OnSettingsChanged += OnSettingChanged;

            if (infoButton)
            {
                infoButton.onClick.AddListener(OnInfoClick);
            }
        }

        public override void UpdateWidget()
        {
            toggleTween.Progress(SettingsService.GetSettingValue(setting) ? 0f : 1f, PlaybackDirection.FORWARD);
        }

        private void OnClick()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);
            SettingsService.SetSettingValue(setting, !SettingsService.GetSettingValue(setting));
        }

        private void OnInfoClick()
        {
            PromptOverlay infoOverlay = MenuOverlay.MenuController.GetOverlay<PromptOverlay>();

            infoOverlay.SetPromptData(
                GetTitleForSetting(),
                GetDescriptionForSetting(),
                "Close"
                );
            MenuOverlay.MenuController.OpenOverlay(infoOverlay);
        }

        private void OnSettingChanged(SettingsOptions setting, bool value)
        {
            if (this.setting != setting || !MenuOverlay.IsCurrent)
            {
                return;
            }

            if (value)
            {
                toggleTween.PlayBackward(false);
            }
            else
            {
                toggleTween.PlayForward(false);
            }
        }

        private string GetTitleForSetting()
        {
            switch (setting)
            {
                case SettingsOptions.FreeSnap:
                    return "Sticky Puzzle";

                case SettingsOptions.Rotation:
                    return "Rotate Puzzle";

                default:
                    return "title";
            }
        }

        private string GetDescriptionForSetting()
        {
            switch (setting)
            {
                case SettingsOptions.FreeSnap:
                    return "When the puzzle piece is placed in the correct location, it won't need a neighbor to snap to the board.";

                case SettingsOptions.Rotation:
                    return "Rotates puzzle pieces when you click on them.";

                default:
                    return "title";
            }
        }
    }
}