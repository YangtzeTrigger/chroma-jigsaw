using HootyBird.JigsawPuzzleEngine.Tween;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Base overlay type for tabbed overlay.
    /// </summary>
    [RequireComponent(typeof(PositionTween))]
    public abstract class TabOverlay : MenuOverlay
    {
        private int tabIndex;
        private List<TabOverlay> tabs;
        private PositionTween positionTween;

        public Action<bool> OnStateChanged { get; set; }

        protected override void Awake()
        {
            base.Awake();

            positionTween = GetComponent<PositionTween>();
        }

        protected override void Start()
        {
            base.Start();

            if (isDefault)
            {
                OnStateChanged?.Invoke(true);
            }

            // Find other tabs.
            tabs = MenuController.GetOverlays<TabOverlay>().ToList();
            // and this index.
            tabIndex = tabs.IndexOf(this);
        }

        public override void Close(bool animate = true)
        {
            if (IsOpened)
            {
                OnStateChanged?.Invoke(false);
            }

            base.Close(animate);
        }

        public override void Open()
        {
            if (!IsOpened)
            {
                OnStateChanged?.Invoke(true);
            }

            base.Open();
        }

        // Do nothing when back is pressed.
        public override void OnBack() { }

        /// <summary>
        /// Open tab with specified index.
        /// </summary>
        /// <param name="targetTabIndex"></param>
        public void OpenTabIndex(int targetTabIndex)
        {
            if (!IsCurrent)
            {
                return;
            }

            // Either move to the right or left, depending on a specified tab index.
            if (targetTabIndex > tabIndex)
            {
                OpenRight(targetTabIndex);
            }
            else if (targetTabIndex < tabIndex)
            {
                OpenLeft(targetTabIndex);
            }
        }

        /// <summary>
        /// Moves this tab to the left, and taget tab from the right.
        /// </summary>
        /// <param name="index"></param>
        public void OpenRight(int index)
        {
            AdjustPositionFrom(true);
            tabs[index].AdjustPositionFrom(false);
            CloseSelf();
            MenuController.OpenOverlay(index);
        }

        /// <summary>
        /// Moves this tab to the right, and taget tab from the left.
        /// </summary>
        /// <param name="index"></param>
        public void OpenLeft(int index)
        {
            AdjustPositionFrom(false);
            tabs[index].AdjustPositionFrom(true);
            CloseSelf();
            MenuController.OpenOverlay(index);
        }

        private void AdjustPositionFrom(bool left)
        {
            positionTween.from = new Vector3(left ? -RectTransform.rect.width : RectTransform.rect.width, 0f);
            positionTween.to = Vector3.zero;
        }
    }
}
