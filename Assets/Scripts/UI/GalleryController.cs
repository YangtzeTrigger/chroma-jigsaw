using System;
using System.Collections.Generic;
using UnityEngine;
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

        private readonly Stack<Action>    _backStack = new();
        private readonly List<PackCardView> _cards   = new();

        private void Start()
        {
            PopulateGrid();
            ShowL1();
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
            _panelPackGrid.SetActive(false);
            _galleryWallView.gameObject.SetActive(true);
            _galleryWallView.Load(packId, this);
            _backStack.Push(BackToL1);
            AnalyticsManager.Instance.LogScreenView("gallery_wall");
        }

        public void OpenArtwork(string packId, string imageId)
        {
            _spotlightView.Open(packId, imageId);
            _backStack.Push(_spotlightView.Close);
            AnalyticsManager.Instance.LogScreenView("artwork_spotlight");
        }

        public void NavigateBack()
        {
            if (_backStack.Count > 0)
                _backStack.Pop()?.Invoke();
        }

        private void BackToL1()
        {
            _galleryWallView.gameObject.SetActive(false);
            _panelPackGrid.SetActive(true);
        }
    }
}
