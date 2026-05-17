using System;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// Settings used by <see cref="Tools.PuzzleFactory.FromPuzzleSettings(PuzzleSettings)"/> to generate puzzle data from.
    /// </summary>
    [Serializable]
    public class PuzzleSettings
    {
        public string id;
        public int rows = 2;
        public int columns = 2;
        public Edge[] edgeOptions;

#if UNITY_EDITOR
        // This seed is editor only value.
        public int seed = 0;
#endif
    }
}
