using System;
using System.Linq;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// <see cref="Polygon"/> edge. Each polygon have 4 edges.
    /// </summary>
    [Serializable]
    public struct Edge
    {
        /// <summary>
        /// Used for edge length calculation.
        /// </summary>
        public static int PointsPerBezierLine = 35;

        public BezierPoint[] points;

        public float[] segmentsLength;

        public float TotalLength => segmentsLength.Sum();

        public Edge(Edge toCopy)
        {
            points = new BezierPoint[toCopy.points.Length];
            segmentsLength = new float[toCopy.segmentsLength.Length];

            Array.Copy(toCopy.points, points, toCopy.points.Length);
            Array.Copy(toCopy.segmentsLength, segmentsLength, toCopy.segmentsLength.Length);
        }

        /// <summary>
        /// Segmenst lengths are calculated and stored for each edge to be used later on.
        /// </summary>
        public void UpdateLength()
        {
            if (points == null || points.Length < 2)
            {
                return;
            }

            segmentsLength = new float[points.Length - 1];

            for (int pointIndex = 0; pointIndex < segmentsLength.Length; pointIndex++)
            {
                BezierPoint point = points[pointIndex];
                BezierPoint nextPoint = points[pointIndex + 1];

                Vector2 rightControlPoint = point.Position + point.RightControlPoint;
                Vector2 leftControlPoint = nextPoint.Position + nextPoint.LeftControlPoint;

                Vector2 currentBezierPoint;
                Vector2 previousBezierPoint = point.Position;
                for (int segmentIndex = 1; segmentIndex <= PointsPerBezierLine; segmentIndex++)
                {
                    currentBezierPoint = GetBezierPoint(
                        point.Position, 
                        rightControlPoint,
                        nextPoint.Position, 
                        leftControlPoint, 
                        (float)segmentIndex / PointsPerBezierLine);

                    // Update length/prev point.
                    segmentsLength[pointIndex] += Vector2.Distance(currentBezierPoint, previousBezierPoint);
                    previousBezierPoint = currentBezierPoint;
                }
            }
        }

        private Vector2 GetBezierPoint(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float tt = t * t;
            float ttt = t * tt;
            float u = 1.0f - t;
            float uu = u * u;
            float uuu = u * uu;

            Vector2 point = uuu * p0;
            point += 3.0f * uu * t * p1;
            point += 3.0f * u * tt * p2;
            point += ttt * p3;

            return point;
        }
    }
}
