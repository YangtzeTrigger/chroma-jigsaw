using System;
using UnityEngine;
using ChromaJigsaw.Core;

namespace ChromaJigsaw.Ads
{
    public class AdsManager : MonoBehaviour
    {
        private static AdsManager _instance;
        public static AdsManager Instance
        {
            get
            {
                if (_instance == null) _instance = FindAnyObjectByType<AdsManager>();
                if (_instance == null) { var go = new GameObject("[AdsManager]"); _instance = go.AddComponent<AdsManager>(); }
                return _instance;
            }
        }

        private const float InterstitialCooldown = 60f;
        private float _lastInterstitialTime = -InterstitialCooldown;

        private void Awake()
        {
            if (_instance != null && _instance != this) { Destroy(gameObject); return; }
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitAdMob();
        }

        private void InitAdMob()
        {
            // ADMOB: uncomment when Google Mobile Ads SDK is imported.
            // MobileAds.Initialize(_ => { });
        }

        public void ShowInterstitial(string placement = "default")
        {
            if (ZenPassService.Instance.IsZenPass) return;
            if (Time.realtimeSinceStartup - _lastInterstitialTime < InterstitialCooldown) return;
            _lastInterstitialTime = Time.realtimeSinceStartup;

            // ADMOB: _interstitialAd.Show();
#if UNITY_EDITOR
            Debug.Log($"[Ads] Interstitial — placement: {placement}");
#endif
        }

        public void ShowRewarded(string placement, Action onRewarded)
        {
            if (ZenPassService.Instance.IsZenPass) { onRewarded?.Invoke(); return; }

            // ADMOB: _rewardedAd.Show(_ => onRewarded?.Invoke());
#if UNITY_EDITOR
            Debug.Log($"[Ads] Rewarded — placement: {placement}");
            onRewarded?.Invoke();
#endif
        }

        public void ShowBanner(bool show)
        {
            if (ZenPassService.Instance.IsZenPass) show = false;

            // ADMOB: if (show) _bannerAd.Show(); else _bannerAd.Hide();
#if UNITY_EDITOR
            Debug.Log($"[Ads] Banner {(show ? "shown" : "hidden")}");
#endif
        }
    }
}
