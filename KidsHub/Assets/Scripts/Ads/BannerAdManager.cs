using UnityEngine;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine.SceneManagement;

/// <summary>
/// Persistent Banner Manager that keeps AdMob banner alive and visible
/// even after scene changes using a predictable, fixed-size banner.
/// </summary>
public class BannerAdManager : MonoBehaviour
{
    public static BannerAdManager instance;

#if UNITY_ANDROID
    // Test Android ID provided by Google AdMob
    private string _adUnitId = "ca-app-pub-3940256099942544/6300978111"; 
    // private string _adUnitId = "ca-app-pub-8653293678103388/5698358892"; // Original
#elif UNITY_IPHONE
    // Test iOS ID provided by Google AdMob
    private string _adUnitId = "ca-app-pub-3940256099942544/2934735716"; 
    // private string _adUnitId = "ca-app-pub-8653293678103388/3307165123"; // Original
#else
    private string _adUnitId = "unused";
#endif

    private BannerView _bannerView;

    private void Awake()
    {
        // Singleton pattern to ensure only one manager exists across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);

        // Subscribe to scene load events
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Configure COPPA: Tag for Child Directed Treatment
        RequestConfiguration requestConfiguration = new RequestConfiguration
        {
            TagForChildDirectedTreatment = TagForChildDirectedTreatment.True
        };
        MobileAds.SetRequestConfiguration(requestConfiguration);

        // Initialize SDK and request the first banner
        MobileAds.Initialize(initStatus =>
        {
            Debug.Log("✅ AdMob initialized");
            RequestBanner();
        });
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Example: Hide banner on scene 7 (e.g., a critical gameplay or credit scene)
        if (scene.buildIndex == 7)
        {
            HideBanner();
        }
        else
        {
            // Recreate and show banner for all other scenes
            Debug.Log($"🔄 Scene changed to: {scene.name}. Rebuilding banner...");
            RecreateBanner();
            ShowBanner();
        }
    }

    private void RecreateBanner()
    {
        DestroyBanner();
        // A small delay helps ensure the new scene's layout is fully calculated before requesting ad size
        Invoke(nameof(RequestBanner), 0.5f); 
    }

    private void RequestBanner()
    {
        // Destroy any previous view before creating a new one
        DestroyBanner(); 

        // 🌟 FIX FOR UNWANTED SPACE/OVERLAP: Switching to a fixed, standard banner size. 🌟
        // AdSize.Banner is a fixed size (e.g., 320x50 on mobile) which prevents layout shifts 
        // and large empty areas caused by variable adaptive banner heights.
        AdSize bannerSize = AdSize.Banner;
        
        // If you want to use a specific size for tablets, you can use AdSize.MediumRectangle (300x250)
        // or AdSize.Leaderboard (728x90) and check the device type.

        // Create the BannerView at the bottom center position
        _bannerView = new BannerView(_adUnitId, bannerSize, AdPosition.Bottom);

        RegisterEventHandlers(_bannerView);

        // Load the ad with a standard AdRequest
        AdRequest adRequest = new AdRequest();
        _bannerView.LoadAd(adRequest);

        Debug.Log("📢 Banner requested...");
    }

    public void ShowBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Show();
        }
        else
        {
            // If banner was destroyed or not yet requested, try again
            RequestBanner();
        }
    }

    public void HideBanner()
    {
        _bannerView?.Hide();
    }

    public void DestroyBanner()
    {
        if (_bannerView != null)
        {
            _bannerView.Destroy();
            _bannerView = null;
            Debug.Log("🗑️ Banner destroyed.");
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        DestroyBanner();
        if (instance == this) instance = null;
    }

    private void RegisterEventHandlers(BannerView bannerView)
    {
        bannerView.OnBannerAdLoaded += () =>
        {
            Debug.Log("✅ Banner loaded and visible");
        };

        bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
        {
            Debug.LogError($"❌ Banner failed to load: {error.GetMessage()}");
        };
    }
}