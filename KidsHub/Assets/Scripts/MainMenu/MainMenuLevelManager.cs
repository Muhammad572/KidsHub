// using UnityEngine;
// using UnityEngine.SceneManagement;

// public class MainMenuLevelManager : MonoBehaviour
// {
//     public static MainMenuLevelManager Instance;

//     void Awake()
//     {
//         // Optional: make it a singleton if you want to reuse across scenes
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//         }
//     }

//     // // Load scene by build index
//     // public void LoadLevel(int index)
//     // {
//     //     if (index < 0 || index >= SceneManager.sceneCountInBuildSettings)
//     //     {
//     //         Debug.LogError("Invalid scene index: " + index);
//     //         return;
//     //     }

//     //     SceneManager.LoadScene(index);
//     // }

//     // Load scene by name (optional helper)
//     public void LoadLevel(int scene)
//     {
//         SceneManager.LoadScene(scene);
//     }

//     // Go back to Main Menu
//     public void LoadMainMenu()
//     {
//         SceneManager.LoadScene("Main");
//     }

//     // Quit game
//     public void QuitGame()
//     {
//         Debug.Log("Quitting game...");
//         Application.Quit();
//     }
// }


using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuLevelManager : MonoBehaviour
{
    public void LoadLevel(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    public void LoadMainMenu()
    {
        // 🧽 Stop any ongoing music before loading the menu
        if (AudioManager.Instance != null)
            // AudioManager.Instance.StopAllMusic();
            AudioManager.Instance.StopAllMusic();
            
        SceneManager.LoadScene("Main");
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}
