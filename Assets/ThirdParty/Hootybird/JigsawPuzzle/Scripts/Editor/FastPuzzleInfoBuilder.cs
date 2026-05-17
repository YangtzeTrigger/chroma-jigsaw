using HootyBird.JigsawPuzzleEngine.Model;
using HootyBird.JigsawPuzzleEngine.ScriptableObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace HootyBird.JigsawPuzzleEngine.Editor
{
    public class FastPuzzleInfoBuilder : EditorWindow
    {
        public VisualTreeAsset editorAsset;

        [SerializeField]
        private List<PuzzleSettingsObject> easy = new List<PuzzleSettingsObject>();
        [SerializeField]
        private List<PuzzleSettingsObject> medium = new List<PuzzleSettingsObject>();
        [SerializeField]
        private List<PuzzleSettingsObject> hard = new List<PuzzleSettingsObject>();
        [SerializeField]
        private List<Texture> textures = new List<Texture>();

        [SerializeField]
        private string destinationPath;

        private SerializedObject serializedObject;
        private SerializedProperty destinationProperty;

        [MenuItem("Window/Batch Create Puzzles")]
        public static void ShowWindow()
        {
            GetWindow(typeof(FastPuzzleInfoBuilder));
        }

        private void CreateGUI()
        {
            serializedObject = new SerializedObject(this);
            destinationProperty = serializedObject.FindProperty(nameof(destinationPath));

            VisualElement root = editorAsset.CloneTree();

            SetupListView(nameof(easy));
            SetupListView(nameof(medium));
            SetupListView(nameof(hard));
            SetupListView(nameof(textures));

            TextField destination = root.Q<TextField>("destination");
            destination.bindingPath = nameof(destinationPath);
            destination.Bind(serializedObject);

            Button pickDestinationButton = root.Q<Button>("pick-to");
            pickDestinationButton.clicked += OnPickToClicked;

            Button buildButton = root.Q<Button>("build");
            buildButton.clicked += OnBuildClicked;

            rootVisualElement.Add(root);

            void SetupListView(string path)
            {
                ListView listView = root.Q<ListView>(path);
                listView.bindingPath = path;
                listView.makeItem = () => new PropertyField();

                listView.Bind(serializedObject);
            }
        }

        private void OnPickToClicked()
        {
            string path = EditorUtility.OpenFolderPanel("Pick Destination Path", destinationProperty.stringValue, "");

            if (!string.IsNullOrEmpty(path))
            {
                destinationProperty.stringValue = path;
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void OnBuildClicked()
        {
            string relativePath = destinationProperty.stringValue.Substring(destinationProperty.stringValue.IndexOf("Assets/"));

            for (int textureIndex = 0; textureIndex < textures.Count; textureIndex++)
            {
                PuzzleSettingsObject easySetting = easy.Count > 0 ? easy[UnityEngine.Random.Range(0, easy.Count)] : null;
                PuzzleSettingsObject mediumSetting = medium.Count > 0 ? medium[UnityEngine.Random.Range(0, medium.Count)] : null;
                PuzzleSettingsObject hardSetting = hard.Count > 0 ? hard[UnityEngine.Random.Range(0, hard.Count)] : null;

                PuzzleInfoObject newPuzzleInfo = PuzzleInfoObject.Create(textures[textureIndex], easySetting, mediumSetting, hardSetting);

                AssetDatabase.CreateAsset(newPuzzleInfo, $"{relativePath}/{newPuzzleInfo.Id}.asset");
                AssetDatabase.Refresh();
            }
        }
    }
}
