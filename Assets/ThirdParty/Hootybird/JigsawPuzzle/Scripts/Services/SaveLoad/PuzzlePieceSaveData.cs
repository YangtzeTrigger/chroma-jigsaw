using HootyBird.JigsawPuzzleEngine.Gameplay;
using System;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Services
{
    /// <summary>
    /// Puzzle piece data used for save/load logic.
    /// </summary>
    [Serializable]
    public struct PuzzlePieceSaveData
    {
        /// <summary>
        /// Local position.
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// Target puzzle piece location on a board.
        /// </summary>
        public Vector2Int boardLocation;

        /// <summary>
        /// Puzzle piece state.
        /// </summary>
        public PuzzlePieceState state;

        /// <summary>
        /// Puzzle piece direction.
        /// </summary>
        public PuzzlePieceDirection direction;

        /// <summary>
        /// Cluster Id.
        /// </summary>
        public int clusterId;

        public PuzzlePieceSaveData(PuzzlePiece fromPuzzlePiece, int clusterId)
        {
            position = fromPuzzlePiece.transform.localPosition;
            boardLocation = fromPuzzlePiece.Polygon.location;
            state = fromPuzzlePiece.State;
            direction = fromPuzzlePiece.Direction;

            this.clusterId = clusterId;
        }
    }
}
