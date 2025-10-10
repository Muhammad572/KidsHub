using UnityEngine;
using System.Collections;
using System; // Needed for Action

/// <summary>
/// Shakes an object horizontally (X-axis) for a specified duration and then executes a callback (Action).
/// This is used by the BubbleSpawner to ensure the shake completes before the bubble is launched.
/// </summary>
public class ObjectShaker : MonoBehaviour
{
    // --- Configuration ---

    [Tooltip("The time (in seconds) to wait after TriggerShake() is called before the shaking starts.")]
    public float pauseBeforeShake = 0.1f; // Shortened initial pause

    [Tooltip("The total duration (in seconds) the object will shake.")]
    public float shakeDuration = 0.2f; // Short burst before spawn

    [Tooltip("The maximum distance the object will move from its origin on the X-axis.")]
    public float shakeMagnitude = 0.1f;

    [Tooltip("The speed/frequency of the shake movement.")]
    public float shakeSpeed = 50.0f;

    // --- Private Variables ---

    private Vector3 originalPosition;
    private Coroutine activeShakeCoroutine = null;

    void Start()
    {
        // Store the starting local position of the object for reference
        originalPosition = transform.localPosition;
        
        // Removed automatic start. Shaking is now controlled entirely by the BubbleSpawner.
    }

    /// <summary>
    /// Initiates a delayed and duration-based shaking effect, executing a callback upon completion.
    /// Stops any currently active shake before starting a new one.
    /// </summary>
    /// <param name="onComplete">Action to execute after the shaking animation is finished and position is reset.</param>
    public void TriggerShake(Action onComplete = null)
    {
        // Ensure no multiple shake routines are running simultaneously
        if (activeShakeCoroutine != null)
        {
            StopCoroutine(activeShakeCoroutine);
            // Ensure position is reset if stopping an existing shake
            transform.localPosition = originalPosition;
        }

        // Start the coroutine and store a reference to it
        activeShakeCoroutine = StartCoroutine(DelayedShake(onComplete));
    }

    /// <summary>
    /// Coroutine that handles the initial pause, the timed shaking loop, and the callback.
    /// </summary>
    IEnumerator DelayedShake(Action onComplete)
    {
        // 1. Initial Pause: Wait for the specified duration (e.g., cannon winding up)
        yield return new WaitForSeconds(pauseBeforeShake);

        // 2. Shake Loop Setup
        float elapsedTime = 0f;
        float noiseSeed = UnityEngine.Random.Range(0f, 100f);

        // Shake loop: Runs until the total shakeDuration is reached (the "firing" moment)
        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;

            // Use Perlin Noise for smooth random movement
            float xNoise = Mathf.PerlinNoise(noiseSeed + Time.time * shakeSpeed, 0f) * 2f - 1f;
            float xOffset = xNoise * shakeMagnitude;

            transform.localPosition = originalPosition + new Vector3(xOffset, 0f, 0f);

            yield return null; // Wait until the next frame
        }

        // 3. Reset Position
        transform.localPosition = originalPosition;
        activeShakeCoroutine = null; // Clear the reference

        // 4. Execute Callback (Tell the Spawner we are ready to launch the bubble)
        onComplete?.Invoke();
    }
}
