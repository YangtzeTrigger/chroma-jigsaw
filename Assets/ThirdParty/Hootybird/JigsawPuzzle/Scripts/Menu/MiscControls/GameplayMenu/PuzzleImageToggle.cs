using HootyBird.JigsawPuzzleEngine.Services;
using HootyBird.JigsawPuzzleEngine.Tween;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Toggle used to control <see cref="PuzzleImage"/> state.
    /// </summary>
    [RequireComponent(typeof(Button))]
    [RequireComponent(typeof(ColorTween))]
    public class PuzzleImageToggle : MonoBehaviour
    {
        [SerializeField]
        private bool state;

        private ColorTween animationTween;
        private Button button;

        // bool State, bool Animate
        public Action<bool, bool> OnStateChanged { get; set; }

        private void Awake()
        {
            animationTween = GetComponent<ColorTween>();
            button = GetComponent<Button>();
            button.onClick.AddListener(OnButtonClick);
        }

        private void Start()
        {
            animationTween.Progress(state ? 1f : 0f, PlaybackDirection.FORWARD);
            OnStateChanged?.Invoke(state, false);
        }

        public void ToggleState()
        {
            state = !state;
            OnStateChanged?.Invoke(state, true);

            if (state)
            {
                animationTween.PlayForward(false);
            }
            else
            {
                animationTween.PlayBackward(false);
            }
        }

        private void OnButtonClick()
        {
            AudioService.Instance.PlaySfx("menu-click", .4f);
            ToggleState();
        }
    }
}
