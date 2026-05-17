using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// Collection of puzzles. See <see cref="PuzzleInfoObject"/> for more.
    /// </summary>
    [Serializable]
    public class Category
    {
        [SerializeField]
        private List<PuzzleInfoObject> puzzles;

        public List<PuzzleInfoObject> Puzzles => puzzles;
    }
}
