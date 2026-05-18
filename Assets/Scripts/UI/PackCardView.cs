using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.UI
{
    public class PackCardView : MonoBehaviour
    {
        [SerializeField] private RawImage            _artworkImage;
        [SerializeField] private Image               _frameBorder;
        [SerializeField] private Image               _spotlightHalo;
        [SerializeField] private Image               _veil;
        [SerializeField] private GameObject          _lockIcon;
        [SerializeField] private TextMeshProUGUI     _titleText;
        [SerializeField] private TextMeshProUGUI     _statusText;
        [SerializeField] private TextMeshProUGUI     _countText;
        [SerializeField] private Button              _button;

        private static readonly Color GoldBorder       = new Color(0.957f, 0.741f, 0.380f);  // #f4bd61
        private static readonly Color FrameBorder      = new Color(0.239f, 0.169f, 0.102f);  // #3d2b1a
        private static readonly Color OutlineVariant   = new Color(0.310f, 0.271f, 0.216f);  // #4f4537
        private static readonly Color OnSurfaceVariant = new Color(0.827f, 0.769f, 0.698f);  // #d3c4b2
        private static readonly Color VeilColor        = new Color(0.055f, 0.055f, 0.055f, 0.72f);

        private string        _packId;
        private Action<string> _onTap;
        private bool          _isCompleted;

        private const int TotalImages = 12;

        public void Populate(PackProgress pack, Texture2D artwork, Action<string> onTap)
        {
            _packId = pack.packId;
            _onTap  = onTap;

            int done  = pack.completedImageIds?.Count ?? 0;
            bool isLocked = !pack.isOwned;
            _isCompleted = done >= TotalImages;

            if (artwork != null)
                _artworkImage.texture = artwork;

            _titleText.text = pack.packId;  // display name wired in B-06
            _countText.text = $"{done}/{TotalImages}";

            if (isLocked)
                ApplyLockedState();
            else if (_isCompleted)
                ApplyCompletedState();
            else
                ApplyInProgressState();

            _button.onClick.RemoveAllListeners();
            if (!isLocked)
                _button.onClick.AddListener(HandleTap);
            _button.interactable = !isLocked;
        }

        private void ApplyCompletedState()
        {
            _frameBorder.color = GoldBorder;
            _spotlightHalo.gameObject.SetActive(true);
            _veil.gameObject.SetActive(false);
            _lockIcon.SetActive(false);
            _artworkImage.color = Color.gray;  // greyscale until tapped; swap for greyscale material in polish pass
            _statusText.text  = "COMPLETED";
            _statusText.color = GoldBorder;
            _countText.color  = OnSurfaceVariant;
        }

        private void ApplyInProgressState()
        {
            _frameBorder.color = FrameBorder;
            _spotlightHalo.gameObject.SetActive(false);
            _veil.gameObject.SetActive(false);
            _lockIcon.SetActive(false);
            _artworkImage.color = new Color(1f, 1f, 1f, 0.6f);
            _statusText.text  = "IN PROGRESS";
            _statusText.color = OnSurfaceVariant;
            _countText.color  = OnSurfaceVariant;
        }

        private void ApplyLockedState()
        {
            _frameBorder.color = OutlineVariant;
            _spotlightHalo.gameObject.SetActive(false);
            _veil.gameObject.SetActive(true);
            _veil.color = VeilColor;
            _lockIcon.SetActive(true);
            _artworkImage.color = new Color(1f, 1f, 1f, 0.3f);
            Color dimText = new Color(OnSurfaceVariant.r, OnSurfaceVariant.g, OnSurfaceVariant.b, 0.5f);
            _statusText.text  = "LOCKED";
            _statusText.color = dimText;
            _countText.text   = "0/12";
            _countText.color  = dimText;
            _titleText.color  = dimText;
        }

        private void HandleTap()
        {
            if (_isCompleted)
                StartCoroutine(ColourReveal());
            _onTap?.Invoke(_packId);
        }

        private IEnumerator ColourReveal()
        {
            // 700ms greyscale → full colour. DOTween: _artworkImage.DOColor(Color.white, 0.7f);
            float elapsed = 0f;
            Color start   = _artworkImage.color;
            while (elapsed < 0.7f)
            {
                elapsed += Time.deltaTime;
                _artworkImage.color = Color.Lerp(start, Color.white, elapsed / 0.7f);
                yield return null;
            }
            _artworkImage.color = Color.white;
        }
    }
}
