using System.Collections.Generic;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Tools
{
    public static class Settings
    {
        public static class PuzzleSettings
        {
            /// <summary>
            /// Value used to build puzzle piece polygon.
            /// </summary>
            public static int PointsPerEdge = 30;

            /// <summary>
            /// Padding between each puzzle piece on a texture.
            /// </summary>
            public static float PuzzlePiecePaddingSize = .35f;

            /// <summary>
            /// Puzzle piece size (in pixels). Higher value - more detailed mask.
            /// Leads to mask having bigger resolution, which takes more video memory.
            /// </summary>
            public static int PuzzlePiecePixelResolution = 512;

            /// <summary>
            /// Max mask texture size.
            /// </summary>
            public static Vector2Int MaxMaskTextureSize = new Vector2Int(4096, 4096);

            /// <summary>
            /// Highlight size in percent.
            /// </summary>
            public static float HighlighSize = 2.5f;

            /// <summary>
            /// Puzzle piece size on a board, reference value.
            /// </summary>
            public static float PuzzlePieceBoardSize = 150f;

            /// <summary>
            /// Piece must be further than (x * <see cref="PuzzlePieceBoardSize"/>) distance from the target to be able to snap to it.
            /// </summary>
            public static float PieceSnapPieceMinDistance = .8f;

            /// <summary>
            /// Piece must be closer than (x * PuzzleSize) distance from the target to be able to snap to it.
            /// </summary>
            public static float PieceSnapPieceMaxDistance = 1.5f;

            /// <summary>
            /// Value from [0, 1], 0 means that another piece must be on a same axis as the snap target.
            /// Value of 1 will let on diagonals to snap together.
            /// </summary>
            public static float PieceSnapMaxDirectionDeviation = .3f;

            /// <summary>
            /// Clicks only registered when time between pointer down and up is less that value. 
            /// </summary>
            public static float PuzzlePieceClickTimeThreshold = .2f;

            /// <summary>
            /// Clicks only registered when distance between pointer down and up is less that value.
            /// </summary>
            public static float PuzzlePieceClickDistanceThreshold = 10f;

            /// <summary>
            /// Lower value will make tilted puzzle pieces return tilted with dragging return to zero rotation faster.
            /// </summary>
            public static float PuzzlePieceDragTilt_TargetToZeroSpeed = .65f;

            /// <summary>
            /// Higher value will make puzzle piece rotation follow target rotation faster.
            /// </summary>
            public static float PuzzlePieceDragTilt_TargetFolloSpeed = .1f;

            /// <summary>
            /// Should mask texture be blurred?. 
            /// </summary>
            public static float BlurMaskSize = .003f;
        }

        public static class GameData
        {
            /// <summary>
            /// Save games data folder location.
            /// </summary>
            public static string SaveFolderPath = "SavedGames";
        }

        public static class InternalAppSettings
        {
            public static string MainMenuControllerName = "MainMenuCanvas";
            public static string GameplayMenuControllerName = "GameplayCanvas";
            /// <summary>
            /// Target framerate.
            /// </summary>
            public static int TargetFramerate = 120;
            /// <summary>
            /// Amount of random puzzle widgets at 'PickRandom' section of home tab.
            /// </summary>
            public static int HomeTabRandomPuzzleCount = 6;
        }

        /// <summary>
        /// Default app settings.
        /// </summary>
        public static class AppSettings
        {
            public static Dictionary<SettingsOptions, bool> DefaultSettings = new Dictionary<SettingsOptions, bool>()
            {
                [SettingsOptions.FreeSnap] = true,
                [SettingsOptions.Rotation] = true,
                [SettingsOptions.ShufflePieces] = false,
            };
        }
    }

    public enum SettingsOptions
    {
        FreeSnap,
        Rotation,
        ShufflePieces,
    }
}
