using System;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Model
{
    /// <summary>
    /// Point that <see cref="Edge"/> consists of.
    /// </summary>
    [Serializable]
    public struct BezierPoint
    {
        [SerializeField]
        private Vector2 position;
        [SerializeField]
        private Vector2 leftControlPoint;
        [SerializeField]
        private Vector2 rightControlPoint;

        public BezierPointMode mode;

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public Vector2 LeftControlPoint
        {
            get => leftControlPoint;
            set
            {
                leftControlPoint = value;
                if (mode == BezierPointMode.Continuous)
                {
                    rightControlPoint = -value;
                }
            }
        }

        public Vector2 RightControlPoint
        {
            get => rightControlPoint;
            set
            {
                rightControlPoint = value;
                if (mode == BezierPointMode.Continuous)
                {
                    leftControlPoint = -value;
                }
            }
        }

        public void Rotate(float angle)
        {
            position = Quaternion.Euler(0f, 0f, angle) * position;
            leftControlPoint = Quaternion.Euler(0f, 0f, angle) * leftControlPoint;
            rightControlPoint = Quaternion.Euler(0f, 0f, angle) * rightControlPoint;
        }

        public void FlipControlPoints()
        {
            Vector2 temp = rightControlPoint;
            rightControlPoint = leftControlPoint;
            leftControlPoint = temp;
        }

#if UNITY_EDITOR
        private static Vector2 PointPosMin = new Vector2(0f, -.3f);
        private static Vector2 PointPosMax = new Vector2(1f, .3f);
        private static Vector2 ControlPointPosMin = new Vector2(-.3f, -.3f);
        private static Vector2 ControlPointPosMax = new Vector2(.3f, .3f);

        public void Validate()
        {
            position = new Vector2(
                Round(Mathf.Clamp(position.x, PointPosMin.x, PointPosMax.x)), Round(Mathf.Clamp(position.y, PointPosMin.y, PointPosMax.y)));
            leftControlPoint = new Vector2(
                Round(Mathf.Clamp(leftControlPoint.x, ControlPointPosMin.x, ControlPointPosMax.x)), 
                Round(Mathf.Clamp(leftControlPoint.y, ControlPointPosMin.y, ControlPointPosMax.y)));
            rightControlPoint = new Vector2(
                Round(Mathf.Clamp(rightControlPoint.x, ControlPointPosMin.x, ControlPointPosMax.x)), 
                Round(Mathf.Clamp(rightControlPoint.y, ControlPointPosMin.y, ControlPointPosMax.y)));
        }

        private float Round(float input)
        {
            return (float)Math.Round(input, 3, MidpointRounding.AwayFromZero);
        }
#endif
    }
}
