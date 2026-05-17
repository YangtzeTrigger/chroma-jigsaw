using HootyBird.JigsawPuzzleEngine.Repositories;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Part of home tab overlay. Lets player continue puzzle from where they left it.
    /// </summary>
    public class ContinuePlayingSection : MenuWidget
    {
        [SerializeField]
        private ContinuePlayingWidget continueWidgetPrefab;
        [SerializeField]
        private RectTransform widgetsParent;

        private List<ContinuePlayingWidget> widgets = new List<ContinuePlayingWidget>();

        public override void UpdateWidget()
        {
            // Get all saved games.
            List<SavedGameData> savedGamesData = SaveGameService.GetAllSavedGames();

            // Check saved files for a valid settings option.
            int savedGameIndex = 0;
            while (savedGameIndex < savedGamesData.Count)
            {
                PuzzleInfoObject puzzleInfoObject = DataHandler.Instance.CategoryRepository
                    .FindPuzzleInfoById(savedGamesData[savedGameIndex].puzzleId);
                PuzzleSettingsObject targetSettings = puzzleInfoObject?.FindSettings(savedGamesData[savedGameIndex].settingsId);

                if (targetSettings == null)
                {
                    Debug.LogWarning($"Save file points to settings option that is not present on any puzzle. " +
                        $"Removing corrupt save file");
                    SaveGameService.DeleteSavedGameData(
                        PuzzleTools.CombineSettingsWithId(
                            savedGamesData[savedGameIndex].settingsId, 
                            savedGamesData[savedGameIndex].puzzleId));
                    savedGamesData.RemoveAt(savedGameIndex);
                }
                else
                {
                    savedGameIndex++;
                }
            }

            // If none availble, section is disabled.
            if (savedGamesData.Count == 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);

                // Add widgets if necessary.
                while (widgets.Count < savedGamesData.Count)
                {
                    widgets.Add(Instantiate(continueWidgetPrefab, widgetsParent));
                }

                // Update widgets data with saved game data.
                for (int index = 0; index < savedGamesData.Count; index++)
                {
                    if (index > savedGamesData.Count)
                    {
                        widgets[index].gameObject.SetActive(false);
                    }
                    else
                    {
                        SavedGameData savedGame = savedGamesData[index];

                        PuzzleInfoObject puzzleInfoObject = DataHandler.Instance.CategoryRepository.FindPuzzleInfoById(savedGame.puzzleId);
                        PuzzleSettingsObject targetSettings = puzzleInfoObject.FindSettings(savedGame.settingsId);

                        if (targetSettings == null)
                        {
                            Debug.LogWarning($"Save file points to settings option that is not present on any puzzle. " +
                                $"Removing corrupt save file");
                            SaveGameService.DeleteSavedGameData(PuzzleTools.CombineSettingsWithId(savedGame.settingsId, savedGame.puzzleId));

                            continue;
                        }

                        widgets[index].gameObject.SetActive(true);
                        widgets[index].UpdateData(
                            puzzleInfoObject,
                            targetSettings.PuzzleSettings,
                            savedGame.GetProgress(targetSettings.PuzzleSettings.columns, targetSettings.PuzzleSettings.rows));
                    }
                }
            }
        }
    }
}
