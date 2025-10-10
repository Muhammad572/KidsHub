using UnityEngine;

[ExecuteAlways]
public class AdaptiveCameraSize : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Assign your main orthographic Camera.")]
    public Camera targetCamera;

    [Header("Reference Settings")]
    [Tooltip("Orthographic size at reference aspect ratio (usually your design view).")]
    public float referenceOrthographicSize = 5f;

    [Tooltip("Aspect ratio (width / height) your game was designed for, e.g., 16:9 = 1.7777f.")]
    public float referenceAspect = 1.7777f; // 16:9 baseline

    [Header("Fit Mode")]
    [Tooltip("If true, ensures content always fits vertically (prevents top/bottom cutoff).")]
    public bool alwaysFitVertically = true;

    private float lastAspect = -1f;

    private void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        UpdateCameraSize();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
            UpdateCameraSize();
#endif

        float currentAspect = (float)Screen.width / Screen.height;

        if (Mathf.Abs(currentAspect - lastAspect) > 0.001f)
            UpdateCameraSize();
    }

    private void UpdateCameraSize()
    {
        if (targetCamera == null || !targetCamera.orthographic) return;

        float currentAspect = (float)Screen.width / Screen.height;
        lastAspect = currentAspect;

        float newSize = referenceOrthographicSize;

        if (alwaysFitVertically)
        {
            // Maintain full height (expand horizontally if needed)
            if (currentAspect < referenceAspect)
            {
                newSize = referenceOrthographicSize * (referenceAspect / currentAspect);
            }
        }
        else
        {
            // Maintain full width (expand vertically if needed)
            if (currentAspect > referenceAspect)
            {
                newSize = referenceOrthographicSize * (currentAspect / referenceAspect);
            }
        }

        targetCamera.orthographicSize = newSize;
    }
}
