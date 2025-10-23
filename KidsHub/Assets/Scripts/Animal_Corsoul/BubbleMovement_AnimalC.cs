using UnityEngine;

public class BubbleMovement_AnimalC : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base speed the bubble moves up")]
    public float upwardSpeed = 2f;
    [Tooltip("Max horizontal drift magnitude")]
    public float driftMagnitude = 0.5f;
    [Tooltip("How quickly the bubble wiggles horizontally")]
    public float wiggleSpeed = 2f;
    
    [Header("Lifetime Settings")]
    [Tooltip("The Y position at which the bubble is destroyed")]
    public float despawnHeight = 10f;

    // A random value to make each bubble wiggle differently
    private float horizontalOffset; 

    void Start()
    {
        // Give a random starting point for the sine wave to ensure unique wiggling
        horizontalOffset = Random.Range(0f, 10f);
        Destroy(gameObject,5f);
    }

    void Update()
    {
        // 1. Upward Movement (Buoyancy)
        // Move the bubble up along the Y-axis
        transform.Translate(Vector2.up * upwardSpeed * Time.deltaTime);

        // 2. Horizontal Drift (Wiggle)
        // Use a Sine wave for smooth back-and-forth motion
        float horizontalMovement = Mathf.Sin(Time.time * wiggleSpeed + horizontalOffset) * driftMagnitude * Time.deltaTime;
        transform.Translate(Vector2.right * horizontalMovement);

        // 3. Despawn Logic
        if (transform.position.y > despawnHeight)
        {
            Destroy(gameObject);
        }
    }
}