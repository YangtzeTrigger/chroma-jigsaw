using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using ChromaJigsaw.Audio;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class DailyCardView : MonoBehaviour
    {
        [SerializeField] private RawImage _artworkPreview;
        [SerializeField] private Button   _beginButton;
        [SerializeField] private Image[]  _dotIndicators;   // 3 elements
        [SerializeField] private Sprite   _dotCompleted;    // filled circle ◉
        [SerializeField] private Sprite   _dotInProgress;   // ring ◎
        [SerializeField] private Sprite   _dotEmpty;        // open circle ◯

        private static readonly Color GoldColor = new Color(0.957f, 0.741f, 0.380f);
        private static readonly Color DimColor  = new Color(0.498f, 0.498f, 0.498f, 0.6f);

        private Action _onBeginRitual;

        private void Awake() =>
            _beginButton.onClick.AddListener(HandleBeginRitual);

        public void Refresh(Action onBeginRitual)
        {
            _onBeginRitual = onBeginRitual;
            var entries = DailyService.Instance.GetActiveDailies();

            // Show newest daily's artwork in the card preview
            if (entries.Length > 0 && entries[0].imageSprite != null)
                _artworkPreview.texture = entries[0].imageSprite.texture;

            // dots[0]=leftmost=oldest=entries[2], dots[2]=rightmost=newest=entries[0]
            for (int i = 0; i < _dotIndicators.Length; i++)
            {
                _dotIndicators[i].DOKill();
                int entryIdx = (_dotIndicators.Length - 1) - i;

                if (entryIdx >= entries.Length)
                {
                    _dotIndicators[i].sprite = _dotEmpty;
                    _dotIndicators[i].color  = DimColor;
                    continue;
                }

                var state = DailyService.Instance.GetDailyState(entries[entryIdx].dailyId);
                switch (state)
                {
                    case DailyState.Completed:
                        _dotIndicators[i].sprite = _dotCompleted;
                        _dotIndicators[i].color  = GoldColor;
                        break;

                    case DailyState.InProgress:
                        _dotIndicators[i].sprite = _dotInProgress;
                        _dotIndicators[i].color  = GoldColor;
                        _dotIndicators[i].DOFade(0.3f, 0.8f)
                                         .SetLoops(-1, LoopType.Yoyo)
                                         .SetEase(Ease.InOutSine);
                        break;

                    default:
                        _dotIndicators[i].sprite = _dotEmpty;
                        _dotIndicators[i].color  = DimColor;
                        break;
                }
            }
        }

        private void HandleBeginRitual()
        {
            _beginButton.transform.DOKill();
            _beginButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 6, 0.5f);
            AudioManager.Instance.Play(SFXType.ButtonClick);
            var entries = DailyService.Instance.GetActiveDailies();
            string dailyId = entries.Length > 0 ? entries[0].dailyId : "";
            AnalyticsManager.Instance.LogEvent("daily_opened",
                new Dictionary<string, object> { { "daily_id", dailyId } });
            _onBeginRitual?.Invoke();
        }
    }
}
