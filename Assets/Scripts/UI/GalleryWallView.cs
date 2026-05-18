using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.UI
{
    public class GalleryWallView : MonoBehaviour
    {
        [SerializeField] private Transform          _framesContainer;
        [SerializeField] private GalleryFrameItem   _frameItemPrefab;
        [SerializeField] private TextMeshProUGUI    _packTitleText;
        [SerializeField] private TextMeshProUGUI    _subtitleText;

        private GalleryController          _controller;
        private string                     _packId;
        private readonly List<GalleryFrameItem> _frames = new();

        private const int FrameCount = 12;

        public void Load(string packId, GalleryController controller)
        {
            _packId     = packId;
            _controller = controller;
            ClearFrames();
            SpawnFrames();
        }

        private void ClearFrames()
        {
            foreach (var f in _frames)
                if (f != null) Destroy(f.gameObject);
            _frames.Clear();
        }

        private void SpawnFrames()
        {
            PackProgress pack      = SaveManager.Instance.Data.packs.Find(p => p.packId == _packId);
            var completed          = pack?.completedImageIds ?? new List<string>();

            _packTitleText.text = _packId.ToUpper();
            _subtitleText.text  = $"{FrameCount} Images";

            for (int i = 0; i < FrameCount; i++)
            {
                string imageId   = $"{_packId}_{i:D2}";
                bool   isDone    = completed.Contains(imageId);
                var    frame     = Instantiate(_frameItemPrefab, _framesContainer);
                frame.Setup(imageId, isDone, 0, OnFrameTapped);
                _frames.Add(frame);
            }
        }

        private void OnFrameTapped(string imageId, bool isCompleted)
        {
            if (isCompleted)
                _controller.OpenArtwork(_packId, imageId);
        }
    }
}
