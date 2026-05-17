using System;
using UnityEngine;
using HootyBird.JigsawPuzzleEngine.Gameplay;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// Package containing PuzzleData and texture to use with it.
    /// Used by <see cref="Puzzle.Initialize(PuzzleDataPackage, Services.SavedGameData, float)"/> to start new puzzle.
    /// </summary>
    [Serializable]
    public class PuzzleDataPackage
    {
        public Texture puzzleTexture;
        public PuzzleData puzzleData;
    }
}
