using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    public class CategoryOverlay : MenuOverlay
    {
        [SerializeField]
        private TMP_Text categoryName;
        [SerializeField]
        private PuzzleItemWidget puzzleWidgetPrefab;
        [SerializeField]
        private RectTransform puzzlesParent;

        private CategoryObject categoryObject;
        private List<PuzzleItemWidget> puzzleWidgets = new List<PuzzleItemWidget>();

        public void OpenCategory(CategoryObject categoryObject)
        {
            this.categoryObject = categoryObject;

            LoadCategory();
            MenuController.OpenOverlay(this);
        }

        private void LoadCategory()
        {
            categoryName.text = categoryObject.name;

            foreach (PuzzleItemWidget puzzleItemWidget in puzzleWidgets)
            {
                Destroy(puzzleItemWidget.gameObject);
            }
            puzzleWidgets.Clear();

            foreach (PuzzleInfoObject puzzleInfo in categoryObject.Category.Puzzles)
            {
                PuzzleItemWidget puzzleItemWidget = Instantiate(puzzleWidgetPrefab, puzzlesParent);
                puzzleItemWidget.SetPuzzleInfo(puzzleInfo);

                puzzleWidgets.Add(puzzleItemWidget);
            }
        }
    }
}
