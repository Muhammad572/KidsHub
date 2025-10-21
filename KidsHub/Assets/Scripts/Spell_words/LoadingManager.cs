using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LoadingManager : MonoBehaviour
{
    public static LoadingManager Instance;

    [Header("Loading Panel Settings")]
    public GameObject loadingPanel;
    [Tooltip("How long the loading panel stays visible (seconds)")]
    public float loadingDuration = 2f;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
        else
            Debug.LogWarning("⚠️ No Loading Panel assigned to LoadingManager!");
    }

    public void ShowLoading(float duration = -1f)
    {
        if (loadingPanel == null) return;
        StopAllCoroutines();
        loadingPanel.SetActive(true);
        StartCoroutine(HideAfter(duration > 0 ? duration : loadingDuration));
    }

    private IEnumerator HideAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }
}
