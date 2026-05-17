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
    /// Widget to display different settings ontop of <see cref="PuzzleItemWidget"/> widget.
    /// </summary>
    public class PuzzleSettingWidget : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text label;

        private Button button;
        private PuzzleItemWidget puzzleItemWidget;
        private MenuController menuController;

        private PuzzleSettingsObject puzzleSettingsObject;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OpenPuzzle);

            puzzleItemWidget = GetComponentInParent<PuzzleItemWidget>();
            menuController = GetComponentInParent<MenuController>();
        }

        public void SetPuzzleSettings(PuzzleSettingsObject puzzleSettingsObject)
        {
            this.puzzleSettingsObject = puzzleSettingsObject;

            label.text = $"{puzzleSettingsObject.PuzzleSettings.columns}*{puzzleSettingsObject.PuzzleSettings.rows}";
        }

        public void OpenPuzzle()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);

            // If save file is present for this option, ask player if they want to continue.
            if (SaveGameService.HaveSaveFile(
                PuzzleTools.CombineSettingsWithId(puzzleSettingsObject.PuzzleSettings.id, puzzleItemWidget.PuzzleId)))
            {
                // Prompt player with "continue" prompt.
                PromptOverlay loadPrompt = menuController.GetOverlay<PromptOverlay>();
                loadPrompt.SetPromptData(
                    "Load Game?",
                    null,
                    "Yes",
                    "No",
                    () => LoadPuzzle(true),
                    () => LoadPuzzle(false));
                loadPrompt.CloseOnAccept();
                loadPrompt.CloseOnReject();

                menuController.OpenOverlay(loadPrompt);
            }
            else
            {
                // Otherwise just open without loading.
                LoadPuzzle(false);
            }
        }

        private void LoadPuzzle(bool tryLoad)
        {
            //Find the gameplay menu controller and start the puzzle.
            GameplayMenuController gameplayMenuController =
                MenuController.GetMenuController<GameplayMenuController>(Settings.InternalAppSettings.GameplayMenuControllerName);

            gameplayMenuController.SetActive(true);
            gameplayMenuController.StartPuzzle(new PuzzleSettingsPackage()
            {
                puzzleTexture = puzzleItemWidget.PuzzleInfo.PuzzleTexture,
                puzzleSettings = puzzleSettingsObject.PuzzleSettings,
                seed = puzzleItemWidget.PuzzleSeed,
                puzzleId = puzzleItemWidget.PuzzleId,
            },
            tryLoad);
        }
    }
}
