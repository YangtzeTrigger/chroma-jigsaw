using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using System;
using UnityEditor;
using UnityEngine;

namespace HootyBird.JigsawPuzzleEngine.Editor
{
    [CustomEditor(typeof(PuzzleInfoObject))]
    public class CustomPuzzleInfoEditor : UnityEditor.Editor
    {
        private SerializedProperty idProperty;

        private void OnEnable()
        {
            idProperty = serializedObject.FindProperty("id");

            if (string.IsNullOrEmpty(idProperty.stringValue))
            {
                idProperty.stringValue = Guid.NewGuid().ToString();
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.BeginHorizontal();
            {
                EditorGUILayout.LabelField("Id", GUILayout.MaxWidth(30));
                EditorGUILayout.TextArea(idProperty.stringValue);
            }
            GUILayout.EndHorizontal();
        }
    }
}
