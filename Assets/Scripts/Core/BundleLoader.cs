using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

namespace ChromaJigsaw.Core
{
    public class BundleLoader : MonoBehaviour
    {
        private static BundleLoader _instance;
        public static BundleLoader Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<BundleLoader>();
                if (_instance == null) { var go = new GameObject("[BundleLoader]"); _instance = go.AddComponent<BundleLoader>(); }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private static string CacheDir        => Path.Combine(Application.persistentDataPath, "bundles");
        private static string StreamingDir    => Path.Combine(Application.streamingAssetsPath, "bundles");

        // Loads a pack AssetBundle.
        // Priority: 1) StreamingAssets (onboarding pack, ships with APK)
        //           2) Local cache (previously downloaded)
        //           3) Remote download from Elysium
        public void LoadPackBundle(string packId, Action<AssetBundle> onComplete)
        {
            StartCoroutine(LoadRoutine(packId, onComplete));
        }

        private IEnumerator LoadRoutine(string packId, Action<AssetBundle> onComplete)
        {
            // 1. StreamingAssets — onboarding pack ships with the APK
            string streamingPath = Path.Combine(StreamingDir, $"{packId}.bundle");
            string streamingUrl  = $"{Application.streamingAssetsPath}/bundles/{packId}.bundle";
            using (var streamReq = UnityWebRequestAssetBundle.GetAssetBundle(streamingUrl))
            {
                yield return streamReq.SendWebRequest();
                if (streamReq.result == UnityWebRequest.Result.Success)
                {
                    onComplete?.Invoke(DownloadHandlerAssetBundle.GetContent(streamReq));
                    yield break;
                }
            }

            // 2. Local cache
            string cachedPath = Path.Combine(CacheDir, $"{packId}.bundle");
            if (File.Exists(cachedPath))
            {
                var loadOp = AssetBundle.LoadFromFileAsync(cachedPath);
                yield return loadOp;
                onComplete?.Invoke(loadOp.assetBundle);
                yield break;
            }

            // 3. Remote download from Elysium
            string url = BundleConfig.PackBundleUrl(packId);
            using var req = UnityWebRequestAssetBundle.GetAssetBundle(url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[BundleLoader] Failed to download {packId}: {req.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            var bundle = DownloadHandlerAssetBundle.GetContent(req);
            CacheBundle(packId, req.downloadHandler.data);
            onComplete?.Invoke(bundle);
        }

        // Downloads a raw 4K souvenir JPEG for a given image.
        public void DownloadSouvenir(string packId, string imageId, Action<byte[]> onComplete)
        {
            StartCoroutine(SouvenirRoutine(packId, imageId, onComplete));
        }

        private IEnumerator SouvenirRoutine(string packId, string imageId, Action<byte[]> onComplete)
        {
            string url = BundleConfig.SouvenirUrl(packId, imageId);
            using var req = UnityWebRequest.Get(url);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[BundleLoader] Souvenir download failed {imageId}: {req.error}");
                onComplete?.Invoke(null);
                yield break;
            }

            onComplete?.Invoke(req.downloadHandler.data);
        }

        private static void CacheBundle(string packId, byte[] data)
        {
            try
            {
                Directory.CreateDirectory(CacheDir);
                File.WriteAllBytes(Path.Combine(CacheDir, $"{packId}.bundle"), data);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[BundleLoader] Cache write failed: {e.Message}");
            }
        }

        public static void ClearCache(string packId)
        {
            string path = Path.Combine(CacheDir, $"{packId}.bundle");
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
