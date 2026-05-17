using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    /// <summary>
    /// Shadow effect for puzzle piece clusters.
    /// </summary>
    [RequireComponent(typeof(Puzzle))]
    public class ClustersShadowEffectHandler : MonoBehaviour
    {
        [SerializeField]
        private BaseClusterShadowEffect clusterShadowPrefab;

        private List<BaseClusterShadowEffect> effects = new List<BaseClusterShadowEffect>();

        public Puzzle PuzzleView { get; private set; }

        private void Awake()
        {
            PuzzleView = GetComponent<Puzzle>();

            PuzzleView.OnPuzzlePieceCreated += OnPuzzlePieceCreated;
            PuzzleView.OnClusterRemoved += OnClusterRemoved;
        }

        private void OnPuzzlePieceCreated(PuzzlePiece puzzlePiece)
        {
            puzzlePiece.OnSnappedToPuzzleCluster += OnPuzzlePieceSnappedToCuster;

            PuzzlePieceInteraction puzzlePieceInteraction = puzzlePiece.GetComponent<PuzzlePieceInteraction>();

            if (puzzlePieceInteraction)
            {
                puzzlePieceInteraction.OnPieceDrag += OnPuzzlePieceDrag;
                puzzlePieceInteraction.OnPiecePointerUp += OnPuzzlePiecePointerUp;
            }
        }

        private void OnClusterRemoved(PuzzlePieceCluster cluster)
        {
            BaseClusterShadowEffect effect = effects.Find(effect => effect.AssignedCluster == cluster);

            effect.Clear();
            Destroy(effect.gameObject);

            effects.Remove(effect);
        }

        private void OnPuzzlePieceDrag(PointerEventData eventData, PuzzlePiece puzzlePiece)
        {
            if (puzzlePiece.Cluster == null)
            {
                return;
            }

            effects
                .Find(effect => effect.AssignedCluster == puzzlePiece.Cluster)
                .OnClusterDrag(puzzlePiece);
        }

        private void OnPuzzlePiecePointerUp(PointerEventData eventData, PuzzlePiece puzzlePiece)
        {
            if (puzzlePiece.Cluster == null)
            {
                return;
            }

            effects
                .Find(effect => effect.AssignedCluster == puzzlePiece.Cluster)
                .OnClusterDrop();
        }

        private void OnPuzzlePieceSnappedToCuster(PuzzlePiece puzzlePiece, PuzzlePieceEventOrigin eventOrigin)
        {
            BaseClusterShadowEffect effect = effects.Find(effect => effect.AssignedCluster == puzzlePiece.Cluster);

            if (effect == null)
            {
                effect = Instantiate(clusterShadowPrefab, PuzzleView.transform);
                effect.AssignedCluster = puzzlePiece.Cluster;
                effect.PuzzleView = PuzzleView;

                effects.Add(effect);
            }

            effect.AddShadowForPuzzlePiece(puzzlePiece);
        }
    }
}
