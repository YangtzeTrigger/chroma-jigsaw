using System;
using System.Collections.Generic;
using UnityEngine;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.Data
{
    public enum PackUnlockResult { Owned, XPLocked, IAPLocked, ZenPassLocked }

    public class PackUnlockService : MonoBehaviour
    {
        private static PackUnlockService _instance;
        public static PackUnlockService Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<PackUnlockService>();
                if (_instance == null) { var go = new GameObject("[PackUnlockService]"); _instance = go.AddComponent<PackUnlockService>(); }
                return _instance;
            }
        }

        // Wire in Inspector alongside GalleryController, or call SetRegistry() from GalleryController.Awake()
        [SerializeField] private PackRegistry _registry;

        // productId → packId, populated per-session by InitiatePurchase
        private readonly Dictionary<string, string> _productToPackId = new();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            IAPManager.OnPackPurchased += OnPackPurchasedExternally;
        }

        private void OnDestroy()
        {
            IAPManager.OnPackPurchased -= OnPackPurchasedExternally;
        }

        // Called from GalleryController if registry is not wired in Inspector
        public void SetRegistry(PackRegistry registry) => _registry = registry;

        public PackUnlockResult Evaluate(PackConfigSO config, out string lockedSubLabel)
        {
            lockedSubLabel = string.Empty;

            if (config.unlockType == PackUnlockType.Free)
                return PackUnlockResult.Owned;

            var save     = SaveManager.Instance.Data;
            var progress = save.packs.Find(p => p.packId == config.packId);

            if (progress != null && progress.isOwned)
                return PackUnlockResult.Owned;

            switch (config.unlockType)
            {
                case PackUnlockType.XP:
                    if (save.totalXP >= config.xpRequired)
                    {
                        GrantOwnership(config.packId);
                        return PackUnlockResult.Owned;
                    }
                    lockedSubLabel = $"UNLOCK AT {config.xpRequired} XP";
                    return PackUnlockResult.XPLocked;

                case PackUnlockType.IAP:
                    var price = IAPManager.Instance.GetLocalizedPrice(config.iapProductId);
                    lockedSubLabel = string.IsNullOrEmpty(price) ? "BUY PACK" : price;
                    return PackUnlockResult.IAPLocked;

                case PackUnlockType.ZenPass:
                    if (ZenPassService.Instance.IsZenPass)
                    {
                        GrantOwnership(config.packId);
                        return PackUnlockResult.Owned;
                    }
                    lockedSubLabel = "ZEN PASS";
                    return PackUnlockResult.ZenPassLocked;

                default:
                    return PackUnlockResult.Owned;
            }
        }

        public void InitiatePurchase(PackConfigSO config, Action<bool> onResult)
        {
            // Record mapping so OnPackPurchasedExternally can match IAP event → packId
            _productToPackId[config.iapProductId] = config.packId;
            IAPManager.Instance.PurchasePack(config.iapProductId, onResult);
        }

        private void OnPackPurchasedExternally(string productId)
        {
            // Try session-local mapping first (normal purchase flow)
            if (_productToPackId.TryGetValue(productId, out var packId))
            {
                GrantOwnership(packId);
                return;
            }

            // Fallback: scan registry (handles cold-start restore or cross-session IAP events)
            if (_registry == null) return;
            foreach (var config in _registry.packs)
            {
                if (config.iapProductId == productId)
                {
                    GrantOwnership(config.packId);
                    return;
                }
            }
        }

        private void GrantOwnership(string packId)
        {
            var save = SaveManager.Instance.Data;
            var p    = save.packs.Find(x => x.packId == packId);
            if (p == null)
            {
                p = new PackProgress { packId = packId };
                save.packs.Add(p);
            }
            if (!p.isOwned)
            {
                p.isOwned = true;
                SaveManager.Instance.Save();
            }
        }
    }
}
