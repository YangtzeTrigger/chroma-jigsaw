using HootyBird.JigsawPuzzleEngine.Model;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Tools
{
    /// <summary>
    /// Custom multithreaded job used to calculate puzzle mesh.
    /// </summary>
    [BurstCompile(
        FloatMode = FloatMode.Fast,
        FloatPrecision = FloatPrecision.Low,
        OptimizeFor = OptimizeFor.Performance)]
    public struct PuzzleMeshDataJob : IJobParallelFor
    {
        // Input data.
        [ReadOnly]
        public NativeReference<int> maxPointsPerEdge;
        [ReadOnly]
        public NativeReference<float> polygonStrength;

        // Iutput data.
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float4> maskRects;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float4> textureRects;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float2> anchorMin;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float2> anchorMax;

        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<float2> allPoints;
        [WriteOnly]
        [NativeDisableParallelForRestriction]
        public NativeArray<int> triangles;

        // Internal use data.
        [ReadOnly]
        public NativeReference<float2> meshSize;
        [ReadOnly]
        private NativeReference<float> puzzlePiecePadding;
        [ReadOnly]
        private NativeReference<int2> puzzleSize;
        [ReadOnly]
        private NativeArray<BezierPoint> edgeData;
        [ReadOnly]
        private NativeArray<int> edgePointsIndices;
        [ReadOnly]
        private NativeArray<float> edgeSegmentsData;
        [ReadOnly]
        private NativeArray<int> edgeSegmentsIndices;
        [ReadOnly]
        private NativeArray<float> edgeSegmentsLength;

        /// <summary>
        /// Initializes job data.
        /// </summary>
        /// <param name="puzzleData"></param>
        /// <param name="scaleTexture"></param>
        /// <param name="puzzlePiecePaddingSize"></param>
        public void Init(PuzzleData puzzleData, float puzzlePiecePaddingSize = .25f)
        {
            puzzleSize = new NativeReference<int2>(new int2(puzzleData.columns, puzzleData.rows), Allocator.Persistent);
            puzzlePiecePadding = new NativeReference<float>(puzzlePiecePaddingSize, Allocator.Persistent);
            // Mesh size, including padding value.
            meshSize = new NativeReference<float2>(new float2(
                puzzlePiecePaddingSize * 2 * puzzleData.columns + puzzleData.columns,
                puzzlePiecePaddingSize * 2 * puzzleData.rows + puzzleData.rows), Allocator.Persistent);

            // PolygonStrength used to slightly increase each polygon size, so there are no 'empy spots' between puzzle pieces.
            polygonStrength = new NativeReference<float>(.008f, Allocator.Persistent);

            // Allocating output data.
            maskRects = new NativeArray<float4>(puzzleData.columns * puzzleData.rows, Allocator.Persistent);
            textureRects = new NativeArray<float4>(puzzleData.columns * puzzleData.rows, Allocator.Persistent);
            anchorMin = new NativeArray<float2>(puzzleData.columns * puzzleData.rows, Allocator.Persistent);
            anchorMax = new NativeArray<float2>(puzzleData.columns * puzzleData.rows, Allocator.Persistent);
            allPoints = new NativeArray<float2>(puzzleData.polygons.Length * (4 * maxPointsPerEdge.Value - 4), Allocator.Persistent);
            triangles = new NativeArray<int>(puzzleData.polygons.Length * (4 * maxPointsPerEdge.Value - 6) * 3, Allocator.Persistent);

            // Flatten polygon data into format used by IJob.
            List<BezierPoint> edgePoints = new List<BezierPoint>();
            List<int> pointsIndices = new List<int>();
            List<float> edgeSegments = new List<float>();
            List<int> segmentsIndices = new List<int>();
            List<float> segmentsLength = new List<float>();
            for (int polygonIndex = 0; polygonIndex < puzzleData.polygons.Length; polygonIndex++)
            {
                Polygon polygon = puzzleData.polygons[polygonIndex];
                for (int edgeIndex = 0; edgeIndex < polygon.EdgeCount; edgeIndex++)
                {
                    Edge edge = polygon[edgeIndex];

                    // Add points.
                    pointsIndices.Add(edgePoints.Count);
                    edgePoints.AddRange(edge.points);

                    // Add segments.
                    segmentsIndices.Add(edgeSegments.Count);
                    edgeSegments.AddRange(edge.segmentsLength);

                    // Add total length.
                    segmentsLength.Add(edge.TotalLength);
                }
            }
            pointsIndices.Add(edgePoints.Count);
            segmentsIndices.Add(edgeSegments.Count);

            // Write data/indices.
            edgeData = new NativeArray<BezierPoint>(edgePoints.ToArray(), Allocator.Persistent);
            edgePointsIndices = new NativeArray<int>(pointsIndices.ToArray(), Allocator.Persistent);

            edgeSegmentsData = new NativeArray<float>(edgeSegments.ToArray(), Allocator.Persistent);
            edgeSegmentsIndices = new NativeArray<int>(segmentsIndices.ToArray(), Allocator.Persistent);

            edgeSegmentsLength = new NativeArray<float>(segmentsLength.ToArray(), Allocator.Persistent);
        }

        public void Dispose()
        {
            // Clear input.
            maxPointsPerEdge.Dispose();
            polygonStrength.Dispose();

            // Clear internal data.
            meshSize.Dispose();
            puzzlePiecePadding.Dispose();
            puzzleSize.Dispose();
            edgeData.Dispose();
            edgePointsIndices.Dispose();
            edgeSegmentsData.Dispose();
            edgeSegmentsIndices.Dispose();
            edgeSegmentsLength.Dispose();

            // Clear output.
            maskRects.Dispose();
            textureRects.Dispose();
            anchorMin.Dispose();
            anchorMax.Dispose();
            allPoints.Dispose();
            triangles.Dispose();
        }

        public void Execute(int polygonIndex)
        {
            // Allocate polygon edge points arrays.
            NativeList<float2> top = new NativeList<float2>(maxPointsPerEdge.Value, Allocator.Temp);
            NativeList<float2> right = new NativeList<float2>(maxPointsPerEdge.Value, Allocator.Temp);
            NativeList<float2> bottom = new NativeList<float2>(maxPointsPerEdge.Value, Allocator.Temp);
            NativeList<float2> left = new NativeList<float2>(maxPointsPerEdge.Value, Allocator.Temp);

            int edgeIndexFrom = polygonIndex * 4;
            int edgeIndexTo = (polygonIndex + 1) * 4;
            // Process each edge.
            for (int edgeIndex = edgeIndexFrom; edgeIndex < edgeIndexTo; edgeIndex++)
            {
                // Take a slice of bezier points for this edge.
                NativeSlice<BezierPoint> bezierPoints = new NativeSlice<BezierPoint>(
                    edgeData,
                    edgePointsIndices[edgeIndex],
                    edgePointsIndices[edgeIndex + 1] - edgePointsIndices[edgeIndex]);
                // Take a slice of segments (lengths) for this edge.
                NativeSlice<float> segments = new NativeSlice<float>(
                    edgeSegmentsData,
                    edgeSegmentsIndices[edgeIndex],
                    edgeSegmentsIndices[edgeIndex + 1] - edgeSegmentsIndices[edgeIndex]);
                // And take the edge overal length.
                float totalLength = edgeSegmentsLength[edgeIndex];

                // Construct edge depending on an edge index.
                switch (edgeIndex - edgeIndexFrom)
                {
                    case 0:
                        FillEdgePoints(totalLength, in bezierPoints, in segments, ref top);

                        break;

                    case 1:
                        FillEdgePoints(totalLength, in bezierPoints, in segments, ref right);

                        break;

                    case 2:
                        FillEdgePoints(totalLength, in bezierPoints, in segments, ref bottom);

                        break;

                    default:
                        FillEdgePoints(totalLength, in bezierPoints, in segments, ref left);

                        break;
                }
            }

            int2 polygonLocation = new int2(polygonIndex % puzzleSize.Value.x, polygonIndex / puzzleSize.Value.x);

            // Collect all polygon points (from edges) and move them to image space.
            NativeList<float2> polygonPoints = new NativeList<float2>(
                top.Length - 1 +
                right.Length - 1 +
                bottom.Length - 1 +
                left.Length - 1,
                Allocator.Temp);
            // From top.
            for (int index = 0; index < top.Length - 1; index++)
            {
                polygonPoints.Add((polygonLocation + top[index] + .5f));
            }
            // From right.
            for (int index = 0; index < right.Length - 1; index++)
            {
                polygonPoints.Add((polygonLocation + right[index] + .5f));
            }
            // From bottom.
            for (int index = 0; index < bottom.Length - 1; index++)
            {
                polygonPoints.Add((polygonLocation + bottom[index] + .5f));
            }
            // From left.
            for (int index = 0; index < left.Length - 1; index++)
            {
                polygonPoints.Add((polygonLocation + left[index] + .5f));
            }

            float2 min = float2.zero;
            float2 max = float2.zero;

            // Get polygon min/max points.
            // These are required for PuzzlePieces to display polygon textures correctly.
            GetMinMax(top, ref min, ref max);
            GetMinMax(right, ref min, ref max);
            GetMinMax(bottom, ref min, ref max);
            GetMinMax(left, ref min, ref max);

            // Since all polygons have (0,0) as center point, move them so (0,0) is a bottom-left corner.
            min += .5f;
            max += .5f;

            // Store polygons locations on puzzle texture.
            textureRects[polygonIndex] = new float4(
                (float)(min.x + polygonLocation.x) / puzzleSize.Value.x,
                (float)(min.y + polygonLocation.y) / puzzleSize.Value.y,
                (max.x - min.x) / puzzleSize.Value.x,
                (max.y - min.y) / puzzleSize.Value.y);

            // Store polygons locations on mask.
            maskRects[polygonIndex] = new float4(
                (puzzlePiecePadding.Value * 2 * polygonLocation.x + puzzlePiecePadding.Value + polygonLocation.x + min.x) / meshSize.Value.x,
                (puzzlePiecePadding.Value * 2 * polygonLocation.y + puzzlePiecePadding.Value + polygonLocation.y + min.y) / meshSize.Value.y,
                (max.x - min.x) / meshSize.Value.x,
                (max.y - min.y) / meshSize.Value.y);

            // Store anchor points.
            anchorMin[polygonIndex] = min;
            anchorMax[polygonIndex] = max;

            // Move polygon to it's position within mask.
            // Scale polygon up a bit (by polygonStrength value).
            NativeArray<float2> newPoints = new NativeArray<float2>(polygonPoints.Length, Allocator.Temp);
            // For each polygon point.
            for (int index = 0; index < polygonPoints.Length; index++)
            {
                float2 ab = polygonPoints[(index - 1 + polygonPoints.Length) % polygonPoints.Length] - polygonPoints[index];
                float2 ac = polygonPoints[(index + 1) % polygonPoints.Length] - polygonPoints[index];
                
                // Find 'outside' direction.
                float2 direction = math.normalize(math.normalize(ab) + math.normalize(ac)) * -math.sign(CrossFloat2(ab, ac));
                // Move point in 'ouside' direction by a bit.
                newPoints[index] = polygonPoints[index] + direction * polygonStrength.Value;
                // Move point to it's location within mask.
                newPoints[index] += new float2(
                    puzzlePiecePadding.Value * 2 * polygonLocation.x + puzzlePiecePadding.Value,
                    puzzlePiecePadding.Value * 2 * polygonLocation.y + puzzlePiecePadding.Value);
            }

            #region Triangulation.

            // Move polygon point into all points list.
            int pointOffset = polygonIndex * (4 * maxPointsPerEdge.Value - 4);
            // Only fill valid points, rest are filled with (-1, -1)
            for (int from = 0; from < 4 * maxPointsPerEdge.Value - 4; from++)
            {
                if (from < newPoints.Length)
                {
                    allPoints[pointOffset + from] = newPoints[from];
                }
                else
                {
                    allPoints[pointOffset + from] = new float2(-1f, -1f);
                }
            }

            NativeList<int> indices = new NativeList<int>(newPoints.Length, Allocator.Temp);
            for (int i = 0; i < newPoints.Length; i++)
            {
                indices.Add(i);
            }

            int currentTriangleIndex = polygonIndex * (4 * maxPointsPerEdge.Value - 6) * 3;
            int safety = 0;
            while (indices.Length > 3)
            {
                for (int i = 0; i < indices.Length; i++)
                {
                    int a = indices[i];
                    int b = indices[PrevIndex(i)];
                    int c = indices[NextIndex(i)];

                    float2 va = newPoints[a];
                    float2 vb = newPoints[b];
                    float2 vc = newPoints[c];

                    float2 ab = vb - va;
                    float2 ac = vc - va;

                    if (CrossFloat2(ab, ac) < 0f)
                    {
                        continue;
                    }

                    bool isEar = true;
                    for (int j = 0; j < indices.Length; j++)
                    {
                        int pointIndex = indices[j];
                        if (pointIndex == a || pointIndex == b || pointIndex == c)
                        {
                            continue;
                        }

                        if (PointInTriangle(newPoints[pointIndex], vb, va, vc))
                        {
                            isEar = false;
                            break;
                        }
                    }

                    if (isEar)
                    {
                        triangles[currentTriangleIndex++] = b;
                        triangles[currentTriangleIndex++] = a;
                        triangles[currentTriangleIndex++] = c;

                        indices.RemoveAt(i);
                        break;
                    }
                }

                // Exit point so we won't stuck in while loop.
                if (safety++ > newPoints.Length * 2)
                {
                    break;
                }
            }

            triangles[currentTriangleIndex++] = indices[0];
            triangles[currentTriangleIndex++] = indices[1];
            triangles[currentTriangleIndex++] = indices[2];

            // fill rest with -1
            for (int from = currentTriangleIndex; from < (polygonIndex + 1) * (4 * maxPointsPerEdge.Value - 6) * 3; from++)
            {
                triangles[from] = -1;
            }

            int NextIndex(int index)
            {
                return (index + 1) % indices.Length;
            }

            int PrevIndex(int index)
            {
                return (index - 1 + indices.Length) % indices.Length;
            }

            #endregion
        }

        /// <summary>
        /// Extract mask rect for polygonIndex polygon.
        /// </summary>
        /// <param name="polygonIndex"></param>
        /// <returns></returns>
        public Rect GetMaskRect(int polygonIndex)
        {
            return new Rect(
                maskRects[polygonIndex].x, 
                maskRects[polygonIndex].y, 
                maskRects[polygonIndex].z, 
                maskRects[polygonIndex].w);
        }

        public Rect GetTextureRect(int polygonIndex)
        {
            return new Rect(
                textureRects[polygonIndex].x,
                textureRects[polygonIndex].y,
                textureRects[polygonIndex].z,
                textureRects[polygonIndex].w);
        }

        /// <summary>
        /// Get anchor min.
        /// </summary>
        /// <param name="polygonIndex"></param>
        /// <returns></returns>
        public Vector2 GetPolygonAnchorMin(int polygonIndex)
        {
            return new Vector2(anchorMin[polygonIndex].x, anchorMin[polygonIndex].y);
        }

        /// <summary>
        /// Get anchor max.
        /// </summary>
        /// <param name="polygonIndex"></param>
        /// <returns></returns>
        public Vector2 GetPolygonAnchorMax(int polygonIndex)
        {
            return new Vector2(anchorMax[polygonIndex].x, anchorMax[polygonIndex].y);
        }

        public Mesh GetMesh()
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            int pointsPerPolygon = 4 * maxPointsPerEdge.Value - 4;
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();

            float2 negative = new float2(-1f, -1f);
            int polygons = allPoints.Length / pointsPerPolygon;
            int vertLength = 0;
            for (int polygonIndex = 0; polygonIndex < polygons; polygonIndex++)
            {
                int vertOffset = polygonIndex * pointsPerPolygon;
                int trianglesOffset = polygonIndex * (pointsPerPolygon - 2) * 3;

                // Add vertices.
                for (int vIndex = 0; vIndex < pointsPerPolygon; vIndex++)
                {
                    if (allPoints[vertOffset + vIndex].Equals(negative))
                    {
                        break;
                    }

                    vertices.Add(new Vector3(allPoints[vertOffset + vIndex].x, allPoints[vertOffset + vIndex].y));
                }

                for (int tIndex = 0; tIndex < (pointsPerPolygon - 2) * 3; tIndex++)
                {
                    if (triangles[trianglesOffset + tIndex].Equals(-1))
                    {
                        break;
                    }

                    indices.Add(triangles[trianglesOffset + tIndex] + vertLength);
                }

                vertLength = vertices.Count;
            }

            Mesh mesh = new Mesh();
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();

            mesh.RecalculateBounds();

            Debug.Log($"Mesh created in {sw.ElapsedMilliseconds}ms. Verts: {vertices.Count}");
            sw.Stop();

            return mesh;
        }

        /// <summary>
        /// Transfrom bezier points into set of regular points.
        /// </summary>
        /// <param name="totalLength"></param>
        /// <param name="bezierPoints"></param>
        /// <param name="segments"></param>
        /// <param name="points"></param>
        private void FillEdgePoints(
            float totalLength,
            in NativeSlice<BezierPoint> bezierPoints,
            in NativeSlice<float> segments,
            ref NativeList<float2> points)
        {
            int segmentIndex = 0;
            float step = 1f / (maxPointsPerEdge.Value - 1);
            float progress = 0f;
            // Create maxPointsPerEdge amount of points along the edge.
            for (int pointIndex = 0; pointIndex < maxPointsPerEdge.Value; pointIndex++)
            {
                float relativeSegmentLength = segments[segmentIndex] / totalLength;
                // If progress is nore than current segment length, move to next segment.
                while (progress > relativeSegmentLength)
                {
                    // If it's the last segment, make progress equal to it's legth.
                    if (segmentIndex == segments.Length - 1)
                    {
                        progress = relativeSegmentLength;
                    }
                    // Otherwise move to next segment.
                    else
                    {
                        progress -= relativeSegmentLength;
                        relativeSegmentLength = segments[++segmentIndex] / totalLength;
                    }
                }

                // Calculate bezier point.
                float2 point = GetBezierPoint(
                    bezierPoints[segmentIndex].Position,
                    bezierPoints[segmentIndex].Position + bezierPoints[segmentIndex].RightControlPoint,
                    bezierPoints[segmentIndex + 1].Position + bezierPoints[segmentIndex + 1].LeftControlPoint,
                    bezierPoints[segmentIndex + 1].Position,
                    progress / relativeSegmentLength);

                points.Add(point);

                // Optimize points.
                int pointsCount = points.Length;
                if (pointsCount > 2)
                {
                    // If current point is on a straight line between previous and next one, remove it.
                    float dot = math.dot(
                        math.normalize(points[pointsCount - 1] - points[pointsCount - 2]),
                        math.normalize(points[pointsCount - 2] - points[pointsCount - 3]));

                    if (dot > .99f)
                    {
                        points.RemoveAt(pointsCount - 2);
                    }
                }

                progress += step;
            }
        }

        private float2 GetBezierPoint(float2 p0, float2 p1, float2 p2, float2 p3, float t)
        {
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            float2 point = uuu * p0;
            point += 3.0f * uu * t * p1;
            point += 3.0f * u * tt * p2;
            point += ttt * p3;

            return point;
        }

        /// <summary>
        /// Checks if point p is inside triangle abc.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool PointInTriangle(float2 p, float2 a, float2 b, float2 c)
        {
            float2 ab = b - a;
            float2 bc = c - b;
            float2 ca = a - c;

            float2 ap = p - a;
            float2 bp = p - b;
            float2 cp = p - c;

            return CrossFloat2(ab, ap) < 0f && CrossFloat2(bc, bp) < 0f && CrossFloat2(ca, cp) < 0f;
        }

        /// <summary>
        /// Points cross product.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private float CrossFloat2(float2 a, float2 b)
        {
            return (a.x * b.y) - (a.y * b.x);
        }

        /// <summary>
        /// Modified min/max using a set of points.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        private void GetMinMax(in NativeList<float2> points, ref float2 min, ref float2 max)
        {
            for (int pointIndex = 0; pointIndex < points.Length; pointIndex++)
            {
                min.x = math.min(min.x, points[pointIndex].x);
                min.y = math.min(min.y, points[pointIndex].y);

                max.x = math.max(max.x, points[pointIndex].x);
                max.y = math.max(max.y, points[pointIndex].y);
            }
        }
    }
}
