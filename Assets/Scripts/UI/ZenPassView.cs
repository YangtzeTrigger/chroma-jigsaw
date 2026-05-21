using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ChromaJigsaw.Audio;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.UI
{
    // ZenPass purchase screen. Never show mid-puzzle or in Sanctuary.
    // Trigger from: Settings "Manage" button, locked pack card tap.
    public class ZenPassView : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Prices")]
        [SerializeField] private TextMeshProUGUI _lifetimePriceText;
        [SerializeField] private TextMeshProUGUI _monthlyPriceText;

        [Header("Buttons")]
        [SerializeField] private Button _lifetimeButton;   // "ENTER THE SANCTUARY →"
        [SerializeField] private Button _monthlyButton;    // monthly CTA (optional secondary)
        [SerializeField] private Button _dismissButton;    // "Continue with sponsor support"
        [SerializeField] private Button _restoreButton;    // "Restore Purchases"

        [Header("State")]
        [SerializeField] private GameObject _loadingOverlay;

        private Action _onDismiss;

        private void Awake()
        {
            _lifetimeButton.onClick.AddListener(OnLifetimeTapped);
            _monthlyButton.onClick.AddListener(OnMonthlyTapped);
            _dismissButton.onClick.AddListener(Hide);
            _restoreButton.onClick.AddListener(OnRestoreTapped);
            gameObject.SetActive(false);
        }

        public void Show(Action onDismiss = null)
        {
            _onDismiss = onDismiss;
            gameObject.SetActive(true);
            _canvasGroup.alpha          = 1f;
            _canvasGroup.interactable   = true;
            _canvasGroup.blocksRaycasts = true;
            SetLoadingVisible(false);
            RefreshPrices();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            _onDismiss?.Invoke();
            _onDismiss = null;
        }

        private void RefreshPrices()
        {
            var lifetime = IAPManager.Instance.GetLocalizedPrice(IAPManager.ProductZenPassLifetime);
            var monthly  = IAPManager.Instance.GetLocalizedPrice(IAPManager.ProductZenPassMonthly);

            if (_lifetimePriceText != null)
                _lifetimePriceText.text = string.IsNullOrEmpty(lifetime) ? "$14.99 / Once" : $"{lifetime} / Once";

            if (_monthlyPriceText != null)
                _monthlyPriceText.text = string.IsNullOrEmpty(monthly) ? "$1.99 / Month" : $"{monthly} / Month";
        }

        private void OnLifetimeTapped()
        {
            SetLoadingVisible(true);
            _canvasGroup.interactable = false;
            IAPManager.Instance.PurchaseZenPassLifetime(OnLifetimeResult);
        }

        private void OnLifetimeResult(bool success)
        {
            SetLoadingVisible(false);
            _canvasGroup.interactable = true;
            if (success)
            {
                AudioManager.Instance.Play(SFXType.Reward);
                Hide();
            }
        }

        private void OnMonthlyTapped()
        {
            SetLoadingVisible(true);
            _canvasGroup.interactable = false;
            IAPManager.Instance.PurchaseZenPassMonthly(OnMonthlyResult);
        }

        private void OnMonthlyResult(bool success)
        {
            SetLoadingVisible(false);
            _canvasGroup.interactable = true;
            if (success)
            {
                AudioManager.Instance.Play(SFXType.Reward);
                Hide();
            }
        }

        private void OnRestoreTapped()
        {
            SetLoadingVisible(true);
            _canvasGroup.interactable = false;
            IAPManager.Instance.RestorePurchases();
            // IAP callbacks fire async; re-enable after a beat
            Invoke(nameof(ReenableAfterRestore), 3f);
        }

        private void ReenableAfterRestore()
        {
            SetLoadingVisible(false);
            _canvasGroup.interactable = true;
            // If ZenPass was restored the view can close
            if (ZenPassService.Instance.IsZenPass) Hide();
        }

        private void SetLoadingVisible(bool visible)
        {
            if (_loadingOverlay != null) _loadingOverlay.SetActive(visible);
        }
    }
}
