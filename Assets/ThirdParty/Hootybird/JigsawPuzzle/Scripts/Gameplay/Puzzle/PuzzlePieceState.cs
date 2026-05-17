namespace HootyBird.JigsawPuzzleEngine.Gameplay
{
    /// <summary>
    /// Current puzzle piece state.
    /// </summary>
    public enum PuzzlePieceState
    {
        /// <summary>
        /// Puzzle piece exists, but not as a part of cluster/board.
        /// </summary>
        None,
        /// <summary>
        /// Puzzle piece is on a board, but is not snapped to it.
        /// </summary>
        Overlay,
        /// <summary>
        /// Puzzle piece is snapped to board.
        /// </summary>
        Board,
    }
}