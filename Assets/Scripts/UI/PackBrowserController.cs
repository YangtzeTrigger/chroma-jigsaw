using System.Collections.Generic;
using UnityEngine;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class PackBrowserController : MonoBehaviour
    {
        [SerializeField] private PackRegistry _registry;
        [SerializeField] private PackCardView _cardPrefab;
        [SerializeField] private Transform    _container;

        private readonly List<PackCardView> _cards = new();

        private void Start()
        {
            if (_registry == null || _cardPrefab == null || _container == null) return;
            Populate();
            AnalyticsManager.Instance.LogScreenView("pack_browser");
        }

        private void Populate()
        {
            foreach (var card in _cards)
                if (card != null) Destroy(card.gameObject);
            _cards.Clear();

            foreach (var config in _registry.packs)
            {
                var result = PackUnlockService.Instance.Evaluate(config, out string subLabel);
                int done   = GetCompletedCount(config.packId);
                var card   = Instantiate(_cardPrefab, _container);
                card.Populate(config, result, subLabel, done, OnPackTapped);
                _cards.Add(card);
            }
        }

        private void OnPackTapped(string packId)
        {
            // B-07: navigate into pack puzzle selection
            AnalyticsManager.Instance.LogEvent("pack_tapped",
                new Dictionary<string, object> { { "pack_id", packId } });
        }

        private static int GetCompletedCount(string packId)
        {
            var p = SaveManager.Instance.Data.packs.Find(x => x.packId == packId);
            return p?.completedImageIds?.Count ?? 0;
        }
    }
}
