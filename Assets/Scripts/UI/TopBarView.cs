using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChromaJigsaw.UI
{
    public class TopBarView : MonoBehaviour
    {
        [SerializeField] private Button          _hamburgerButton;
        [SerializeField] private Button          _profileButton;
        [SerializeField] private TextMeshProUGUI _titleText;

        private void Awake()
        {
            if (_titleText != null)
                _titleText.text = "CHROMA JIGSAW";
        }
    }
}
