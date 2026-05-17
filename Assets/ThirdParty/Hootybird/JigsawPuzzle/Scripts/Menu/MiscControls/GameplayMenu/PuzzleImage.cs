using HootyBird.JigsawPuzzleEngine.Tween;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Menu
{
    /// <summary>
    /// Puzzle image visible under puzzle board when enabled.
    /// </summary>
    [RequireComponent(typeof(RawImage))]
    [RequireComponent(typeof(TweenBase))]
    public class PuzzleImage : MonoBehaviour
    {
        private TweenBase animationTween;
        private RawImage image;

        private void Awake()
        {
            image = GetComponent<RawImage>();
            animationTween = GetComponent<TweenBase>();
        }

        public void SetImage(Texture texture, Rect imageRect)
        {
            image.texture = texture;
            image.uvRect = imageRect;
        }

        public void SetState(bool state, bool animate)
        {
            if (animate)
            {
                if (state)
                {
                    animationTween.PlayForward(false);
                }
                else
                {
                    animationTween.PlayBackward(false);
                }
            }
            else
            {
                animationTween.Progress(state ? 1f : 0f, PlaybackDirection.FORWARD);
            }
        }
    }
}
