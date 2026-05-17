using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using HootyBird.JigsawPuzzleEngine.Tools;

namespace HootyBird.JigsawPuzzleEngine.Editor
{
    [CustomEditor(typeof(EdgeObject))]
    public class CustomEdgeEditor : UnityEditor.Editor
    {
        private static Vector2 EdgeViewPadding = new Vector2(30f, 30f);
        private static float EdgeViewHeight = 250f;
        private static float PointSize = .02f;
        private static float ControlPointSize = .015f;
        private static float CurveWidth = .005f;
        private static Color CurveColor = Color.cyan;

        public VisualTreeAsset editorAsset;
        public VisualTreeAsset itemAsset;

        private VisualElement edgeView;
        private SerializedProperty edgeProperty;
        private SerializedProperty pointsProperty;
        private (int index, bool? leftControlPoint) pointCaptured;

        private Vector2 EdgeViewOrigin
        {
            get;
            set;
        }

        private float ViewZoom => edgeView.layout.width - EdgeViewPadding.x * 2f;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = editorAsset.CloneTree();

            ListView points = root.Q<ListView>("points");
            points.makeItem = () =>
            {
                TemplateContainer templateContainer = itemAsset.CloneTree();

                PropertyField p = templateContainer.Q<PropertyField>("pos");
                p.RegisterCallback<ChangeEvent<Vector2>>(OnPointUpdate, TrickleDown.TrickleDown);
                p.label = "";

                p = templateContainer.Q<PropertyField>("cp1");
                p.RegisterCallback<ChangeEvent<Vector2>>(OnPointUpdate, TrickleDown.TrickleDown);
                p.label = "";

                p = templateContainer.Q<PropertyField>("cp2");
                p.RegisterCallback<ChangeEvent<Vector2>>(OnPointUpdate, TrickleDown.TrickleDown);
                p.label = "";

                return templateContainer;
            };

            edgeView = root.Q<VisualElement>("edge-view");
            edgeView.style.width = new StyleLength(new Length(100, LengthUnit.Percent));
            edgeView.style.height = new StyleLength(new Length(EdgeViewHeight, LengthUnit.Pixel));
            edgeView.generateVisualContent += GenerateEdgeView;

            edgeView.RegisterCallback<PointerMoveEvent>(EdgeViewPointerMoveEvent, TrickleDown.TrickleDown);
            edgeView.RegisterCallback<PointerDownEvent>(EdgeViewPointerDownEvent, TrickleDown.TrickleDown);
            edgeView.RegisterCallback<PointerUpEvent>(EdgeViewPointerUpEvent, TrickleDown.TrickleDown);
            edgeView.RegisterCallback<PointerLeaveEvent>(EdgeViewPointerLeaveEvent, TrickleDown.TrickleDown);

            root.Add(edgeView);

            Init();

            return root;
        }

        private void Init()
        {
            edgeProperty = serializedObject.FindProperty("edge");
            pointsProperty = edgeProperty.FindPropertyRelative("points");

            if (pointsProperty.arraySize == 0)
            {
                edgeProperty.boxedValue = PuzzleTools.FlatEdge();
                serializedObject.ApplyModifiedProperties();
            }

            pointCaptured = (-1, null);
            EdgeViewOrigin = new Vector2(EdgeViewPadding.x, EdgeViewHeight - EdgeViewPadding.y);
        }

        private void EdgeViewPointerDownEvent(PointerDownEvent evt)
        {
            switch (evt.button)
            {
                case 0:
                    for (int pointIndex = 0; pointIndex < pointsProperty.arraySize; pointIndex++)
                    {
                        if (PointCaptured(pointIndex, evt.localPosition))
                        {
                            return;
                        }
                    }

                    break;
            }
        }

        private void EdgeViewPointerMoveEvent(PointerMoveEvent evt)
        {
            if (pointCaptured.index > -1)
            {
                UpdatePointPosition(evt.localPosition);
                edgeView.MarkDirtyRepaint();
            }
        }

        private void EdgeViewPointerUpEvent(PointerUpEvent evt)
        {
            pointCaptured = (-1, null);
        }

        private void EdgeViewPointerLeaveEvent(PointerLeaveEvent evt)
        {
            pointCaptured = (-1, null);
        }

        private bool PointCaptured(int pointIndex, Vector2 position)
        {
            BezierPoint point = (BezierPoint)pointsProperty.GetArrayElementAtIndex(pointIndex).boxedValue;

            if (
                pointIndex != 0 &&
                Vector2.Distance(
                    PointToViewPos(point.Position + point.LeftControlPoint) + EdgeViewOrigin, 
                    position) <= ControlPointSize * ViewZoom)
            {
                pointCaptured = (pointIndex, true);
            }
            else if (
                pointIndex != pointsProperty.arraySize - 1 &&
                Vector2.Distance(
                    PointToViewPos(point.Position + point.RightControlPoint) + EdgeViewOrigin, 
                    position) <= ControlPointSize * ViewZoom)
            {
                pointCaptured = (pointIndex, false);
            }
            else if (pointIndex > 0 &&
                pointIndex < pointsProperty.arraySize - 1 &&
                Vector2.Distance(PointToViewPos(point.Position) + EdgeViewOrigin, position) <= PointSize * ViewZoom)
            {
                pointCaptured = (pointIndex, null);
            }

            return pointCaptured.index > -1;
        }

        private void UpdatePointPosition(Vector2 position)
        {
            SerializedProperty pointProperty = pointsProperty.GetArrayElementAtIndex(pointCaptured.index);
            BezierPoint point = (BezierPoint)pointProperty.boxedValue;

            if (pointCaptured.leftControlPoint.HasValue)
            {
                if (pointCaptured.leftControlPoint.Value)
                {
                    point.LeftControlPoint = ViewPosToPoint(position) - point.Position;
                }
                else
                {
                    point.RightControlPoint = ViewPosToPoint(position) - point.Position;
                }
            }
            else
            {
                point.Position = ViewPosToPoint(position);
            }

            point.Validate();
            pointProperty.boxedValue = point;

            serializedObject.ApplyModifiedProperties();
        }

        private void OnPointUpdate(ChangeEvent<Vector2> evt)
        {
            edgeView.MarkDirtyRepaint();
        }

        private void GenerateEdgeView(MeshGenerationContext ctx)
        {
            Painter2D painter = ctx.painter2D;

            DrawPoints();

            DrawControlPointsConnection();

            DrawCurves();

            void DrawPoints()
            {
                for (int pointIndex = 0; pointIndex < pointsProperty.arraySize; pointIndex++)
                {
                    BezierPoint shapePoint = (BezierPoint)pointsProperty.GetArrayElementAtIndex(pointIndex).boxedValue;
                    bool isStaticPoint = pointIndex == 0 || pointIndex == pointsProperty.arraySize - 1;

                    Vector2 pointPos = PointToViewPos(shapePoint.Position) + EdgeViewOrigin;
                    Vector2 leftControlPointPos = pointPos + PointToViewPos(shapePoint.LeftControlPoint);
                    Vector2 rightControlPointPos = pointPos + PointToViewPos(shapePoint.RightControlPoint);

                    DrawPoint();

                    if (pointIndex < pointsProperty.arraySize)
                    {
                        if (pointIndex != 0)
                        {
                            DrawLeftControlPoint();
                        }

                        if (pointIndex != pointsProperty.arraySize -1)
                        {
                            DrawRightControlPoint();
                        }
                    }

                    void DrawPoint()
                    {
                        painter.fillColor = isStaticPoint ? Color.yellow : Color.white;

                        painter.BeginPath();
                        painter.Arc(pointPos, PointSize * ViewZoom, 0f, 360f);
                        painter.Fill();
                        painter.ClosePath();
                    }

                    void DrawRightControlPoint()
                    {
                        painter.fillColor = Color.green;

                        painter.BeginPath();
                        painter.Arc(rightControlPointPos, ControlPointSize * ViewZoom, 0f, 360f);
                        painter.Fill();
                        painter.ClosePath();
                    }

                    void DrawLeftControlPoint()
                    {
                        painter.fillColor = Color.green;

                        painter.BeginPath();
                        painter.Arc(leftControlPointPos, ControlPointSize * ViewZoom, 0f, 360f);
                        painter.Fill();
                        painter.ClosePath();
                    }
                }
            }

            void DrawControlPointsConnection()
            {
                painter.strokeColor = Color.white;
                painter.BeginPath();

                for (int pointIndex = 0; pointIndex < pointsProperty.arraySize - 1; pointIndex++)
                {
                    BezierPoint point = (BezierPoint)pointsProperty.GetArrayElementAtIndex(pointIndex).boxedValue;
                    BezierPoint nextPoint = (BezierPoint)pointsProperty.GetArrayElementAtIndex(pointIndex + 1).boxedValue;

                    Vector2 pointPos = PointToViewPos(point.Position) + EdgeViewOrigin;
                    Vector2 rightControlPoint = pointPos + PointToViewPos(point.RightControlPoint);
                    Vector2 nextPointPos = PointToViewPos(nextPoint.Position) + EdgeViewOrigin;
                    Vector2 leftControlPoint = nextPointPos + PointToViewPos(nextPoint.LeftControlPoint);

                    if (pointIndex == 0)
                    {
                        painter.MoveTo(pointPos);
                    }

                    painter.LineTo(rightControlPoint);
                    painter.LineTo(leftControlPoint);
                    painter.LineTo(nextPointPos);
                }

                painter.Stroke();
                painter.ClosePath();
            }

            void DrawCurves()
            {
                painter.BeginPath();
                painter.lineWidth = CurveWidth * ViewZoom;
                painter.lineCap = LineCap.Round;
                painter.strokeColor = CurveColor;

                for (int pointIndex = 0; pointIndex < pointsProperty.arraySize - 1; pointIndex++)
                {
                    BezierPoint point = (BezierPoint)pointsProperty.GetArrayElementAtIndex(pointIndex).boxedValue;
                    BezierPoint nextPoint = (BezierPoint)pointsProperty.GetArrayElementAtIndex(pointIndex + 1).boxedValue;

                    Vector2 pointPos = PointToViewPos(point.Position) + EdgeViewOrigin;
                    Vector2 rightControlPoint = pointPos + PointToViewPos(point.RightControlPoint);
                    Vector2 nextPointPos = PointToViewPos(nextPoint.Position) + EdgeViewOrigin;
                    Vector2 leftControlPoint = nextPointPos + PointToViewPos(nextPoint.LeftControlPoint);

                    if (pointIndex == 0)
                    {
                        painter.MoveTo(pointPos);
                    }

                    painter.BezierCurveTo(rightControlPoint, leftControlPoint, nextPointPos);
                }

                painter.Stroke();
                painter.ClosePath();
            }
        }

        private Vector2 PointToViewPos(Vector3 pointPos)
        {
            return (new Vector2(pointPos.x, -pointPos.y) * ViewZoom);
        }

        private Vector2 ViewPosToPoint(Vector2 viewPos)
        {
            Vector2 pointPos = (new Vector2(viewPos.x, viewPos.y) - EdgeViewOrigin) / ViewZoom;
            return new Vector2(pointPos.x, -pointPos.y);
        }
    }
}
