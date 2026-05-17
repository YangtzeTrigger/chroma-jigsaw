using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// <see cref="PuzzlePiecesPanel"/> placeholder that indicates where <see cref="PuzzleOverlay.ActivePiece"/> will be placed if returned
    /// under pieces panel.
    /// </summary>
    public class PuzzlePiecesPanelPlaceholder : MonoBehaviour
    {
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();     
        }

        public void SetSize(float size)
        {
            rectTransform.sizeDelta = new Vector2(size, size);
        }

        public void SetActive(bool state)
        {
            gameObject.SetActive(state);
        }
    }
}
