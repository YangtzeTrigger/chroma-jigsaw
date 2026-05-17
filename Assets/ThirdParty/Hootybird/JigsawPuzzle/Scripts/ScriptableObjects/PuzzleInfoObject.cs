using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.ScriptableObjects
{
    /// <summary>
    /// Represents a puzzle in <see cref="CategoryObject"/> object.
    /// Each puzzleInfo object have texture and puzzle settings attached to it.
    /// </summary>
    [CreateAssetMenu(fileName = "PuzzleInfo", menuName = "JigsawPuzzle/Create Puzzle Info Asset")]
    public class PuzzleInfoObject : ScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        private string id;
        [SerializeField]
        private Texture puzzleTexture;
        [SerializeField]
        private List<PuzzleSettingsObject> options;
        [SerializeField]
        private int seed;

        /// <summary>
        /// Used for save game functionality.
        /// </summary>
        public string Id => id;

        public Texture PuzzleTexture => puzzleTexture;

        public List<PuzzleSettingsObject> Options => options;

        /// <summary>
        /// Same seed value results in same puzzle data.
        /// </summary>
        public int Seed => seed;

        public static PuzzleInfoObject Create(
            Texture puzzleTexture,
            params PuzzleSettingsObject[] options)
        {
            PuzzleInfoObject newPuzzleInfo = CreateInstance<PuzzleInfoObject>();

            newPuzzleInfo.puzzleTexture = puzzleTexture;
            newPuzzleInfo.options = new List<PuzzleSettingsObject>(options.Where(value => value != null));
            newPuzzleInfo.ResetSeed();
            newPuzzleInfo.ResetID();

            return newPuzzleInfo;
        }

        public PuzzleSettingsObject FindSettings(string settingsId)
        {
            return options.Find(settings => settings.PuzzleSettings.id.Equals(settingsId));
        }

        [ContextMenu("Reset Seed")]
        private void ResetSeed()
        {
            seed = UnityEngine.Random.Range(0, int.MaxValue);
        }

        [ContextMenu("Reset ID")]
        private void ResetID()
        {
            id = Guid.NewGuid().ToString();
        }
    }
}
