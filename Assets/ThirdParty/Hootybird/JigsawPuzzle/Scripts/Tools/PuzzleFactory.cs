using HootyBird.JigsawPuzzleEngine.Model;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Tools
{
    public static class PuzzleFactory
    {
        /// <summary>
        /// Generated PuzzleData fomr settings, seed, and an optional puzzleId (used for save game functionality).
        /// </summary>
        /// <param name="puzzleSettings"></param>
        /// <param name="seed"></param>
        /// <param name="puzzleId"></param>
        /// <returns></returns>
        public static PuzzleData FromPuzzleSettings(PuzzleSettings puzzleSettings, int seed, string puzzleId = "")
        {
            return FromEdgePresets(
                puzzleSettings.rows, 
                puzzleSettings.columns, 
                puzzleSettings.edgeOptions, 
                seed, 
                puzzleId, 
                puzzleSettings.id);
        }

        public static PuzzleDataPackage ToPuzzleDataPackage(this PuzzleSettingsPackage puzzleSettingsPackage)
        {
            return new PuzzleDataPackage() 
            { 
                puzzleData = FromPuzzleSettings(
                    puzzleSettingsPackage.puzzleSettings, 
                    puzzleSettingsPackage.seed, 
                    puzzleSettingsPackage.puzzleId),
                puzzleTexture = puzzleSettingsPackage.puzzleTexture,
            };
        }

        public static PuzzleData FromEdgePresets(
            int rows,
            int columns,
            IEnumerable<Edge> edges,
            int seed,
            string puzzleId = "",
            string settingsId = "")
        {
            if (rows == 0 || columns == 0)
            {
                return null;
            }

            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            // Set default edge if none supplied.
            if (edges == null || edges.Count() == 0)
            {
                edges = new Edge[] { PuzzleTools.FlatEdge() };
            }

            // Update random state to use seed value.
            Random.State prev = Random.state;
            Random.InitState(seed);

            // Prepare build-phase edges.
            Edge[] flatEdges = BuildEdgeRotations(PuzzleTools.FlatEdge());
            // Create rotated version for each edge option.
            Edge[][] rotatedEdges = new Edge[edges.Count()][];
            for (int edgePresetIndex = 0; edgePresetIndex < edges.Count(); edgePresetIndex++)
            {
                rotatedEdges[edgePresetIndex] = BuildEdgeRotations(edges.ElementAt(edgePresetIndex));
            }

            // New dictionary of polygons. Keys are locations.
            Dictionary<Vector2Int, Polygon> polygons = new Dictionary<Vector2Int, Polygon>();

            // Generate random list of edge indices to fill.
            // each polygon is 4 edges, so rows*columns*4 number of indices.
            List<int> emptyEdges = Enumerable.Range(0, rows * columns * 4).ToList();
            // Randomize them.
            emptyEdges.Sort((a, b) => Random.Range(-1, 1));

            // Fill edges.
            int rowIndex;
            int columnIndex;
            int polygonIndex;
            int edgeIndex;
            Polygon polygon;
            while (emptyEdges.Count > 0)
            {
                polygonIndex = Mathf.FloorToInt(emptyEdges[0] / 4);
                edgeIndex = emptyEdges[0] % 4;

                columnIndex = polygonIndex % columns;
                rowIndex = Mathf.FloorToInt(polygonIndex / columns);

                Vector2Int location = new Vector2Int(columnIndex, rowIndex);

                // Create new polygon in none exist under 'location' key. Otherwise use existing one.
                if (polygons.ContainsKey(location))
                {
                    polygon = polygons[location];
                }
                else
                {
                    polygons.Add(location, polygon = new Polygon() { 
                        location = location,
                        edges = new Edge[]
                        {
                            new Edge(),
                            new Edge(),
                            new Edge(),
                            new Edge(),
                        }
                    });
                }

                // If edge have a neighbor, meaning it's not on a border of puzzle.
                if (EdgeHaveNeighbour())
                {
                    // Location edge that this edge is opposite of.
                    Vector2Int neighbourLocation = location + DirectionFromEdgeIndex(edgeIndex);
                    int edgeIndexInversed = EdgeIndexInversed(edgeIndex);

                    // If opposite edge exists, and have points (already resolved), mirror it to this edge.
                    if (polygons.ContainsKey(neighbourLocation) &&
                        polygons[neighbourLocation][edgeIndexInversed].points != null)
                    {
                        // Create copy of opposite edge.
                        polygon[edgeIndex] = new Edge(polygons[neighbourLocation][edgeIndexInversed]);

                        // Move edge to align with this polygon.
                        PuzzleTools.Move(ref polygon.edges[edgeIndex], DirectionFromEdgeIndex(edgeIndex));
                        // Since it's a copy of edge from opposite side, flip edge points order.
                        PuzzleTools.ReverseOrder(ref polygon.edges[edgeIndex]);

                    }
                    // Otherwise apply random edge preset with proper rotation.
                    else
                    {
                        polygon[edgeIndex] = rotatedEdges[Random.Range(0, rotatedEdges.Length)][edgeIndex];
                    }
                }
                // It's a border edge, apply flat edge preset.
                else
                {
                    polygon[edgeIndex] = flatEdges[edgeIndex];
                }

                // Edge resolved, remove it.
                emptyEdges.RemoveAt(0);

                bool EdgeHaveNeighbour()
                {
                    switch (edgeIndex)
                    {
                        case 0: return rowIndex < rows - 1;
                        case 1: return columnIndex < columns - 1;
                        case 2: return rowIndex > 0;
                        default: return columnIndex > 0;
                    }
                }
            }

            // Restore previous random state.
            Random.state = prev;

            // Finally create puzzle data using polygons.
            PuzzleData puzzleData = new PuzzleData()
            {
                polygons = polygons.Values.OrderBy(polygon => polygon.location.y * columns + polygon.location.x).ToArray(),
                rows = rows,
                columns = columns,
                puzzleId = puzzleId,
                settingsId = settingsId,
            };

#if !UNITY_EDITOR
            Debug.Log($"Generated puzzle data from settings in {stopwatch.ElapsedMilliseconds} ms");
#endif

            return puzzleData;
        }

        /// <summary>
        /// Translate edge index into direction.
        /// </summary>
        /// <param name="edgeIndex"></param>
        /// <returns></returns>
        private static Vector2Int DirectionFromEdgeIndex(int edgeIndex)
        {
            switch(edgeIndex)
            {
                case 0: return new Vector2Int(0, 1);
                case 1: return new Vector2Int(1, 0);
                case 2: return new Vector2Int(0, -1);
                default: return new Vector2Int(-1, 0);
            }
        }

        /// <summary>
        /// Inversed value for edge index. (If 0, returns 2. If 3, returns 1.)
        /// </summary>
        /// <param name="edgeIndex"></param>
        /// <returns></returns>
        private static int EdgeIndexInversed(int edgeIndex) => (edgeIndex + 2) % 4;

        /// <summary>
        /// Created 4 versions of a given edge, moved by (0, 0.5) and rotated by 90 degree.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        private static Edge[] BuildEdgeRotations(Edge edge)
        {
            Edge copy = new Edge(edge);
            PuzzleTools.Move(ref copy, new Vector2(-.5f, .5f));

            // Top edge index 0.
            Edge[] result = new Edge[4];
            result[0] = new Edge(copy);

            // Right edge index 1.
            result[1] = new Edge(copy);
            PuzzleTools.Rotate(ref result[1], -90f);

            // Bottom edge index 2.
            result[2] = new Edge(copy);
            PuzzleTools.Rotate(ref result[2], -180f);

            // Left edge index 3.
            result[3] = new Edge(copy);
            PuzzleTools.Rotate(ref result[3], -270f);

            return result;
        }
    }
}
