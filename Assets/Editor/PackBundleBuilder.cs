using System.IO;
using UnityEditor;
using UnityEngine;

namespace ChromaJigsaw.Editor
{
    public static class PackBundleBuilder
    {
        private const string PackSourceRoot   = "Assets/Art/Packs";
        private const string BundleOutputRoot = "Bundles"; // project-root/Bundles/, not tracked by git

        // The onboarding pack ships inside the APK via StreamingAssets.
        public const string OnboardingPackId  = "pack_001";

        [MenuItem("ChromaJigsaw/Build Pack Bundles (Android)")]
        public static void BuildAndroid()
        {
            Build(BuildTarget.Android);
        }

        public static void Build(BuildTarget target)
        {
            AssignBundleNames();

            string outputDir = Path.Combine(Directory.GetParent(Application.dataPath).FullName, BundleOutputRoot, target.ToString());
            Directory.CreateDirectory(outputDir);

            BuildPipeline.BuildAssetBundles(
                outputDir,
                BuildAssetBundleOptions.ChunkBasedCompression,
                target);

            // Copy onboarding bundle into StreamingAssets so it ships with the APK.
            string onboardingBundle = Path.Combine(outputDir, $"{OnboardingPackId}.bundle");
            if (File.Exists(onboardingBundle))
            {
                string dest = Path.Combine(Application.dataPath, $"StreamingAssets/bundles/{OnboardingPackId}.bundle");
                Directory.CreateDirectory(Path.GetDirectoryName(dest));
                File.Copy(onboardingBundle, dest, overwrite: true);
                Debug.Log("[PackBundleBuilder] Onboarding bundle copied to StreamingAssets.");
            }

            AssetDatabase.Refresh();
            Debug.Log($"[PackBundleBuilder] Bundles built to: {outputDir}");
        }

        // Scans Assets/Art/Packs/{packId}/ and assigns each texture to bundle "{packId}".
        [MenuItem("ChromaJigsaw/Assign Bundle Names")]
        public static void AssignBundleNames()
        {
            if (!AssetDatabase.IsValidFolder(PackSourceRoot))
            {
                Debug.LogWarning($"[PackBundleBuilder] Pack source folder not found: {PackSourceRoot}");
                return;
            }

            string[] packDirs = Directory.GetDirectories(PackSourceRoot);
            foreach (string packDir in packDirs)
            {
                string packId = Path.GetFileName(packDir);
                string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { $"{PackSourceRoot}/{packId}" });

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var importer     = AssetImporter.GetAtPath(assetPath);
                    if (importer != null) importer.assetBundleName = packId;
                }

                Debug.Log($"[PackBundleBuilder] Assigned {guids.Length} textures to bundle '{packId}'");
            }

            AssetDatabase.SaveAssets();
        }
    }
}
