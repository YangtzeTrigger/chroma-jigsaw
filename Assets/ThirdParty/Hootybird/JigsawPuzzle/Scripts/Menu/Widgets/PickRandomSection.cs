using HootyBird.JigsawPuzzleEngine.Repositories;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Part of home tab overlay. Picks a <see cref="Settings.InternalAppSettings.HomeTabRandomPuzzleCount"/> number of puzzles to choose from.
    /// Only those that are not part of <see cref="ContinuePlayingSection"/> are available to choose from.
    /// Meaning if all puzzles have save files, this section will be empty.
    /// </summary>
    public class PickRandomSection : MenuWidget
    {
        [SerializeField]
        private PlayRadomWidget playRandomPrefab;
        [SerializeField]
        private RectTransform widgetsParent;

        private List<PlayRadomWidget> widgets = new List<PlayRadomWidget>();

        public override void UpdateWidget()
        {
            // Collect all saved files.
            List<SavedGameData> savedGamesData = SaveGameService.GetAllSavedGames();

            // Collect all possible puzzles.
            var allPuzzles = DataHandler.Instance.CategoryRepository.Categories
                .SelectMany(categoryObject => categoryObject.Category.Puzzles)
                .ToDictionary(
                    // Puzzle info.
                    puzzleInfoObject => puzzleInfoObject, 
                    // And puzzle settings.
                    puzzleInfoObject => new List<PuzzleSettingsObject>(puzzleInfoObject.Options));

            // For each saved game, remove entry from all available puzzles.
            foreach (SavedGameData savedGameData in savedGamesData)
            {
                PuzzleInfoObject puzzleInfo = allPuzzles.Keys.First(puzzleInfo => puzzleInfo.Id.Equals(savedGameData.puzzleId));

                // Remove this game from list of available ones.
                allPuzzles[puzzleInfo]
                    .RemoveAll(puzzleSettingsObject => puzzleSettingsObject.PuzzleSettings.id.Equals(savedGameData.settingsId));

                // If there's no available puzzle options in this puzzle, remove it.
                if (allPuzzles[puzzleInfo].Count == 0)
                {
                    allPuzzles.Remove(puzzleInfo);
                }
            }

            // Disable all widget before assgning new data.
            foreach (PlayRadomWidget widget in widgets)
            {
                widget.gameObject.SetActive(false);
            }

            // If no puzzles are available, PickRandom section is disabled.
            if (allPuzzles.Count == 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                // Otherwise add entries.
                gameObject.SetActive(true);

                for (int index = 0; index < Settings.InternalAppSettings.HomeTabRandomPuzzleCount; index++)
                {
                    if (allPuzzles.Count > 0)
                    {
                        // Pick random puzzleInfo.
                        var puzzleInfo = allPuzzles.Keys.OrderBy(puzzleObject => Random.value).First();
                        PlayRadomWidget widget;

                        // If not enough widgets, add one.
                        if (index >= widgets.Count)
                        {
                            widgets.Add(widget = Instantiate(playRandomPrefab, widgetsParent));
                        }
                        else
                        {
                            widget = widgets[index];
                            widget.gameObject.SetActive(true);
                        }

                        // Update it with first puzzle setting from puzzleInfo object.
                        widget.UpdateData(puzzleInfo, allPuzzles[puzzleInfo][0].PuzzleSettings);

                        // Remove puzzle setting and puzzleInfo if needed.
                        allPuzzles[puzzleInfo].RemoveAt(0);
                        if (allPuzzles[puzzleInfo].Count == 0)
                        {
                            allPuzzles.Remove(puzzleInfo);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }
    }
}
