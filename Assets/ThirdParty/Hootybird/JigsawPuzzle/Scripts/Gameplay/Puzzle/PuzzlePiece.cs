using HootyBird.JigsawPuzzleEngine.Model;
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    public class PuzzlePiece : PuzzlePieceRenderer
    {
        [SerializeField]
        [Header("Piece texture target.")]
        private RawImage target;
        [SerializeField]
        private RectTransform rotationParent;
        [Header("Left, Top, Right, Bottom")]
        [SerializeField]
        private Vector4 highlightEffectStrength = new Vector4(30f, 30f, 30f, 30f);
        [SerializeField]
        [Range(0f, 10f)]
        private float highlightEffectSize = 2.5f;

        [Header("Read-only editor values.")]
#if UNITY_EDITOR
        [ReadOnly]
        [SerializeField]
        private List<PuzzlePiece> cluster;
        [ReadOnly]
        [SerializeField]
        private PuzzlePieceState state;
#endif

        private Rect positionRestrictionWorldRect;
        private Puzzle puzzle;

        // Components.
        public RectTransform RotationParent => rotationParent;
        public override RawImage RenderTarget { get => target; }

        // Internal data.
        public PuzzlePieceCluster Cluster { get; set; }
        public Polygon Polygon { get; set; }
        public PuzzlePieceState State { get; set; }
        public PuzzlePieceDirection Direction { get; set; }
        public Vector4 HighlightStrength => highlightEffectStrength;
        public float HighlightEffectSize => highlightEffectSize;

        // Events.
        public Action<PuzzlePiece, PuzzlePieceEventOrigin> OnSnappedToPuzzleBoard { get; set; }
        public Action<PuzzlePiece, PuzzlePieceEventOrigin> OnSnappedToPuzzleCluster { get; set; }
        public Action<PuzzlePiece, PuzzlePieceEventOrigin> OnRotated { get; set; }
        public Action<PuzzlePiece> OnPuzzlePieceReset { get; set; }


#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (Cluster != null)
            {
                cluster = Cluster.PuzzlePieces;
            }

            state = State;
        }
#endif

        public void SetPuzzleObject(Puzzle puzzle)
        {
            this.puzzle = puzzle;
        }

        public void SetPolygonData(Polygon polygon)
        {
            Polygon = polygon;
            name = $"PuzzlePiece: {polygon.location.x}:{polygon.location.y}";
        }

        public void SetRestrictionWorldRect(Rect rect)
        {
            positionRestrictionWorldRect = rect;
        }

        /// <summary>
        /// Snap puzzle piece to position on a puzzle board.
        /// If it's a puzzle piece cluster, snap them all, and remove the cluster.
        /// </summary>
        public void TrySnapClusterToBoard()
        {
            if (Cluster != null)
            {
                foreach (PuzzlePiece puzzlePiece in Cluster.PuzzlePieces)
                {
                    puzzlePiece.SnapToBoard();
                }

                puzzle.ClearCluster(Cluster);
            }
            else
            {
                SnapToBoard();
            }
        }

        /// <summary>
        /// Snap puzzle piece to location on board.
        /// </summary>
        /// <param name="eventOrigin">Event origin.</param>
        public void SnapToBoard(PuzzlePieceEventOrigin eventOrigin = PuzzlePieceEventOrigin.Player)
        {
            State = PuzzlePieceState.Board;
            Vector2 targetPosition = 
                puzzle.HalfPuzzlePieceWorldSize + 
                puzzle.PuzzleBoardWorldRect.min + 
                new Vector2(Polygon.location.x * puzzle.PuzzlePieceWorldSize, Polygon.location.y * puzzle.PuzzlePieceWorldSize);

            transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.parent.position.z);

            OnSnappedToPuzzleBoard?.Invoke(this, eventOrigin);
        }

        /// <summary>
        /// Connect puzzle piece to cluster, or cluster to puzzle piece.
        /// </summary>
        /// <param name="target"></param>
        public void ConnectTo(PuzzlePiece target)
        {
            PuzzlePieceCluster cluster = Cluster;
            PuzzlePieceCluster targetCluster = target.Cluster;
            Vector2 direction = Polygon.location - target.Polygon.location;
            MoveTo(target.transform.position + (Vector3)direction * puzzle.PuzzlePieceWorldSize, true);

            // If both this and target have clusters.
            if (cluster != null && targetCluster != null)
            {
                List<PuzzlePiece> clusterPizzlePieces = new List<PuzzlePiece>(cluster.PuzzlePieces);
                puzzle.ClearCluster(cluster);

                foreach (PuzzlePiece piece in clusterPizzlePieces)
                {
                    piece.JoinCluster(targetCluster);
                }
            }
            // If part of cluster, snap puzzlePiece to this.
            else if (cluster != null)
            {
                target.JoinCluster(cluster);
            }
            // If target is cluster, attach this to it.
            else if (targetCluster != null)
            {
                JoinCluster(targetCluster);
            }
            // Or just create new cluster.
            else
            {
                cluster = puzzle.CreateCluster();

                JoinCluster(cluster);
                target.JoinCluster(cluster);
            }
        }

        public void JoinCluster(PuzzlePieceCluster cluster, PuzzlePieceEventOrigin eventOrigin = PuzzlePieceEventOrigin.Player)
        {
            Cluster = cluster;

            Cluster.AddPuzzlePiece(this);
            OnSnappedToPuzzleCluster?.Invoke(this, eventOrigin);
        }

        /// <summary>
        /// Collect all cluster pieces, including self.
        /// </summary>
        /// <returns></returns>
        public List<PuzzlePiece> GetClusterPieces()
        {
            List<PuzzlePiece> result = new List<PuzzlePiece>();

            if (Cluster != null)
            {
                result.AddRange(Cluster.PuzzlePieces);
            }
            else
            {
                result.Add(this);
            }

            return result;
        }

        /// <summary>
        /// Move this piece/cluster by offset value.
        /// </summary>
        /// <param name="offset">Offset value.</param>
        /// <param name="ignoreRestriction">Can move even if result position if outside rectriction bounds.</param>
        public void Move(Vector3 offset, bool ignoreRestriction = false)
        {
            // Modify offset value if restrictions check enabled.
            if (!ignoreRestriction)
            {
                Vector2 min = Cluster != null ?
                    Cluster.Min :
                    (Vector2)transform.position - puzzle.HalfPuzzlePieceWorldSize;
                Vector2 max = Cluster != null ?
                    Cluster.Max :
                    (Vector2)transform.position + puzzle.HalfPuzzlePieceWorldSize;

                // Restrict puzzle peice movement to not be able to move outside restriction bounds.
                // For x axis.
                if (min.x + offset.x < positionRestrictionWorldRect.min.x)
                {
                    offset.x = positionRestrictionWorldRect.min.x - min.x;
                }
                else if (max.x + offset.x > positionRestrictionWorldRect.max.x)
                {
                    offset.x = positionRestrictionWorldRect.max.x - max.x;
                }

                // For y axis.
                if (min.y + offset.y < positionRestrictionWorldRect.min.y)
                {
                    offset.y = positionRestrictionWorldRect.min.y - min.y;
                }
                else if (max.y + offset.y > positionRestrictionWorldRect.max.y)
                {
                    offset.y = positionRestrictionWorldRect.max.y - max.y;
                }
            }

            // Apply offset to itself if individual puzzle piece.
            if (Cluster == null)
            {
                transform.position += offset;
            }
            // Or to a whole cluster if part of one.
            else
            {
                foreach (PuzzlePiece puzzlePiece in Cluster.PuzzlePieces)
                {
                    puzzlePiece.transform.position += offset;
                }
            }
        }

        /// <summary>
        /// Move piece/cluster to target world position.
        /// </summary>
        /// <param name="newPos">Target postition.</param>
        /// <param name="ignoreRestriction">Can move even if result position if outside rectriction bounds</param>
        public void MoveTo(Vector2 newPos, bool ignoreRestriction = false)
        {
            Move(newPos - (Vector2)transform.position, ignoreRestriction);
        }

        /// <summary>
        /// Rotae puzzle piece to given direction value.
        /// </summary>
        /// <param name="direction">Target direction.</param>
        /// <param name="forceRotation">Force rotation even if value is disabled in puzzle settings.</param>
        /// <param name="eventOrigin">OnRotated event origin value.</param>
        public void RotateTo(
            PuzzlePieceDirection direction, 
            bool forceRotation = false,
            PuzzlePieceEventOrigin eventOrigin = PuzzlePieceEventOrigin.SaveFile)
        {
            // Ignore rotations if setting is unchecked.
            if (!puzzle.PiecesRotationEnabled && !forceRotation)
            {
                return;
            }

            Direction = direction;
            OnRotated?.Invoke(this, eventOrigin);

            // Rotate to value.
            switch (Direction)
            {
                case PuzzlePieceDirection.Up:
                    rotationParent.localEulerAngles = Vector3.zero;

                    break;

                case PuzzlePieceDirection.Right:
                    rotationParent.localEulerAngles = new Vector3(0f, 0f, 90f);

                    break;

                case PuzzlePieceDirection.Down:
                    rotationParent.localEulerAngles = new Vector3(0f, 0f, 180f);

                    break;

                default:
                    rotationParent.localEulerAngles = new Vector3(0f, 0f, 270f);

                    break;
            }
        }

        public void ResetToDefault()
        {
            RotateTo(PuzzlePieceDirection.Up, true);
            State = PuzzlePieceState.None;

            OnPuzzlePieceReset?.Invoke(this);
        }
    }
}