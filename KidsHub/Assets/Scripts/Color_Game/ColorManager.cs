//using UnityEngine;

//public class ColorManager : MonoBehaviour
//{
//    [Header("Color Info")]
//    public string colorName;
//    public AudioClip colorSound;

//    private AudioSource audioSource;
//    private bool hasBeenTapped = false;

//    [Header("Movement Settings")]
//    public float startSpeed = 8f;      // Fast at start
//    public float slowDownRate = 2f;    // How quickly it slows
//    public float minSpeed = 1f;        // Slowest speed
//    private float currentSpeed;

//    [Header("Scale Animation")]
//    public float startScale = 0.3f;    // Small at start
//    public float targetScale = 1f;     // Full size
//    public float scaleSpeed = 2f;      // How fast it grows
//    private bool scalingDone = false;

//    [Header("Auto Destroy")]
//    public float autoDestroyTime = 4f; // Time before auto destroy if not clicked

//    private SpriteRenderer sr;
//    private float lifeTimer = 0f;

//    private void Start()
//    {
//        // Setup audio
//        audioSource = gameObject.AddComponent<AudioSource>();
//        audioSource.playOnAwake = false;

//        // Start movement
//        currentSpeed = startSpeed;

//        // Start small
//        transform.localScale = Vector3.one * startScale;

//        // Get SpriteRenderer
//        sr = GetComponent<SpriteRenderer>();

//        // Ensure visible
//        if (sr != null)
//        {
//            sr.sortingOrder = 5;
//        }
//    }

//    private void Update()
//    {
//        HandleMovement();
//        HandleScaling();
//        HandleAutoDestroy();
//    }

//    private void HandleMovement()
//    {
//        transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);

//        if (currentSpeed > minSpeed)
//            currentSpeed -= slowDownRate * Time.deltaTime;
//    }

//    private void HandleScaling()
//    {
//        if (scalingDone) return;

//        transform.localScale = Vector3.Lerp(
//            transform.localScale,
//            Vector3.one * targetScale,
//            Time.deltaTime * scaleSpeed
//        );

//        if (Vector3.Distance(transform.localScale, Vector3.one * targetScale) < 0.01f)
//        {
//            transform.localScale = Vector3.one * targetScale;
//            scalingDone = true;
//        }
//    }

//    private void HandleAutoDestroy()
//    {
//        if (hasBeenTapped) return;

//        lifeTimer += Time.deltaTime;
//        if (lifeTimer >= autoDestroyTime)
//        {
//            Destroy(gameObject);
//        }
//    }

//    private void OnMouseDown()
//    {
//        if (hasBeenTapped) return;
//        hasBeenTapped = true;

//        if (colorSound != null)
//            audioSource.PlayOneShot(colorSound);

//        float delay = (colorSound != null) ? colorSound.length : 0f;
//        Destroy(gameObject, delay);
//    }

//    public void SetColorInfo(string name, Color color, AudioClip clip)
//    {
//        colorName = name;
//        colorSound = clip;

//        if (sr == null)
//            sr = GetComponent<SpriteRenderer>();

//        if (sr != null)
//        {
//            color.a = 1f; // Ensure visible
//            sr.color = color;
//            sr.sortingOrder = 5;
//        }
//    }
//}





using UnityEngine;

public class ColorManager : MonoBehaviour
{
    [Header("Color Info")]
    public string colorName;
    public AudioClip colorSound;

    private AudioSource audioSource;
    private bool hasBeenTapped = false;

    [Header("Movement Settings")]
    public float startSpeed = 8f;
    public float slowDownRate = 2f;
    public float minSpeed = 1f;
    private float currentSpeed;

    [Header("Scale Animation")]
    public float startScale = 0.3f;
    public float targetScale = 1f;
    public float scaleSpeed = 2f;
    private bool scalingDone = false;

    [Header("Auto Destroy")]
    public float autoDestroyTime = 4f;

    private SpriteRenderer sr;
    private float lifeTimer = 0f;

    public System.Action<string> OnColorDestroyed; // notify spawner when destroyed

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        currentSpeed = startSpeed;
        transform.localScale = Vector3.one * startScale;
        sr = GetComponent<SpriteRenderer>();

        if (sr != null)
            sr.sortingOrder = 5;
    }

    private void Update()
    {
        HandleMovement();
        HandleScaling();
        HandleAutoDestroy();
    }

    private void HandleMovement()
    {
        transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);
        if (currentSpeed > minSpeed)
            currentSpeed -= slowDownRate * Time.deltaTime;
    }

    private void HandleScaling()
    {
        if (scalingDone) return;

        transform.localScale = Vector3.Lerp(
            transform.localScale,
            Vector3.one * targetScale,
            Time.deltaTime * scaleSpeed
        );

        if (Vector3.Distance(transform.localScale, Vector3.one * targetScale) < 0.01f)
        {
            transform.localScale = Vector3.one * targetScale;
            scalingDone = true;
        }
    }

    private void HandleAutoDestroy()
    {
        if (hasBeenTapped) return;

        lifeTimer += Time.deltaTime;
        if (lifeTimer >= autoDestroyTime)
        {
            OnColorDestroyed?.Invoke(colorName);
            Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        if (hasBeenTapped) return;
        hasBeenTapped = true;

        if (colorSound != null)
            audioSource.PlayOneShot(colorSound);

        float delay = (colorSound != null) ? colorSound.length : 0f;
        OnColorDestroyed?.Invoke(colorName);
        Destroy(gameObject, delay);
    }

    public void SetColorInfo(string name, Color color, AudioClip clip)
    {
        colorName = name;
        colorSound = clip;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr != null)
        {
            color.a = 1f;
            sr.color = color;
            sr.sortingOrder = 5;
        }
    }
}
