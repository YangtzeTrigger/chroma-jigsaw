using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tween;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Tab button at the bottom of main menu controller.
    /// </summary>
    public class OpenTabButton : MonoBehaviour
    {
        private MenuController menuController;
        private int tabIndex;
        private Button button;

        private TabOverlay targetTab;
        private TweenBase stateTween;

        public float TweenDuration => stateTween.playbackTime;
        public Action<OpenTabButton, bool> OnStateChanged { get; set; }

        private void Awake()
        {
            button = GetComponent<Button>();
            stateTween = GetComponent<TweenBase>();

            tabIndex = transform.parent.GetComponentsInChildren<OpenTabButton>().ToList().IndexOf(this);
            menuController = GetComponentInParent<MenuController>();

            button.onClick.AddListener(OnClick);

            // Find a tab to open when tabButton is clicked.
            targetTab = menuController.GetOverlays<TabOverlay>().ElementAt(tabIndex);
            // Subscribe to tab changed event, so it can trigger tabButton animation.
            targetTab.OnStateChanged += OnTabStateChanged;
        }

        private void OnClick()
        {
            // Find current tab.
            TabOverlay currentTab = menuController.currentOverlay.GetType().IsSubclassOf(typeof(TabOverlay))
                ? menuController.currentOverlay as TabOverlay 
                : null;

            if (!currentTab) 
            {
                return;
            }

            AudioService.Instance.PlaySfx("menu-click", .4f);

            // Use current tab to open tab with this tabButton index.
            currentTab.OpenTabIndex(tabIndex);
        }

        private void OnTabStateChanged(bool state)
        {
            if (state)
            {
                stateTween.PlayForward(false);
            }
            else
            {
                stateTween.PlayBackward(false);
            }

            OnStateChanged?.Invoke(this, state);
        }
    }
}
