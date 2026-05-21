using System;
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

        private static readonly Color ActiveColor   = new Color(0.957f, 0.741f, 0.380f);        // #f4bd61
        private static readonly Color InactiveColor = new Color(0.827f, 0.769f, 0.698f, 0.6f);  // #d3c4b2 @ 60%

        private static readonly string[] ScreenNames =
            { "tab_gallery", "tab_puzzles", "tab_ambience", "tab_sanctuary" };

        private int _activeIndex = -1;

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
            _activeIndex = index;
            AudioManager.Instance.Play(SFXType.ButtonClick);

            for (int i = 0; i < _tabs.Length; i++)
            {
                bool active = i == index;
                if (i < _panels.Length) _panels[i].SetActive(active);
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
