using HootyBird.JigsawPuzzleEngine.Model;
using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject wrapper for PuzzleSettings.
    /// </summary>
    [CreateAssetMenu(fileName = "PuzzleSettings", menuName = "JigsawPuzzle/Create Puzzle Settings Asset")]
    public class PuzzleSettingsObject : ScriptableObject
    {
        [SerializeField]
        private PuzzleSettings puzzleSettings;

#if UNITY_EDITOR
        [SerializeField]
        private List<EdgeObject> edgeOptions;
#endif

        public PuzzleSettings PuzzleSettings => puzzleSettings;
    }
}
