// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class MainMenuLevelManager : MonoBehaviour
// {
//     public void LoadLevel(int scene)
//     {
//         SceneManager.LoadScene(scene);
//     }

//     public void LoadMainMenu()
//     {
//         // 🧽 Stop any ongoing music before loading the menu
//         if (AudioManager.Instance != null)
//             // AudioManager.Instance.StopAllMusic();
//             AudioManager.Instance.StopAllMusic();
            
//         SceneManager.LoadScene("Main");
//     }

//     public void QuitGame()
//     {
//         Debug.Log("Quitting game...");
//         Application.Quit();
//     }
// }


using UnityEngine;
using UnityEngine.SceneManagement;
using System; // Required for the Action delegate used in the ad callback

public class MainMenuLevelManager : MonoBehaviour
{
    void Start()
    {
         Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    public void LoadLevel(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadMainMenu()
    {
        // 1. Define the action that must happen AFTER the ad is handled.
        Action finishLoading = () =>
        {
            SceneManager.LoadScene("Main");
        };

        // 2. Stop any ongoing music immediately.
        if (AudioManager.Instance != null)
            AudioManager.Instance.StopAllMusic();

        // 3. Check if the ad is ready to show.
        if (Interstitial.instance != null && Interstitial.instance.IsAdAvailable())
        {
            Debug.Log("Interstitial Ad is available. Showing ad before loading Main Menu.");

            // Show the ad and pass the finishLoading action as the callback.
            // The scene will only load when the ad closes or fails.
            Interstitial.instance.ShowInterstitialAd(finishLoading);
        }
        else
        {
            // Ad is not available or Interstitial instance is missing, so load the scene immediately.
            Debug.Log("Interstitial Ad not ready. Loading Main Menu directly.");
            finishLoading.Invoke();
        }
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}