using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Tools;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Services
{
    /// <summary>
    /// Service used to generate, save, and load puzzle data.
    /// </summary>
    public class SaveGameService
    {
        /// <summary>
        /// Creates <see cref="SavedGameData"/> from puzzle, and saves it to the disk.
        /// </summary>
        /// <param name="puzzle"></param>
        public static void SaveGame(Puzzle puzzle)
        {
            PuzzlePieceSaveData[] data = puzzle.PuzzlePieces
                // Save data about all pieces that are not inside pieces panel.
                .Where(piece => piece.State != PuzzlePieceState.None)
                // Extract cluster index.
                .Select(piece => new PuzzlePieceSaveData(piece, piece.Cluster != null ? puzzle.Clusters.IndexOf(piece.Cluster) : -1))
                .ToArray();

            SavedGameData savedGameData = new SavedGameData() { 
                data = data, 
                puzzleId = puzzle.PuzzleData.puzzleId,
                settingsId = puzzle.PuzzleData.settingsId,
            };

            // Check and combine path.
            CheckFolder();
            string path = Path.Combine(
                Application.persistentDataPath, 
                Settings.GameData.SaveFolderPath, 
                PuzzleTools.CombineSettingsWithId(savedGameData.settingsId, savedGameData.puzzleId));

            // Write save file.
            File.WriteAllText(path, JsonUtility.ToJson(savedGameData));
            Debug.Log($"Saved puzzle progress {savedGameData.puzzleId} to {path}");
        }

        /// <summary>
        /// Check if saveFile for given id is present on the disk.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool HaveSaveFile(string id)
        {
            string path = Path.Combine(Application.persistentDataPath, Settings.GameData.SaveFolderPath);
            if (!Directory.Exists(path))
            {
                return false;
            }

            return File.Exists(Path.Combine(path, id));
        }

        /// <summary>
        /// Load saved game data by id from the disk,
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static SavedGameData LoadSavedGameData(string id)
        {
            if (!HaveSaveFile(id))
            {
                return null;
            }

            string path = Path.Combine(Application.persistentDataPath, Settings.GameData.SaveFolderPath);
            return JsonUtility.FromJson<SavedGameData>(File.ReadAllText(Path.Combine(path, id)));
        }

        /// <summary>
        /// Delete saved game data by id.
        /// </summary>
        /// <param name="id"></param>
        public static void DeleteSavedGameData(string id)
        {
            if (HaveSaveFile(id))
            {
                File.Delete(Path.Combine(Application.persistentDataPath, Settings.GameData.SaveFolderPath, id));
            }
        }

        /// <summary>
        /// Get all saved games from the disk.
        /// </summary>
        /// <returns></returns>
        public static List<SavedGameData> GetAllSavedGames()
        {
            CheckFolder();

            string[] saveFiles = Directory.GetFiles(Path.Combine(Application.persistentDataPath, Settings.GameData.SaveFolderPath));

            return saveFiles
                .Select(filePath => JsonUtility.FromJson<SavedGameData>(File.ReadAllText(filePath)))
                .ToList();
        }

        private static void CheckFolder()
        {
            // Create folder if needed.
            string path = Path.Combine(Application.persistentDataPath, Settings.GameData.SaveFolderPath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}