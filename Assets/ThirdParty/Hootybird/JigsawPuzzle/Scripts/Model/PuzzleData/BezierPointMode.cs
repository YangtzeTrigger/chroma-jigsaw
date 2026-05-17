namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// Bezier point mode.
    /// </summary>
    public enum BezierPointMode
    {
        // Control points free to move around.
        Broken,
        // Control points are locked with each other.
        Continuous,
    }
}
