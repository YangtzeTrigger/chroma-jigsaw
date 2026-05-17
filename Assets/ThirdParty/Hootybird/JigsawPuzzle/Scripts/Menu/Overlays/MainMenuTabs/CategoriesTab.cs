using HootyBird.JigsawPuzzleEngine.Repositories;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Puzzle categories tab.
    /// </summary>
    public class CategoriesTab : TabOverlay
    {
        [SerializeField]
        private CategoryWidget categoryPrefab;
        [SerializeField]
        private RectTransform categoriesParent;

        protected override void Start()
        {
            base.Start();

            LoadCategories();
        }

        /// <summary>
        /// Load categories prefabs.
        /// </summary>
        private void LoadCategories()
        {
            foreach (CategoryWidget item in widgets)
            {
                Destroy(item.gameObject);
            }

            foreach (CategoryObject categoryObject in DataHandler.Instance.CategoryRepository.Categories)
            {
                CategoryWidget item = Instantiate(categoryPrefab, categoriesParent);
                item.SetCategoryData(categoryObject);

                widgets.Add(item);
            }
        }
    }
}
