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

        private static readonly Color GoldColor    = new Color(0.957f, 0.741f, 0.380f);  // #f4bd61
        private static readonly Color DimColor     = new Color(0.498f, 0.498f, 0.498f, 0.6f);
        private static readonly Color OutlineColor = new Color(0.310f, 0.271f, 0.216f);  // #4f4537

        private Action _onBegin;

        private void Awake()
        {
            if (_dotCompleted  == null) _dotCompleted  = MakeDotSprite(GoldColor, true);
            if (_dotInProgress == null) _dotInProgress = MakeDotSprite(GoldColor, false);
            if (_dotEmpty      == null) _dotEmpty      = MakeDotSprite(OutlineColor, false);
            _beginButton.onClick.AddListener(HandleBegin);
        }

        public void Refresh(Action onBegin)
        {
            _onBegin = onBegin;
            var entries = DailyService.Instance.GetActiveDailies();

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

        private void HandleBegin()
        {
            _beginButton.transform.DOKill();
            _beginButton.transform.DOPunchScale(Vector3.one * 0.12f, 0.35f, 6, 0.5f);
            AudioManager.Instance.Play(SFXType.ButtonClick);
            var entries = DailyService.Instance.GetActiveDailies();
            string dailyId = entries.Length > 0 ? entries[0].dailyId : "";
            AnalyticsManager.Instance.LogEvent("daily_opened",
                new Dictionary<string, object> { { "daily_id", dailyId } });
            _onBegin?.Invoke();
        }

        private static Sprite MakeDotSprite(Color color, bool filled, int size = 32)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float c      = (size - 1) * 0.5f;
            float outerR = c - 0.5f;
            float ringW  = 4f;
            for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d  = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                Color px = filled ? (d <= outerR ? color : Color.clear)
                                  : (d <= outerR && d >= outerR - ringW ? color : Color.clear);
                tex.SetPixel(x, y, px);
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), Vector2.one * 0.5f, size);
        }
    }
}
