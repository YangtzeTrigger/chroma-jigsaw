using TMPro;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Puzzle progress widget.
    /// </summary>
    public class PuzzleProgressWidget : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text label;

        public void SetValue(int value)
        {
            label.text = $"{value}%";
        }
    }
}
