using HootyBird.JigsawPuzzleEngine.Model;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.ScriptableObjects
{
    /// <summary>
    /// ScriptableObject wrapper for <see cref="Model.Category"/> object
    /// </summary>
    [CreateAssetMenu(fileName = "CategoryAsset", menuName = "JigsawPuzzle/Create Category Asset")]
    public class CategoryObject : ScriptableObject
    {
        [SerializeField]
        private Category categoryObject;

        public string Name => name;

        public Category Category => categoryObject;
    }
}
