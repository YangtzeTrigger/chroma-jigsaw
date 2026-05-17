using HootyBird.JigsawPuzzleEngine.Tween;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Sliding BG effect for tab button background.
    /// </summary>
    public class TabButtonBg : MonoBehaviour
    {
        [SerializeField]
        private ValueTween leftValueTween;
        [SerializeField]
        private ValueTween rightValueTween;
        [SerializeField]
        private float trailingSideSlowerBy = .2f;

        private RectTransform rectTransform;
        private List<OpenTabButton> openTabButtons;
        private OpenTabButton currentButton;
        private int lastButtonIndex = -1;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            openTabButtons = new List<OpenTabButton>(transform.parent.GetComponentsInChildren<OpenTabButton>());
            foreach (OpenTabButton button in openTabButtons)
            {
                button.OnStateChanged += OnStateChanged;
            }

            leftValueTween._onValue += OnLeftValue;
            rightValueTween._onValue += OnRightValue;
        }

        private void OnStateChanged(OpenTabButton button, bool state)
        {
            if (state)
            {
                currentButton = button;

                if (lastButtonIndex > -1)
                {
                    if (openTabButtons.IndexOf(currentButton) - lastButtonIndex > 0)
                    {
                        rightValueTween.playbackTime = button.TweenDuration;
                        leftValueTween.playbackTime = button.TweenDuration + trailingSideSlowerBy;
                    }
                    else
                    {
                        rightValueTween.playbackTime = button.TweenDuration + trailingSideSlowerBy;
                        leftValueTween.playbackTime = button.TweenDuration;
                    }
                }
                lastButtonIndex = openTabButtons.IndexOf(button);

                StopAllCoroutines();
                StartCoroutine(AnimationRoutine());
            }
        }

        private void OnLeftValue(float value)
        {
            rectTransform.offsetMin = new Vector2(value, rectTransform.offsetMin.y);
        }

        private void OnRightValue(float value)
        {
            rectTransform.offsetMax = new Vector2(value, rectTransform.offsetMax.y);
        }

        private void UpdateTweenValues()
        {
            Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(transform.parent, currentButton.transform);
            leftValueTween.to = bounds.min.x;
            rightValueTween.to = bounds.max.x;
        }

        private IEnumerator AnimationRoutine()
        {
            leftValueTween.from = leftValueTween.to;
            rightValueTween.from = rightValueTween.to;

            leftValueTween.PlayForward(false);
            rightValueTween.PlayForward(false);

            while (leftValueTween.isPlaying || rightValueTween.isPlaying)
            {
                yield return null;

                UpdateTweenValues();
            }
        }
    }
}
