using UnityEngine;
using System.Collections;

public class LoadingScreenManager : MonoBehaviour
{
    [Header("Loading Screen Settings")]
    [Tooltip("Assign your loading screen prefab here.")]
    public GameObject loadingScreenPrefab;

    [Tooltip("How long the loading screen should stay visible (in seconds).")]
    public float loadingDuration = 2f;

    private GameObject loadingScreenInstance;

    private void Start()
    {
        if (loadingScreenPrefab != null)
        {
            // Spawn the loading screen at the start of the game
            loadingScreenInstance = Instantiate(loadingScreenPrefab);
            loadingScreenInstance.SetActive(true);
            StartCoroutine(HideLoadingScreenAfterDelay());
        }
        else
        {
            Debug.LogWarning("LoadingScreenPrefab is not assigned!");
        }
    }

    private IEnumerator HideLoadingScreenAfterDelay()
    {
        yield return new WaitForSeconds(loadingDuration);

        if (loadingScreenInstance != null)
        {
            loadingScreenInstance.SetActive(false);
        }
    }
}
