using System;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// Package containing <see cref="PuzzleSettings"/> (which are used by <see cref="Tools.PuzzleFactory"/> to generate new PuzzleData.
    /// Can be converted to <see cref="PuzzleDataPackage"> using <see cref="Tools.PuzzleFactory.ToPuzzleDataPackage"/>. 
    /// </summary>
    [Serializable]
    public class PuzzleSettingsPackage
    {
        public Texture puzzleTexture;
        public PuzzleSettings puzzleSettings;
        public int seed;
        public string puzzleId;
    }
}
