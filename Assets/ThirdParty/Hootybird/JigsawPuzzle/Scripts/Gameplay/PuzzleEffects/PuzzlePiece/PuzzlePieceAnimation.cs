using HootyBird.JigsawPuzzleEngine.Tween;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    public class PuzzlePieceAnimation : MonoBehaviour
    {
        [SerializeField]
        private PuzzlePieceAnimationType pieceAnimationType;
        [SerializeField]
        private TweenBase[] animations;

        private PuzzlePiece puzzlePiece;

        public PuzzlePieceAnimationType PuzzlePieceAnimationType => pieceAnimationType;

        private void Awake()
        {
            puzzlePiece = GetComponent<PuzzlePiece>();

            puzzlePiece.OnPuzzlePieceReset += OnPuzzlePieceReset;
        }

        private void OnEnable() { }

        public void PlayAnimations()
        {
            if (!enabled)
            {
                return;
            }

            foreach (TweenBase tween in animations)
            {
                tween.PlayForward(false);
            }
        }

        private void OnPuzzlePieceReset(PuzzlePiece piece)
        {
            foreach (TweenBase tween in animations)
            {
                tween.AtProgress(0f);
            }
        }
    }
}
