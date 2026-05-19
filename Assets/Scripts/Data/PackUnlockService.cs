using System;
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

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

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
                    lockedSubLabel = "BUY PACK"; // IAP: replace with price string from billing SDK in B-08
                    return PackUnlockResult.IAPLocked;

                case PackUnlockType.ZenPass:
                    lockedSubLabel = "ZEN PASS";
                    return PackUnlockResult.ZenPassLocked;

                default:
                    return PackUnlockResult.Owned;
            }
        }

        // IAP: wire real purchase flow in B-08
        public void InitiatePurchase(PackConfigSO config, Action<bool> onResult)
        {
#if UNITY_EDITOR
            Debug.Log($"[PackUnlockService] IAP stub — would purchase '{config.iapProductId}'");
#endif
            onResult?.Invoke(false);
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
            p.isOwned = true;
            SaveManager.Instance.Save();
        }
    }
}
