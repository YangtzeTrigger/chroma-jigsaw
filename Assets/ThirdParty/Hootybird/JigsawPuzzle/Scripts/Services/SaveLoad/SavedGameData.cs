using System;

namespace HootyBird.JigsawPuzzleEngine.Services
{
    /// <summary>
    /// Collection of <see cref="PuzzlePieceSaveData"/> data used to restore puzzle state.
    /// </summary>
    [Serializable]
    public class SavedGameData
    {
        /// <summary>
        /// Puzzle id used to locate puzzle this save data.
        /// </summary>
        public string puzzleId;

        /// <summary>
        /// Puzzle settings to locate settings faile used with this save data.
        /// </summary>
        public string settingsId;

        public PuzzlePieceSaveData[] data;
    }
}
