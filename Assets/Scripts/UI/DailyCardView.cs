using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class DailyCardView : MonoBehaviour
    {
        [SerializeField] private RawImage        _artworkPreview;
        [SerializeField] private Button          _beginButton;
        [SerializeField] private Image[]         _dotIndicators;   // 3 elements
        [SerializeField] private Sprite          _dotCompleted;    // filled circle ◉
        [SerializeField] private Sprite          _dotInProgress;   // ring ◎
        [SerializeField] private Sprite          _dotEmpty;        // open circle ◯

        private static readonly Color GoldColor = new Color(0.957f, 0.741f, 0.380f);  // #f4bd61
        private static readonly Color DimColor  = new Color(0.498f, 0.498f, 0.498f, 0.6f);

        private Action _onBeginRitual;

        private void Awake() =>
            _beginButton.onClick.AddListener(HandleBeginRitual);

        public void Refresh(List<DailyRecord> records, Action onBeginRitual)
        {
            _onBeginRitual = onBeginRitual;

            for (int i = 0; i < _dotIndicators.Length; i++)
            {
                DOTween.Kill(_dotIndicators[i]);

                if (i >= records.Count)
                {
                    _dotIndicators[i].sprite = _dotEmpty;
                    _dotIndicators[i].color  = DimColor;
                    continue;
                }

                var rec = records[i];
                if (rec.completed)
                {
                    _dotIndicators[i].sprite = _dotCompleted;
                    _dotIndicators[i].color  = GoldColor;
                }
                else if (rec.piecesSolved > 0)
                {
                    _dotIndicators[i].sprite = _dotInProgress;
                    _dotIndicators[i].color  = GoldColor;

                    var dot = _dotIndicators[i];
                    DOTween.To(() => dot.color.a,
                               x => { var c = dot.color; c.a = x; dot.color = c; },
                               0.3f, 0.8f)
                           .SetTarget(dot)
                           .SetLoops(-1, LoopType.Yoyo)
                           .SetEase(Ease.InOutSine);
                }
                else
                {
                    _dotIndicators[i].sprite = _dotEmpty;
                    _dotIndicators[i].color  = DimColor;
                }
            }
        }

        private void HandleBeginRitual()
        {
            AnalyticsManager.Instance.LogEvent("daily_begin");
            _onBeginRitual?.Invoke();
        }
    }
}
