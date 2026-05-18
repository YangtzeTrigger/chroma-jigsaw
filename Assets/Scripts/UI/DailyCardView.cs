using System;
using System.Collections;
using System.Collections.Generic;
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
                if (i >= records.Count)
                {
                    SetDot(i, _dotEmpty, DimColor, false);
                    continue;
                }
                var rec = records[i];
                if (rec.completed)
                    SetDot(i, _dotCompleted, GoldColor, false);
                else if (rec.piecesSolved > 0)
                    SetDot(i, _dotInProgress, GoldColor, true);   // pulsing
                else
                    SetDot(i, _dotEmpty, DimColor, false);
            }
        }

        private void SetDot(int index, Sprite sprite, Color color, bool pulse)
        {
            _dotIndicators[index].sprite = sprite;
            _dotIndicators[index].color  = color;
            StopAllCoroutines();
            if (pulse)
                StartCoroutine(PulseDot(index));
        }

        private IEnumerator PulseDot(int index)
        {
            // DOTween: _dotIndicators[index].DOFade(0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo);
            while (true)
            {
                yield return Fade(index, 1f, 0.3f, 0.8f);
                yield return Fade(index, 0.3f, 1f, 0.8f);
            }
        }

        private IEnumerator Fade(int index, float from, float to, float duration)
        {
            float elapsed = 0f;
            Color c = _dotIndicators[index].color;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                c.a = Mathf.Lerp(from, to, elapsed / duration);
                _dotIndicators[index].color = c;
                yield return null;
            }
        }

        private void HandleBeginRitual()
        {
            AnalyticsManager.Instance.LogEvent("daily_begin");
            _onBeginRitual?.Invoke();
        }
    }
}
