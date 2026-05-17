using System;
using HootyBird.JigsawPuzzleEngine.Gameplay;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// Actual puzzle data used by <see cref="Puzzle.Initialize(PuzzleData, UnityEngine.Texture, Services.SavedGameData, float)"/> to 
    /// create new puzzle.
    /// </summary>
    [Serializable]
    public class PuzzleData
    {
        public Polygon[] polygons;
        public int rows;
        public int columns;
        public string puzzleId;
        public string settingsId;
    }
}
