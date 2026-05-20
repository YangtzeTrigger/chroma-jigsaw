using UnityEngine;

namespace ChromaJigsaw.Core
{
    public class ZenPassService : MonoBehaviour
    {
        private static ZenPassService _instance;
        public static ZenPassService Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<ZenPassService>();
                if (_instance == null) { var go = new GameObject("[ZenPassService]"); _instance = go.AddComponent<ZenPassService>(); }
                return _instance;
            }
        }

        public bool IsZenPass => SaveManager.Instance.Data.zenPassLifetime ||
                                 SaveManager.Instance.Data.zenPassSubscriber;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialise()
        {
            // State is read directly from SaveData — no additional init needed.
            // Subscription liveness validated by IAPManager.CheckRestoredEntitlements()
            // after IAP initializes.
        }

        public void GrantLifetime()
        {
            var data = SaveManager.Instance.Data;
            if (data.zenPassLifetime) return;
            data.zenPassLifetime = true;
            SaveManager.Instance.Save();
        }

        public void GrantSubscriber()
        {
            var data = SaveManager.Instance.Data;
            if (data.zenPassSubscriber) return;
            data.zenPassSubscriber = true;
            SaveManager.Instance.Save();
        }

        // Called by SubscriptionService when Google Play confirms the sub has lapsed
        public void RevokeSubscriber()
        {
            var data = SaveManager.Instance.Data;
            if (!data.zenPassSubscriber) return;
            data.zenPassSubscriber = false;
            SaveManager.Instance.Save();
        }
    }
}
