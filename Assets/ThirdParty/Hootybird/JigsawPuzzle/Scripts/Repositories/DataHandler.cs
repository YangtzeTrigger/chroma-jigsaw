using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Repositories
{
    /// <summary>
    /// Repositories container.
    /// </summary>
    public class DataHandler : MonoBehaviour
    {
        public static DataHandler Instance { get; private set; }

        [SerializeField]
        private CategoriesRepository categoriesRepository;
        [SerializeField]
        private UIRepository uiRepository;
        [SerializeField]
        private AudioRepository audioRepository;
        
        public CategoriesRepository CategoryRepository => categoriesRepository;

        public UIRepository UIRepository => uiRepository;

        public AudioRepository AudioRepository => audioRepository;

        private void Awake()
        {
            if (Instance != null)
            {
                DestroyImmediate(gameObject);
                return;
            }

            Instance = this;
        }
    }
}
