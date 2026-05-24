using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromaJigsaw.Core;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class DailyMasterpieceView : MonoBehaviour
    {
        [SerializeField] private CanvasGroup     _canvasGroup;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _pieceCountText;
        [SerializeField] private Image           _artworkImage;
        [SerializeField] private TextMeshProUGUI _editorialText;
        [SerializeField] private Button          _beginButton;
        [SerializeField] private Button          _closeButton;
        [SerializeField] private Button[]        _dotButtons;    // 3; tapping switches active daily
        [SerializeField] private Sprite          _dotCompleted;
        [SerializeField] private Sprite          _dotInProgress;
        [SerializeField] private Sprite          _dotEmpty;

        private static readonly Color GoldColor    = new Color(0.957f, 0.741f, 0.380f);   // #f4bd61
        private static readonly Color DimColor     = new Color(0.310f, 0.271f, 0.216f, 0.3f); // #4f4537 @ 30%
        private static readonly Color OutlineColor = new Color(0.310f, 0.271f, 0.216f);   // #4f4537

        private int _currentIndex;

        private void Awake()
        {
            if (_dotCompleted  == null) _dotCompleted  = MakeDotSprite(GoldColor, true);
            if (_dotInProgress == null) _dotInProgress = MakeDotSprite(GoldColor, false);
            if (_dotEmpty      == null) _dotEmpty      = MakeDotSprite(OutlineColor, false);

            _beginButton.onClick.AddListener(HandleBegin);
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Close);

            for (int i = 0; i < _dotButtons.Length; i++)
            {
                int idx = i;
                _dotButtons[i].onClick.AddListener(() =>
                {
                    var entries  = DailyService.Instance.GetActiveDailies();
                    int entryIdx = (_dotButtons.Length - 1) - idx;
                    string dailyId = entryIdx < entries.Length ? entries[entryIdx].dailyId : "";
                    AnalyticsManager.Instance.LogEvent("daily_dot_tapped", new Dictionary<string, object>
                    {
                        { "daily_id",  dailyId },
                        { "dot_index", idx     }
                    });
                    Open(idx);
                });
            }

            _canvasGroup.alpha          = 0f;
            _canvasGroup.interactable   = false;
            _canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
        }

        public void Open(int dailyIndex = 0)
        {
            var entries = DailyService.Instance.GetActiveDailies();
            if (entries.Length == 0) return;

            _currentIndex = Mathf.Clamp(dailyIndex, 0, entries.Length - 1);
            var entry = entries[_currentIndex];

            if (_titleText != null)
                _titleText.text = string.IsNullOrWhiteSpace(entry.title) ? "Today's Puzzle" : entry.title;
            if (_pieceCountText != null)
                _pieceCountText.text = $"{entry.pieceCount} PIECES";
            if (entry.imageSprite != null && _artworkImage != null)
                _artworkImage.sprite = entry.imageSprite;
            if (_editorialText != null)
                _editorialText.text = "A quiet ritual for today";

            RefreshDots(entries);

            gameObject.SetActive(true);
            _canvasGroup.interactable   = true;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
        }

        public void Close()
        {
            _canvasGroup.DOKill();
            _canvasGroup.DOFade(0f, 0.25f).SetEase(Ease.InCubic).OnComplete(() =>
            {
                _canvasGroup.interactable   = false;
                _canvasGroup.blocksRaycasts = false;
                gameObject.SetActive(false);
            });
        }

        private void RefreshDots(DailyEntry[] entries)
        {
            // dots[0]=leftmost=oldest=entries[2], dots[2]=rightmost=newest=entries[0]
            for (int i = 0; i < _dotButtons.Length; i++)
            {
                int  entryIdx = (_dotButtons.Length - 1) - i;
                var  img      = _dotButtons[i].image;
                img.DOKill();

                if (entryIdx >= entries.Length)
                {
                    img.sprite = _dotEmpty;
                    img.color  = DimColor;
                    continue;
                }

                var  state    = DailyService.Instance.GetDailyState(entries[entryIdx].dailyId);
                bool isActive = entryIdx == _currentIndex;

                switch (state)
                {
                    case DailyState.Completed:
                        img.sprite = _dotCompleted;
                        img.color  = GoldColor;
                        break;

                    case DailyState.InProgress:
                        img.sprite = _dotInProgress;
                        img.color  = GoldColor;
                        if (!isActive)
                            img.DOFade(0.3f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
                        break;

                    default:
                        img.sprite = _dotEmpty;
                        img.color  = isActive ? GoldColor : DimColor;
                        break;
                }
            }
        }

        private void HandleBegin()
        {
            var entries = DailyService.Instance.GetActiveDailies();
            if (_currentIndex >= entries.Length) return;

            var entry = entries[_currentIndex];
            DailyService.Instance.SaveProgress(entry.dailyId, 1);
            AnalyticsManager.Instance.LogEvent("daily_started",
                new Dictionary<string, object>
                {
                    { "daily_id",    entry.dailyId },
                    { "piece_count", entry.pieceCount }
                });

            // TODO B-13: HootyBridge.Instance.LoadPuzzle(entry.imageSprite.texture, entry.pieceCount);
            // TODO B-13: SceneManager.LoadScene(SceneNames.Game);
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
