using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace ChromaJigsaw.Editor
{
    public static class B01Setup
    {
        [MenuItem("ChromaJigsaw/Run B-01 Setup")]
        public static void RunAll()
        {
            SetScriptExecutionOrder();
            CreateScenes();
            CreateUrpAssets();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[B01Setup] Complete. Check Console for any warnings.");
        }

        // ── Script Execution Order ──────────────────────────────────────────

        static void SetScriptExecutionOrder()
        {
            SetOrder("AppBootstrap",    -100);
            SetOrder("SaveManager",      -90);
            SetOrder("AudioManager",     -80);
            SetOrder("AnalyticsManager", -70);
            SetOrder("AdsManager",       -60);
            Debug.Log("[B01Setup] Script Execution Order set.");
        }

        static void SetOrder(string scriptName, int order)
        {
            foreach (var guid in AssetDatabase.FindAssets($"{scriptName} t:MonoScript"))
            {
                var path   = AssetDatabase.GUIDToAssetPath(guid);
                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                if (script != null && script.name == scriptName)
                {
                    MonoImporter.SetExecutionOrder(script, order);
                    return;
                }
            }
            Debug.LogWarning($"[B01Setup] Could not find script: {scriptName}");
        }

        // ── Scenes ──────────────────────────────────────────────────────────

        static void CreateScenes()
        {
            string[] names    = { "_Bootstrap", "MainMenu", "Game", "Settings" };
            string   sceneDir = "Assets/Scenes";

            foreach (var name in names)
            {
                string path = $"{sceneDir}/{name}.unity";
                if (!File.Exists(path))
                {
                    var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                        UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                        UnityEditor.SceneManagement.NewSceneMode.Single);
                    UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, path);
                }
            }

            EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/_Bootstrap.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",   true),
                new EditorBuildSettingsScene("Assets/Scenes/Game.unity",       true),
                new EditorBuildSettingsScene("Assets/Scenes/Settings.unity",   true),
            };
            Debug.Log("[B01Setup] Scenes created and build order set.");
        }

        // ── Bootstrap Scene Population ──────────────────────────────────────

        [MenuItem("ChromaJigsaw/Populate _Bootstrap Scene")]
        static void PopulateBootstrapScene()
        {
            const string scenePath = "Assets/Scenes/_Bootstrap.unity";
            var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            AddSingleton<ChromaJigsaw.Core.AppBootstrap>("[AppBootstrap]");
            AddSingleton<ChromaJigsaw.Core.SaveManager>("[SaveManager]");
            AddSingleton<ChromaJigsaw.Audio.AudioManager>("[AudioManager]");
            AddSingleton<ChromaJigsaw.Data.AnalyticsManager>("[AnalyticsManager]");
            AddSingleton<ChromaJigsaw.Ads.AdsManager>("[AdsManager]");

            EditorSceneManager.SaveScene(scene);
            Debug.Log("[B01Setup] _Bootstrap scene populated and saved. Wire AudioMixer to [AudioManager] in the Inspector.");
        }

        static void AddSingleton<T>(string goName) where T : UnityEngine.Component
        {
            var existing = UnityEngine.Object.FindAnyObjectByType<T>();
            if (existing != null) return;
            var go = new GameObject(goName);
            go.AddComponent<T>();
        }

        // ── URP Assets ──────────────────────────────────────────────────────

        static void CreateUrpAssets()
        {
            string settingsDir   = "Assets/Settings";
            string rendererPath  = $"{settingsDir}/URP_Renderer.asset";
            string pipelinePath  = $"{settingsDir}/URP_PipelineAsset.asset";

            if (!File.Exists(rendererPath))
            {
                var rendererData = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(rendererData, rendererPath);
            }

            if (!File.Exists(pipelinePath))
            {
                var rendererData  = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
                var pipelineAsset = UniversalRenderPipelineAsset.Create(rendererData);
                AssetDatabase.CreateAsset(pipelineAsset, pipelinePath);

                GraphicsSettings.defaultRenderPipeline = pipelineAsset;
                QualitySettings.renderPipeline         = pipelineAsset;
            }

            Debug.Log("[B01Setup] URP assets created and assigned.");
        }
    }
}
