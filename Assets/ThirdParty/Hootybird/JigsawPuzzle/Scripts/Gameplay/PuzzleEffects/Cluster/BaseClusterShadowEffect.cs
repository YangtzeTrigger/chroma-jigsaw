using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    public abstract class BaseClusterShadowEffect : MonoBehaviour
    {
        public Puzzle PuzzleView { get; set; }
        public PuzzlePieceCluster AssignedCluster { get; set; }

        protected virtual void OnDestroy()
        {
            Clear();
        }

        public abstract void OnClusterDrag(PuzzlePiece target);

        public abstract void OnClusterDrop();

        public abstract void AddShadowForPuzzlePiece(PuzzlePiece puzzlePiece);

        public virtual void Clear() { }
    }
}
