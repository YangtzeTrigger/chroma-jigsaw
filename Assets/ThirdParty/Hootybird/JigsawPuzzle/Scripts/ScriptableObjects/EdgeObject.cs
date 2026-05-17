using HootyBird.JigsawPuzzleEngine.Model;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.ScriptableObjects
{
    /// <summary>
    /// Scriptable object wrapper for <see cref="Edge"/> object.
    /// Allows user to modify edge points using graphical interface.
    /// </summary>
    [CreateAssetMenu(fileName = "EdgeObject", menuName = "JigsawPuzzle/Create Edge Asset")]
    public class EdgeObject : ScriptableObject
    {
        public Edge edge;

#if UNITY_EDITOR
        // Validation is for editor edges only.
        public void OnValidate()
        {
            foreach (BezierPoint point in edge.points)
            {
                point.Validate();
            }
        }
#endif
    }
}
