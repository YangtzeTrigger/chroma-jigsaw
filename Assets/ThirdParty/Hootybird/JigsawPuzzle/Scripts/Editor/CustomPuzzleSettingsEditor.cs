using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using HootyBird.JigsawPuzzleEngine.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

namespace HootyBird.JigsawPuzzleEngine.Editor
{
    [CustomEditor(typeof(PuzzleSettingsObject))]
    public class CustomPuzzleSettingsEditor : UnityEditor.Editor
    {
        private static Vector2 ViewPadding = new Vector2(4f, 4f);

        public VisualTreeAsset editorAsset;

        private float puzzleUnitSize;
        private Vector2 viewOrigin;

        private Button resetButton;
        private Button resetPuzzle;
        private DropdownField saveOption;
        private VisualElement puzzleView;

        private SerializedProperty puzzleSettingsProperty;
        private SerializedProperty rowsProperty;
        private SerializedProperty columnsProperty;
        private SerializedProperty seedProperty;
        private SerializedProperty idProperty;
        private SerializedProperty settingsEdgesProperty;
        private SerializedProperty edgesProperty;
        private PuzzleData puzzleData;

        public override VisualElement CreateInspectorGUI()
        {
            VisualElement root = editorAsset.CloneTree();

            resetButton = root.Q<Button>("reset-seed");
            resetButton.clicked += ResetSeed;

            resetPuzzle = root.Q<Button>("reset-puzzle");
            resetPuzzle.clicked += GeneratePuzzle;

            puzzleView = root.Q<VisualElement>("puzzle-view");
            puzzleView.RegisterCallback<GeometryChangedEvent>(PuzzleViewSizeUpdate);

            saveOption = root.Q<DropdownField>("save-options");
            saveOption.RegisterValueChangedCallback(changeEvent => OnSaveOptionSelected(changeEvent.newValue));

            root.Add(puzzleView);

            Init();

            return root;
        }

        private void Init()
        {
            puzzleSettingsProperty = serializedObject.FindProperty("puzzleSettings");
            idProperty = puzzleSettingsProperty.FindPropertyRelative("id");
            seedProperty = puzzleSettingsProperty.FindPropertyRelative("seed");
            rowsProperty = puzzleSettingsProperty.FindPropertyRelative("rows");
            columnsProperty = puzzleSettingsProperty.FindPropertyRelative("columns");
            settingsEdgesProperty = puzzleSettingsProperty.FindPropertyRelative("edgeOptions");
            edgesProperty = serializedObject.FindProperty("edgeOptions");

            if (string.IsNullOrEmpty(idProperty.stringValue))
            {
                SetId();
            }

            if (seedProperty.intValue == 0)
            {
                ResetSeed();
            }

            if (puzzleData == null)
            {
                GeneratePuzzle();
            }
        }

        private void PuzzleViewSizeUpdate(GeometryChangedEvent evt)
        {
            ResetPolygonsView();
        }

        private void ResetPolygonsView()
        {
            puzzleView.Clear();

            int rows = puzzleData.rows;
            int columns = puzzleData.columns;

            if (rows * columns < 1000)
            {
                puzzleUnitSize = (puzzleView.layout.width - ViewPadding.x * 2f) / columns;
                viewOrigin = Vector2.one * puzzleUnitSize * .5f;

                for (int rowIndex = 0; rowIndex < rows; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < columns; columnIndex++)
                    {
                        VisualElement polygonView = new VisualElement();
                        polygonView.name = $"R {rowIndex} C {columnIndex}";
                        StyleLength polygonSize = new StyleLength(new Length(puzzleUnitSize, LengthUnit.Pixel));
                        polygonView.style.width = polygonSize;
                        polygonView.style.height = polygonSize;
                        polygonView.userData = new Vector2Int(columnIndex, rowIndex);

                        polygonView.generateVisualContent += DrawPuzzlePiece;

                        puzzleView.Add(polygonView);
                    }
                }
            }
            else
            {
                puzzleView.Add(new Label("Puzzle dimentions are too big to visualize."));
            }
        }

        private void ResetSeed()
        {
            seedProperty.intValue = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            serializedObject.ApplyModifiedProperties();
        }

        private void SetId()
        {
            idProperty.stringValue = Guid.NewGuid().ToString();
            serializedObject.ApplyModifiedProperties();
        }

        private void GeneratePuzzle()
        {
            settingsEdgesProperty.ClearArray();
            List<EdgeObject> edges = new List<EdgeObject>();
            for (int edgeAssetIndex = 0; edgeAssetIndex < edgesProperty.arraySize; edgeAssetIndex++)
            {
                // Collect all edge options from editor edges list.
                EdgeObject edgeObject = (EdgeObject)edgesProperty.GetArrayElementAtIndex(edgeAssetIndex).boxedValue;
                if (edgeObject == null) continue;

                edges.Add(edgeObject);

                edgeObject.edge.UpdateLength();
                // Update puzzle settings edge options.
                settingsEdgesProperty.InsertArrayElementAtIndex(edgeAssetIndex);
                settingsEdgesProperty.GetArrayElementAtIndex(edgeAssetIndex).boxedValue = edgeObject.edge;
            }

            puzzleData = PuzzleFactory.FromEdgePresets(
                rowsProperty.intValue,
                columnsProperty.intValue,
                edges.Select(edgeAsset => edgeAsset.edge),
                seedProperty.intValue);
            serializedObject.ApplyModifiedProperties();

            ResetPolygonsView();
        }

        private void DrawPuzzlePiece(MeshGenerationContext ctx)
        {
            Vector2Int location = (Vector2Int)ctx.visualElement.userData;

            Painter2D painter = ctx.painter2D;
            painter.strokeColor = Color.white;
            painter.lineCap = LineCap.Round;

            Polygon polygon = puzzleData.polygons[location.y * puzzleData.columns + location.x];

            ctx.DrawText(
                $"C: {polygon.location.x} R: {polygon.location.y}",
                viewOrigin + new Vector2(-puzzleUnitSize * .4f, 0f),
                puzzleUnitSize * .15f,
                Color.black);

            for (int edgeIndex = 0; edgeIndex < polygon.EdgeCount; edgeIndex++)
            {
                Edge edge = polygon[edgeIndex];
                painter.BeginPath();

                for (int pointIndex = 0; pointIndex < edge.points.Length - 1; pointIndex++)
                {
                    BezierPoint point = edge.points[pointIndex];
                    BezierPoint nextPoint = edge.points[pointIndex + 1];

                    Vector2 pointPos = PointToViewPos(point.Position) + viewOrigin;
                    Vector2 rightControlPoint = pointPos + PointToViewPos(point.RightControlPoint);
                    Vector2 nextPointPos = PointToViewPos(nextPoint.Position) + viewOrigin;
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

            Vector2 PointToViewPos(Vector2 pointPos)
            {
                return new Vector2(pointPos.x, -pointPos.y) * puzzleUnitSize;
            }
        }

        private void OnSaveOptionSelected(string option)
        {
            if (saveOption.choices[0] == option)
            {
                SavePuzzleDataAsAssetFile();
            }
            else
            {
                SavePuzzleDataAsJson();
            }
        }

        private void SavePuzzleDataAsAssetFile()
        {
            string path = EditorUtility.SaveFilePanel(
                "Save Puzzle Data as Asset File",
                "",
                "puzzleData.asset",
                "asset");

            if (path.Length != 0)
            {
                string relativePath = path.Substring(path.IndexOf("Assets/"));
                SerializedPuzzleDataObject puzzleObject = CreateInstance<SerializedPuzzleDataObject>();
                puzzleObject.puzzleData = puzzleData;

                AssetDatabase.CreateAsset(puzzleObject, relativePath);
            }
        }

        private void SavePuzzleDataAsJson()
        {
            string path = EditorUtility.SaveFilePanel(
                "Save Puzzle Data as Json File",
                "",
                "puzzleData.json",
                "json");

            if (path.Length != 0)
            {
                string data = JsonUtility.ToJson(puzzleData);
                if (!string.IsNullOrEmpty(data))
                {
                    File.WriteAllText(path, data);
                    AssetDatabase.Refresh();
                }
            }
        }
    }
}
