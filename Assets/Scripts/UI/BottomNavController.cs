using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromaJigsaw.Audio;
using ChromaJigsaw.Data;

namespace ChromaJigsaw.UI
{
    public class BottomNavController : MonoBehaviour
    {
        [SerializeField] private TabEntry[] _tabs;    // 4 elements: Gallery, Puzzles, Ambience, Sanctuary
        [SerializeField] private GameObject[] _panels; // matching panel GameObjects
        [SerializeField] private XPBarView  _xpBarView;

        private CanvasGroup[] _panelGroups;

        private static readonly Color ActiveColor   = new Color(0.957f, 0.741f, 0.380f);        // #f4bd61
        private static readonly Color InactiveColor = new Color(0.827f, 0.769f, 0.698f, 0.6f);  // #d3c4b2 @ 60%

        private static readonly string[] ScreenNames =
            { "tab_gallery", "tab_puzzles", "tab_ambience", "tab_sanctuary" };

        // null = no named event (Sanctuary is a no-tracking space)
        private static readonly string[] TabEvents =
            { "gallery_opened", "puzzles_opened", "ambience_opened", null };

        private int _activeIndex = -1;

        private void Awake()
        {
            // Initialize all panels as active but invisible before Start() runs —
            // prevents a single-frame flash where all panels are visible.
            _panelGroups = new CanvasGroup[_panels.Length];
            for (int i = 0; i < _panels.Length; i++)
            {
                _panels[i].SetActive(true);
                _panelGroups[i] = _panels[i].GetComponent<CanvasGroup>();
                if (_panelGroups[i] == null)
                    _panelGroups[i] = _panels[i].AddComponent<CanvasGroup>();
                _panelGroups[i].alpha          = 0f;
                _panelGroups[i].blocksRaycasts = false;
                _panelGroups[i].interactable   = false;
            }
        }

        private void Start()
        {
            for (int i = 0; i < _tabs.Length; i++)
            {
                int idx = i;
                _tabs[i].button.onClick.AddListener(() => SelectTab(idx));
            }
            SelectTab(0);
        }

        public void SelectTab(int index)
        {
            if (index == _activeIndex) return;
            int previous = _activeIndex;
            _activeIndex = index;
            AudioManager.Instance.Play(SFXType.ButtonClick);

            if (previous >= 0 && previous < _panelGroups.Length && _panelGroups[previous] != null)
            {
                var outGroup = _panelGroups[previous];
                outGroup.DOKill();
                outGroup.interactable   = false;
                outGroup.blocksRaycasts = false;
                outGroup.DOFade(0f, 0.2f).SetEase(Ease.InCubic);
            }

            if (index < _panelGroups.Length && _panelGroups[index] != null)
            {
                var inGroup = _panelGroups[index];
                inGroup.DOKill();
                inGroup.alpha          = 0f;
                inGroup.blocksRaycasts = true;
                inGroup.interactable   = true;
                inGroup.DOFade(1f, 0.3f).SetEase(Ease.OutCubic);
            }

            for (int i = 0; i < _tabs.Length; i++)
            {
                bool active = i == index;
                _tabs[i].topBorder.gameObject.SetActive(active);
                _tabs[i].icon.color  = active ? ActiveColor : InactiveColor;
                _tabs[i].label.color = active ? ActiveColor : InactiveColor;
            }

            bool showXP = index == 0 || index == 1;  // Gallery + Puzzles only
            if (_xpBarView != null)
            {
                if (showXP) _xpBarView.Show();
                else        _xpBarView.Hide();
            }

            AnalyticsManager.Instance.LogScreenView(ScreenNames[index]);
            var tabEvent = index < TabEvents.Length ? TabEvents[index] : null;
            if (tabEvent != null)
                AnalyticsManager.Instance.LogEvent(tabEvent);
        }

        [Serializable]
        private class TabEntry
        {
            public Button           button;
            public Image            topBorder;
            public Image            icon;
            public TextMeshProUGUI  label;
        }
    }
}
