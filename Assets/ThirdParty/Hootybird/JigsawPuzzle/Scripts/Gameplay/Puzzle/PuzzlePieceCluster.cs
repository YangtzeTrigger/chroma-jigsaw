using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    public class PuzzlePieceCluster
    {
        /// <summary>
        /// Puzzle piece with max Y pos.
        /// </summary>
        public PuzzlePiece Top { get; private set; }
        /// <summary>
        /// Puzzle piece with max X pos.
        /// </summary>
        public PuzzlePiece Right { get; private set; }
        /// <summary>
        /// Puzzle peice with min Y pos.
        /// </summary>
        public PuzzlePiece Bottom { get; private set; }
        /// <summary>
        /// Puzzle peice with min X pos.
        /// </summary>
        public PuzzlePiece Left { get; private set; }

        /// <summary>
        /// Puzzle object.
        /// </summary>
        public Puzzle Puzzle { get; private set; }
        /// <summary>
        /// List of puzzle pieces in this cluster.
        /// </summary>
        public List<PuzzlePiece> PuzzlePieces { get; private set; } = new List<PuzzlePiece>();
        /// <summary>
        /// Min cluster position.
        /// </summary>
        public Vector2 Min => new Vector2(Left.transform.position.x, Bottom.transform.position.y) - Puzzle.HalfPuzzlePieceWorldSize;
        /// <summary>
        /// max cluster position.
        /// </summary>
        public Vector2 Max => new Vector2(Right.transform.position.x, Top.transform.position.y) + Puzzle.HalfPuzzlePieceWorldSize;

        public PuzzlePieceCluster(Puzzle puzzle)
        {
            Puzzle = puzzle;
        }

        /// <summary>
        /// Add puzzle to cluster.
        /// </summary>
        /// <param name="puzzlePiece"></param>
        public void AddPuzzlePiece(PuzzlePiece puzzlePiece)
        {
            PuzzlePieces.Add(puzzlePiece);

            // Check it's location.
            if (Top == null || Top.Polygon.location.y < puzzlePiece.Polygon.location.y)
            {
                Top = puzzlePiece;
            }

            if (Bottom == null || Bottom.Polygon.location.y > puzzlePiece.Polygon.location.y)
            {
                Bottom = puzzlePiece;
            }

            if (Right == null || Right.Polygon.location.x < puzzlePiece.Polygon.location.x)
            {
                Right = puzzlePiece;
            }

            if (Left == null || Left.Polygon.location.x > puzzlePiece.Polygon.location.x)
            {
                Left = puzzlePiece;
            }
        }
    }
}
