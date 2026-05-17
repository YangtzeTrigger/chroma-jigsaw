using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Repositories
{
    [CreateAssetMenu(fileName = "CategoriesRepository", menuName = "JigsawPuzzle/Create Categories Repository")]
    public class CategoriesRepository : ScriptableObject
    {
        [SerializeField]
        private List<CategoryObject> categories;

        public List<CategoryObject> Categories => categories;

        public PuzzleInfoObject FindPuzzleInfoById(string id)
        {
            return Categories
                .SelectMany(categoryObject => categoryObject.Category.Puzzles)
                .FirstOrDefault(puzzleObject => puzzleObject.Id == id);
        }

        public PuzzleInfoObject GetRandomPuzzleInfoObject()
        {
            Category randomCategory = categories[Random.Range(0, categories.Count)].Category;
            return randomCategory.Puzzles[Random.Range(0, randomCategory.Puzzles.Count)];
        }
    }
}
