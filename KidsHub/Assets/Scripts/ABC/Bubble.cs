using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider2D))]
public class Bubble : MonoBehaviour
{
    [Header("Movement Settings")]
    public float floatSpeed = 0.05f;
    public float driftMagnitude = 0.015f;
    public float driftSpeed = 0.25f;

    [Header("Scaling")]
    public Vector3 maxScale = new Vector3(0.4f, 0.4f, 1f);
    public float scaleGrowSpeed = 1.0f;

    [Header("Interaction")]
    public GameObject popEffectPrefab;
    [Tooltip("The character this bubble represents (A-Z).")]
    public char bubbleLetter = 'A'; // assigned when spawned

    private float noiseOffset;
    private int horizontalDirection = 1;

    void Start()
    {
        transform.localScale = Vector3.zero;
        noiseOffset = Random.Range(0f, 100f);

        Collider2D col = GetComponent<Collider2D>();
        col.isTrigger = true;

        Destroy(gameObject, 12f);
    }

    void Update()
    {
        // Smooth scaling
        transform.localScale = Vector3.Lerp(transform.localScale, maxScale, Time.deltaTime * scaleGrowSpeed);

        // Floating motion
        float upward = floatSpeed * Time.deltaTime;
        float horizontal = Mathf.Sin((Time.time + noiseOffset) * driftSpeed) * driftMagnitude * horizontalDirection;
        transform.Translate(new Vector3(horizontal, upward, 0f), Space.World);

        // --- Touch or Mouse Pop Detection (New Input System) ---
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 pos = Touchscreen.current.primaryTouch.position.ReadValue();
            TryPopAtScreenPosition(pos);
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 pos = Mouse.current.position.ReadValue();
            TryPopAtScreenPosition(pos);
        }
    }

    private void TryPopAtScreenPosition(Vector2 screenPos)
    {
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        Collider2D hit = Physics2D.OverlapPoint(worldPos);

        if (hit != null && hit.gameObject == gameObject)
        {
            bool canPop = false;

            AlphabetMatchManager matchManager = FindFirstObjectByType<AlphabetMatchManager>();
            if (matchManager != null)
                canPop = matchManager.OnBubblePopped(bubbleLetter);

            if (canPop)
                PopBubble(); // ✅ only pop if correct letter
        }
    }

    private void PopBubble()
    {
        if (popEffectPrefab != null)
        {
            GameObject effect = Instantiate(popEffectPrefab, transform.position, Quaternion.identity);
            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
                Destroy(effect, ps.main.duration + ps.main.startLifetimeMultiplier);
            else
                Destroy(effect, 2f);
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Wall"))
        {
            horizontalDirection *= -1;
        }
    }
}
