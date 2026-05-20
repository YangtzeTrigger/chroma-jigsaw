using UnityEngine;

namespace ChromaJigsaw.Core
{
    // Handles monthly ZenPass subscription state validation.
    // Liveness is confirmed by Unity IAP on each cold start via IAPManager.CheckRestoredEntitlements().
    // POST-LAUNCH: loyalty conversion — users who accumulate 12 consecutive months offered lifetime upgrade.
    public class SubscriptionService : MonoBehaviour
    {
        private static SubscriptionService _instance;
        public static SubscriptionService Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<SubscriptionService>();
                if (_instance == null) { var go = new GameObject("[SubscriptionService]"); _instance = go.AddComponent<SubscriptionService>(); }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Initialise()
        {
            // Subscription liveness is re-validated each session by IAPManager after IAP init.
            // No local timer needed — Google Play is the source of truth.
            // POST-LAUNCH: loyalty conversion
        }
    }
}
