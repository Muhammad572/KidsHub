using System;
using UnityEngine;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using GoogleMobileAds;

public class Interstitial : MonoBehaviour
{
    public static Interstitial instance;

#if UNITY_ANDROID
    private const string _adUnitId = "ca-app-pub-3940256099942544/1033173712"; // Test Android ID
    // private const string _adUnitId = "ca-app-pub-8653293678103388/8324522236"; // Live ID
#elif UNITY_IPHONE
    private const string _adUnitId = "ca-app-pub-3940256099942544/4411468910"; // Test iOS ID
    // private const string _adUnitId = "ca-app-pub-8653293678103388/3307165123"; // Live ID
#else
    private const string _adUnitId = "unused";
#endif

    private InterstitialAd _interstitialAd;
    private Action _onAdClosed;

    // --- MAIN THREAD DISPATCHER (Necessary for AdMob Callbacks) ---
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    private static readonly object _queueLock = new object();

    public static void Run(Action action)
    {
        if (action == null) return;
        lock (_queueLock)
        {
            _executionQueue.Enqueue(action);
        }
    }

    private void Update()
    {
        lock (_queueLock)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Configure COPPA (Kids Safe)
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("✅ [AdMob] Initialized successfully.");
            LoadInterstitialAd();
        });
    }

    // LOAD INTERSTITIAL
    public void LoadInterstitialAd()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        Debug.Log("📢 Loading Interstitial Ad...");

        // Ensure ad is not loaded if no internet connection
        if (!IsInternetAvailable())
        {
            Debug.LogWarning("⚠️ Cannot load interstitial: No internet connection.");
            return;
        }
        
        var adRequest = new AdRequest();

        // Use the Load method with the static class
        InterstitialAd.Load(_adUnitId, adRequest, (InterstitialAd ad, LoadAdError error) =>
        {
            Run(() => // Dispatch to main thread
            {
                if (error != null || ad == null)
                {
                    Debug.LogError($"❌ Interstitial failed to load: {error?.GetMessage()}");
                    return;
                }

                _interstitialAd = ad;
                RegisterEventHandlers(ad);
                Debug.Log("✅ Interstitial ad loaded successfully.");
            });
        });
    }

    // CHECK IF READY
    public bool IsAdAvailable()
    {
        return _interstitialAd != null && _interstitialAd.CanShowAd();
    }

    // SHOW INTERSTITIAL
    public void ShowInterstitialAd(Action onClosed = null)
    {
        if (IsAdAvailable())
        {
            // **********************************************
            // FIX: Hide the banner ad before showing the interstitial
            // **********************************************
            if (BannerAdManager.instance != null)
            {
                Debug.Log("👀 Hiding Banner Ad to prevent UI conflict.");
                BannerAdManager.instance.HideBanner();
            }

            _onAdClosed = onClosed;
            Debug.Log("📢 Showing interstitial ad...");
            _interstitialAd.Show();
        }
        else
        {
            Debug.LogWarning("⚠️ Interstitial ad not ready. Attempting to reload and skipping action.");
            onClosed?.Invoke(); 
            LoadInterstitialAd(); 
        }
    }

    private void RegisterEventHandlers(InterstitialAd ad)
    {
        // Called when the ad is closed
        ad.OnAdFullScreenContentClosed += () =>
        {
            Run(() =>
            {
                Debug.Log("📴 Interstitial closed. Resuming game.");
                Time.timeScale = 1f;
                _onAdClosed?.Invoke();
                _onAdClosed = null;
                
                // **********************************************
                // FIX: Show the banner ad again after the interstitial closes
                // **********************************************
                if (BannerAdManager.instance != null)
                {
                    Debug.Log("👀 Showing Banner Ad again.");
                    BannerAdManager.instance.ShowBanner();
                }

                // Pre-load the next ad immediately
                LoadInterstitialAd();
            });
        };

        // Called when the ad fails to show
        ad.OnAdFullScreenContentFailed += (AdError error) =>
        {
            Run(() =>
            {
                Debug.LogError($"❌ Interstitial failed to show: {error.GetMessage()}");
                Time.timeScale = 1f;
                _onAdClosed?.Invoke();
                _onAdClosed = null;
                
                // **********************************************
                // FIX: Show the banner ad again if the interstitial failed
                // **********************************************
                if (BannerAdManager.instance != null)
                {
                    Debug.Log("👀 Showing Banner Ad again after failure.");
                    BannerAdManager.instance.ShowBanner();
                }

                // Pre-load the next ad immediately
                LoadInterstitialAd();
            });
        };

        // Called when the ad opens (game should pause)
        ad.OnAdFullScreenContentOpened += () =>
        {
            Run(() =>
            {
                Debug.Log("⏸️ Interstitial opened (game paused).");
                Time.timeScale = 0f;
            });
        };
        
        // Called when ad impression is recorded (optional, good for analytics)
        ad.OnAdImpressionRecorded += () =>
        {
            Run(() => Debug.Log("Impressions recorded successfully."));
        };
    }

    private void OnDestroy()
    {
        if (_interstitialAd != null)
        {
            _interstitialAd.Destroy();
            _interstitialAd = null;
        }

        if (instance == this)
            instance = null;
    }

    public bool IsInternetAvailable()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}