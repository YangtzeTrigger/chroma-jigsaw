using HootyBird.JigsawPuzzleEngine.Gameplay;
using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.Services;
using System;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Tools
{
    public static class PuzzleTools
    {
        /// <summary>
        /// Get puzzle progress from saved game data.
        /// </summary>
        /// <param name="savedGame"></param>
        /// <param name="columns"></param>
        /// <param name="rows"></param>
        /// <returns></returns>
        public static int GetProgress(this SavedGameData savedGame, int columns, int rows)
        {
            int boardPieces = savedGame.data.Where(piece => piece.state == PuzzlePieceState.Board).Count();

            return Mathf.FloorToInt((float)boardPieces / (columns * rows) * 100f);
        }

        /// <summary>
        /// Combine puzzle id and settings id.
        /// </summary>
        /// <param name="settingsId"></param>
        /// <param name="puzzleId"></param>
        /// <returns></returns>
        public static string CombineSettingsWithId(string settingsId, string puzzleId)
        {
            return $"{puzzleId}-{settingsId}";
        }

        /// <summary>
        /// Generates simple flat edge.
        /// </summary>
        /// <returns></returns>
        public static Edge FlatEdge()
        {
            return new Edge()
            {
                points = new BezierPoint[]
                {
                    new BezierPoint()
                    {
                        Position = Vector2.zero,
                    },
                    new BezierPoint()
                    {
                        Position = new Vector2(1f, 0f),
                    },
                },
                segmentsLength = new float[]
                {
                    1f,
                }
            };
        }

        /// <summary>
        /// Rotate <see cref="Edge"/> by angle.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="angle"></param>
        public static void Rotate(ref Edge edge, float angle)
        {
            for (int pointIndex = 0; pointIndex < edge.points.Length; pointIndex++)
            {
                edge.points[pointIndex].Rotate(angle);
            }
        }

        /// <summary>
        /// Move <see cref="Edge"/> by offset.
        /// </summary>
        /// <param name="edge"></param>
        /// <param name="offset"></param>
        public static void Move(ref Edge edge, Vector2 offset)
        {
            for (int pointIndex = 0; pointIndex < edge.points.Length; pointIndex++)
            {
                edge.points[pointIndex].Position += offset;
            }
        }

        /// <summary>
        /// Reverse <see cref="Edge"/> points order.
        /// </summary>
        /// <param name="edge"></param>
        public static void ReverseOrder(ref Edge edge)
        {
            edge.points = edge.points.Reverse().ToArray();
            for (int pointIndex = 0; pointIndex < edge.points.Length; pointIndex++)
            {
                edge.points[pointIndex].FlipControlPoints();
            }
        }
    }
}
