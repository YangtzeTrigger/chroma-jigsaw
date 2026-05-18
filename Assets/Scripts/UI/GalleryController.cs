using System;
using System.Collections.Generic;
using UnityEngine;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class GalleryController : MonoBehaviour
    {
        [SerializeField] private GameObject        _panelPackGrid;
        [SerializeField] private GalleryWallView   _galleryWallView;
        [SerializeField] private ArtworkSpotlightView _spotlightView;

        private readonly Stack<Action> _backStack = new();

        private void Start() => ShowL1();

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
