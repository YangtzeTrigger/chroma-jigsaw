using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Widget for displaying a category under <see cref="CategoriesTab"/> tab.
    /// </summary>
    public class CategoryWidget : MenuWidget
    {
        [SerializeField]
        private TMP_Text nameLabel;
        [SerializeField]
        private TMP_Text progressLabel;
        [SerializeField]
        private RawImage icon;

        private AspectRatioFitter iconAspectRatioFitter;
        private Button button;
        private CategoryObject category;
        private PuzzleInfoObject puzzleInfoObject;

        protected override void Awake()
        {
            base.Awake();

            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);

            iconAspectRatioFitter = icon.GetComponent<AspectRatioFitter>();
        }

        public void SetCategoryData(CategoryObject categoryObject)
        {
            category = categoryObject;
            puzzleInfoObject = categoryObject.Category.Puzzles[0];
            icon.texture = puzzleInfoObject.PuzzleTexture;
            iconAspectRatioFitter.aspectRatio = (float)icon.texture.width / icon.texture.height;
        }

        public override void UpdateWidget()
        {
            nameLabel.text = category.Name;
        }

        private void OnClick()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);
            MenuOverlay.MenuController.GetOverlay<CategoryOverlay>().OpenCategory(category);
        }
    }
}
