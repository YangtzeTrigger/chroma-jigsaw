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

        private static readonly Color GoldColor = new Color(0.957f, 0.741f, 0.380f);   // #f4bd61
        private static readonly Color DimColor  = new Color(0.310f, 0.271f, 0.216f, 0.3f); // #4f4537 @ 30%

        private int _currentIndex;

        private void Awake()
        {
            _beginButton.onClick.AddListener(HandleBeginRitual);
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

            _titleText.text      = entry.title;
            _pieceCountText.text = $"{entry.pieceCount} PIECES";
            if (entry.imageSprite != null)
                _artworkImage.sprite = entry.imageSprite;
            _editorialText.text  = $"Daily Masterpiece / {entry.overlayScript}";

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

        private void HandleBeginRitual()
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

            // B-08: HootyBridge.Instance.LoadPuzzle(entry.imageSprite.texture, entry.pieceCount);
            // SceneManager.LoadScene(SceneNames.Game);
        }
    }
}
