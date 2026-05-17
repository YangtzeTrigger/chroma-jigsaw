using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using ChromaJigsaw.Core; // SaveManager, BundleLoader, PackProgress

namespace ChromaJigsaw.Data
{
    public class PackManager : MonoBehaviour
    {
        private static PackManager _instance;
        public static PackManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<PackManager>();
                if (_instance == null) { var go = new GameObject("[PackManager]"); _instance = go.AddComponent<PackManager>(); }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public PackCatalogueData Catalogue  { get; private set; }
        public bool              IsReady    { get; private set; }

        public event Action OnCatalogueReady;

        // ── Catalogue ───────────────────────────────────────────────────────

        public void FetchCatalogue()
        {
            StartCoroutine(FetchCatalogueRoutine());
        }

        private IEnumerator FetchCatalogueRoutine()
        {
            using var req = UnityWebRequest.Get(BundleConfig.CatalogueUrl);
            yield return req.SendWebRequest();

            if (req.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"[PackManager] Catalogue fetch failed: {req.error}");
                yield break;
            }

            Catalogue = JsonUtility.FromJson<PackCatalogueData>(req.downloadHandler.text);
            SyncCatalogueWithSave();
            IsReady = true;
            OnCatalogueReady?.Invoke();
        }

        private void SyncCatalogueWithSave()
        {
            if (Catalogue == null) return;
            var save = SaveManager.Instance.Data;
            foreach (var pack in Catalogue.packs)
            {
                var progress = GetOrCreateProgress(pack.packId);
                pack.isOwned                   = progress.isOwned;
                pack.grandMasterpieceUnlocked  = progress.grandMasterpieceUnlocked;
                pack.completionBadge           = pack.imageIds != null &&
                                                 pack.imageIds.All(id => progress.completedImageIds.Contains(id));
            }
        }

        // ── Ownership ───────────────────────────────────────────────────────

        public bool IsOwned(string packId) => GetProgress(packId)?.isOwned ?? false;

        public void GrantOwnership(string packId)
        {
            var p    = GetOrCreateProgress(packId);
            p.isOwned = true;
            SaveManager.Instance.Save();

            var def = GetDefinition(packId);
            if (def != null) def.isOwned = true;
        }

        // ── Completion ──────────────────────────────────────────────────────

        public bool IsImageCompleted(string packId, string imageId) =>
            GetProgress(packId)?.completedImageIds.Contains(imageId) ?? false;

        public void MarkImageCompleted(string packId, string imageId)
        {
            var p = GetOrCreateProgress(packId);
            if (p.completedImageIds.Contains(imageId)) return;
            p.completedImageIds.Add(imageId);

            var def = GetDefinition(packId);
            if (def != null)
            {
                def.completionBadge = def.imageIds != null &&
                                      def.imageIds.All(id => p.completedImageIds.Contains(id));

                if (def.completionBadge && !p.grandMasterpieceUnlocked)
                {
                    p.grandMasterpieceUnlocked = true;
                    def.grandMasterpieceUnlocked = true;
                }
            }

            SaveManager.Instance.Save();
        }

        // ── Bundle Loading ──────────────────────────────────────────────────

        public void LoadPackBundle(string packId, Action<AssetBundle> onComplete) =>
            BundleLoader.Instance.LoadPackBundle(packId, onComplete);

        public void DownloadSouvenir(string packId, string imageId, Action<byte[]> onComplete) =>
            BundleLoader.Instance.DownloadSouvenir(packId, imageId, onComplete);

        // ── Helpers ─────────────────────────────────────────────────────────

        public PackDefinition GetDefinition(string packId) =>
            Catalogue?.packs?.FirstOrDefault(p => p.packId == packId);

        private PackProgress GetProgress(string packId) =>
            SaveManager.Instance.Data.packs?.FirstOrDefault(p => p.packId == packId);

        private PackProgress GetOrCreateProgress(string packId)
        {
            var data = SaveManager.Instance.Data;
            var p    = data.packs.FirstOrDefault(x => x.packId == packId);
            if (p == null)
            {
                p = new PackProgress { packId = packId };
                data.packs.Add(p);
            }
            return p;
        }
    }
}
