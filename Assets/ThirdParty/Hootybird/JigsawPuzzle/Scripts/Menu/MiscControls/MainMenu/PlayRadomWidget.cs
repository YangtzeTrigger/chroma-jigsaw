using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Starts a puzzle with specified info when clicked.
    /// </summary>
    public class PlayRadomWidget : MonoBehaviour
    {
        [SerializeField]
        protected RawImage icon;
        [SerializeField]
        protected TMP_Text puzzleSettingLabel;

        protected AspectRatioFitter iconAspectRatioFitter;
        protected RectTransform rectTransform;
        protected Button button;
        protected PuzzleInfoObject puzzleInfoObject;
        protected PuzzleSettings puzzleSetting;

        protected virtual bool LoadSavedGame { get; } = false;

        protected virtual void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            rectTransform = GetComponent<RectTransform>();
            iconAspectRatioFitter = icon.GetComponent<AspectRatioFitter>();
        }

        public virtual void UpdateData(PuzzleInfoObject puzzleInfoObject, PuzzleSettings puzzleSetting)
        {
            this.puzzleInfoObject = puzzleInfoObject;
            this.puzzleSetting = puzzleSetting;

            icon.texture = puzzleInfoObject.PuzzleTexture;
            iconAspectRatioFitter.aspectRatio = (float)icon.texture.width / icon.texture.height;
            puzzleSettingLabel.text = $"{puzzleSetting.columns}*{puzzleSetting.rows}";

            UpdateIconSize();
        }

        private void UpdateIconSize()
        {
            float imageAspect = (float)puzzleInfoObject.PuzzleTexture.width / puzzleInfoObject.PuzzleTexture.height;
            float parentAspect = rectTransform.sizeDelta.x / rectTransform.sizeDelta.y;

            if (parentAspect > imageAspect)
            {
                icon.rectTransform.sizeDelta = new Vector2(
                    rectTransform.sizeDelta.x,
                    rectTransform.sizeDelta.x / imageAspect);
            }
            else
            {
                icon.rectTransform.sizeDelta = new Vector2(
                    rectTransform.sizeDelta.y / imageAspect,
                    rectTransform.sizeDelta.y);
            }
        }

        private void OnClick()
        {
            GameplayMenuController gameplayMenuController =
                MenuController.GetMenuController<GameplayMenuController>(Settings.InternalAppSettings.GameplayMenuControllerName);

            AudioService.Instance.PlaySfx("menu-click", .4f);

            gameplayMenuController.SetActive(true);
            gameplayMenuController.StartPuzzle(new PuzzleSettingsPackage()
            {
                puzzleTexture = puzzleInfoObject.PuzzleTexture,
                puzzleSettings = puzzleSetting,
                seed = puzzleInfoObject.Seed,
                puzzleId = puzzleInfoObject.Id,
            }, LoadSavedGame);
        }
    }
}
