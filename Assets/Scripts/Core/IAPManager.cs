using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace ChromaJigsaw.Core
{
    public class IAPManager : MonoBehaviour, IDetailedStoreListener
    {
        private static IAPManager _instance;
        public static IAPManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<IAPManager>();
                if (_instance == null) { var go = new GameObject("[IAPManager]"); _instance = go.AddComponent<IAPManager>(); }
                return _instance;
            }
        }

        // Product IDs — must match Google Play Console exactly
        public const string ProductZenPassLifetime    = "com.yourstudio.chromajigsaw.zenpass.lifetime";
        public const string ProductZenPassMonthly     = "com.yourstudio.chromajigsaw.zenpass.monthly";

        [SerializeField] private string[] _additionalProductIds; // pack IAP IDs set in Inspector

        // Fired when a pack purchase completes — PackUnlockService (Data) subscribes
        public static event Action<string> OnPackPurchased;

        private IStoreController  _storeController;
        private IExtensionProvider _extensions;
        private readonly Dictionary<string, Action<bool>> _pendingCallbacks = new();

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitializeIAP()
        {
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            builder.AddProduct(ProductZenPassLifetime, ProductType.NonConsumable);
            builder.AddProduct(ProductZenPassMonthly,  ProductType.Subscription);

            if (_additionalProductIds != null)
            {
                foreach (var id in _additionalProductIds)
                {
                    if (!string.IsNullOrEmpty(id))
                        builder.AddProduct(id, ProductType.NonConsumable);
                }
            }

            UnityPurchasing.Initialize(this, builder);
        }

        // ── Pack purchase (called by PackUnlockService via UI) ──────────────────

        public void PurchasePack(string productId, Action<bool> onResult)
        {
            if (_storeController == null) { onResult?.Invoke(false); return; }
            _pendingCallbacks[productId] = onResult;
            _storeController.InitiatePurchase(productId);
        }

        // ── ZenPass purchases ───────────────────────────────────────────────────

        public void PurchaseZenPassLifetime(Action<bool> onResult)
        {
            if (_storeController == null) { onResult?.Invoke(false); return; }
            _pendingCallbacks[ProductZenPassLifetime] = onResult;
            _storeController.InitiatePurchase(ProductZenPassLifetime);
        }

        public void PurchaseZenPassMonthly(Action<bool> onResult)
        {
            if (_storeController == null) { onResult?.Invoke(false); return; }
            _pendingCallbacks[ProductZenPassMonthly] = onResult;
            _storeController.InitiatePurchase(ProductZenPassMonthly);
        }

        public void RestorePurchases()
        {
            if (_extensions == null) return;
#if UNITY_IOS
            _extensions.GetExtension<IAppleExtensions>().RestoreTransactions(result =>
            {
#if UNITY_EDITOR
                Debug.Log($"[IAPManager] RestoreTransactions result: {result}");
#endif
            });
#elif UNITY_ANDROID
            _extensions.GetExtension<IGooglePlayStoreExtensions>().RestoreTransactions(result =>
            {
#if UNITY_EDITOR
                Debug.Log($"[IAPManager] RestoreTransactions result: {result}");
#endif
            });
#endif
        }

        public string GetLocalizedPrice(string productId)
        {
            if (_storeController == null) return string.Empty;
            var product = _storeController.products.WithID(productId);
            return product != null ? product.metadata.localizedPriceString : string.Empty;
        }

        // ── IStoreListener ──────────────────────────────────────────────────────

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            _storeController = controller;
            _extensions      = extensions;

            // Re-validate persistent entitlements on every cold start
            CheckRestoredEntitlements();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[IAPManager] Init failed: {error}");
#endif
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"[IAPManager] Init failed: {error} — {message}");
#endif
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var id = args.purchasedProduct.definition.id;

            if (id == ProductZenPassLifetime)
                ZenPassService.Instance.GrantLifetime();
            else if (id == ProductZenPassMonthly)
                ZenPassService.Instance.GrantSubscriber();
            else
                OnPackPurchased?.Invoke(id);

            if (_pendingCallbacks.TryGetValue(id, out var cb))
            {
                _pendingCallbacks.Remove(id);
                cb?.Invoke(true);
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            var id = product.definition.id;
            if (_pendingCallbacks.TryGetValue(id, out var cb))
            {
                _pendingCallbacks.Remove(id);
                cb?.Invoke(false);
            }
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            OnPurchaseFailed(product, failureDescription.reason);
        }

        // ── Entitlement restoration ─────────────────────────────────────────────

        private void CheckRestoredEntitlements()
        {
            var lifetime = _storeController.products.WithID(ProductZenPassLifetime);
            if (lifetime != null && lifetime.hasReceipt)
                ZenPassService.Instance.GrantLifetime();

            var monthly = _storeController.products.WithID(ProductZenPassMonthly);
            if (monthly != null && monthly.hasReceipt)
                ZenPassService.Instance.GrantSubscriber();
        }
    }
}
