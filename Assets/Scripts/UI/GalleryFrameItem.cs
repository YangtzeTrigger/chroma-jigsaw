using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ChromaJigsaw.UI
{
    public class GalleryFrameItem : MonoBehaviour
    {
        [SerializeField] private RawImage        _artwork;
        [SerializeField] private Image           _veil;
        [SerializeField] private Image           _frameBorder;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _pieceCountText;
        [SerializeField] private Button          _button;

        private static readonly Color FrameColor = new Color(0.239f, 0.169f, 0.102f);  // #3d2b1a
        private static readonly Color VeilColor  = new Color(0.055f, 0.055f, 0.055f, 0.72f);

        private Action<string, bool> _onTap;
        private string _imageId;
        private bool   _isCompleted;

        public void Setup(string imageId, bool isCompleted, int piecesSolved, Action<string, bool> onTap)
        {
            _imageId     = imageId;
            _isCompleted = isCompleted;
            _onTap       = onTap;

            _frameBorder.color = FrameColor;
            _titleText.text    = imageId;  // display name wired in B-06
            if (_pieceCountText != null)
                _pieceCountText.text = isCompleted ? "Complete" : $"{piecesSolved} pieces";

            _veil.gameObject.SetActive(!isCompleted);
            if (!isCompleted)
                _veil.color = VeilColor;

            _button.onClick.RemoveAllListeners();
            _button.interactable = isCompleted;
            if (isCompleted)
                _button.onClick.AddListener(HandleTap);
        }

        private void HandleTap() => _onTap?.Invoke(_imageId, _isCompleted);
    }
}
