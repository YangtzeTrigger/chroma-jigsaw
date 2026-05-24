using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using ChromaJigsaw.Audio;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class GalleryController : MonoBehaviour
    {
        [SerializeField] private GameObject           _panelPackGrid;
        [SerializeField] private GalleryWallView      _galleryWallView;
        [SerializeField] private ArtworkSpotlightView _spotlightView;
        [SerializeField] private PackRegistry         _registry;
        [SerializeField] private PackCardView         _cardPrefab;
        [SerializeField] private Transform            _gridContainer;

        private RectTransform _gridRT;
        private RectTransform _wallRT;

        private readonly Stack<Action>    _backStack = new();
        private readonly List<PackCardView> _cards   = new();

        private void Start()
        {
            _gridRT = _panelPackGrid.GetComponent<RectTransform>();
            _wallRT = _galleryWallView.GetComponent<RectTransform>();
            IAPManager.OnPackPurchased += OnPackUnlocked;
            PopulateGrid();
            ShowL1();
        }

        private void OnDestroy()
        {
            IAPManager.OnPackPurchased -= OnPackUnlocked;
        }

        private void OnPackUnlocked(string packId)
        {
            AudioManager.Instance.Play(SFXType.Reward);
            PopulateGrid();
        }

        private void PopulateGrid()
        {
            if (_registry == null || _cardPrefab == null || _gridContainer == null) return;

            foreach (var card in _cards)
                if (card != null) Destroy(card.gameObject);
            _cards.Clear();

            foreach (var config in _registry.packs)
            {
                var result = PackUnlockService.Instance.Evaluate(config, out string subLabel);
                int done   = GetCompletedCount(config.packId);
                var card   = Instantiate(_cardPrefab, _gridContainer);
                card.Populate(config, result, subLabel, done, OpenPack);
                _cards.Add(card);
            }
        }

        private static int GetCompletedCount(string packId)
        {
            var p = SaveManager.Instance.Data.packs.Find(x => x.packId == packId);
            return p?.completedImageIds?.Count ?? 0;
        }

        public void ShowL1()
        {
            _panelPackGrid.SetActive(true);
            if (_galleryWallView != null)
                _galleryWallView.gameObject.SetActive(false);
            AnalyticsManager.Instance.LogScreenView("gallery_landing");
        }

        public void OpenPack(string packId)
        {
            var config = _registry != null ? System.Array.Find(_registry.packs, p => p.packId == packId) : null;
            var result = config != null
                ? PackUnlockService.Instance.Evaluate(config, out _)
                : PackUnlockResult.Owned;
            AnalyticsManager.Instance.LogEvent("pack_opened", new Dictionary<string, object>
            {
                { "pack_id",     packId                      },
                { "pack_name",   config?.packName ?? ""      },
                { "unlock_type", result.ToString().ToLower() }
            });
            _gridRT.DOKill();
            _gridRT.DOAnchorPosY(_gridRT.anchoredPosition.y + 60f, 0.3f)
                   .SetEase(Ease.InCubic)
                   .OnComplete(() => _panelPackGrid.SetActive(false));

            _galleryWallView.gameObject.SetActive(true);
            _wallRT.DOKill();
            _wallRT.anchoredPosition = new Vector2(_wallRT.anchoredPosition.x, _wallRT.anchoredPosition.y - 60f);
            _wallRT.DOAnchorPosY(_wallRT.anchoredPosition.y + 60f, 0.3f).SetEase(Ease.OutCubic);
            _galleryWallView.Load(packId, this);
            _backStack.Push(BackToL1);
            AnalyticsManager.Instance.LogScreenView("gallery_wall");
        }

        public void OpenArtwork(string packId, string imageId)
        {
            _spotlightView.Open(packId, imageId);
            _backStack.Push(_spotlightView.Close);
            AnalyticsManager.Instance.LogScreenView("artwork_spotlight");
            AnalyticsManager.Instance.LogEvent("artwork_spotlight_opened", new Dictionary<string, object>
            {
                { "pack_id",  packId  },
                { "image_id", imageId }
            });
        }

        public void NavigateBack()
        {
            if (_backStack.Count > 0)
                _backStack.Pop()?.Invoke();
        }

        private void BackToL1()
        {
            _panelPackGrid.SetActive(true);
            _gridRT.DOKill();
            _gridRT.DOAnchorPosY(_gridRT.anchoredPosition.y - 60f, 0.3f).SetEase(Ease.OutCubic);

            _wallRT.DOKill();
            _wallRT.DOAnchorPosY(_wallRT.anchoredPosition.y - 60f, 0.3f)
                   .SetEase(Ease.InCubic)
                   .OnComplete(() => _galleryWallView.gameObject.SetActive(false));
        }
    }
}
