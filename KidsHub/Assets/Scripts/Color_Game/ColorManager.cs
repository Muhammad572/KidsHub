// using UnityEngine;
// using DG.Tweening;

// public class ColorManager : MonoBehaviour
// {
//     [Header("Color Info")]
//     public string colorName;
//     public AudioClip colorSound; // main bubble pop
//     public AudioClip sparkleChime; // secondary chime layer

//     private bool hasBeenTapped = false;

//     [Header("Movement Settings")]
//     public float startSpeed = 8f;
//     public float slowDownRate = 2f;
//     public float minSpeed = 1f;
//     private float currentSpeed;

//     [Header("Scale Animation")]
//     public float startScale = 0.3f;
//     public float targetScale = 1f;
//     public float scaleSpeed = 2f;
//     private bool scalingDone = false;

//     [Header("Auto Destroy")]
//     public float autoDestroyTime = 4f;

//     [Header("SFX Settings")]
//     [Range(0f, 1f)] public float sfxVolume = 1f;

//     private SpriteRenderer sr;
//     private float lifeTimer = 0f;

//     public System.Action<string> OnColorDestroyed;

//     public GameObject sparkleFXPrefab;
//     public GameObject confettiFXPrefab;
//     public GameObject colorTrailPrefab;

//     private void Start()
//     {
//         currentSpeed = startSpeed;
//         transform.localScale = Vector3.one * startScale;
//         sr = GetComponent<SpriteRenderer>();
//         if (sr != null) sr.sortingOrder = 5;

//         // spawn animation
//         transform.localScale = Vector3.zero;
//         transform.DOScale(targetScale, 0.6f).SetEase(Ease.OutElastic);
//         transform.DORotate(new Vector3(0, 0, Random.Range(-10f, 10f)), 0.5f)
//                  .SetEase(Ease.OutSine);

//         // spawn sparkles and trail
//         SpawnSparkles();
//         SpawnTrail();
//     }

//     private void SpawnTrail()
//     {
//         GameObject fx = colorTrailPrefab != null ? colorTrailPrefab : Resources.Load<GameObject>("ColorTrailFX");
//         if (fx != null)
//         {
//             GameObject trail = Instantiate(fx, transform);
//             var ps = trail.GetComponent<ParticleSystem>();
//             if (ps != null)
//             {
//                 var main = ps.main;
//                 if (sr != null) main.startColor = sr.color * 1.2f;
//             }
//         }
//     }

//     private void SpawnSparkles()
//     {
//         GameObject fx = sparkleFXPrefab != null ? sparkleFXPrefab : Resources.Load<GameObject>("SparkleFX");
//         if (fx != null)
//         {
//             GameObject spawned = Instantiate(fx, transform.position, Quaternion.identity);
//             spawned.transform.localScale = Vector3.one * 0.7f;
//             Destroy(spawned, 1.5f);
//         }
//     }

//     private void Update()
//     {
//         HandleMovement();
//         HandleScaling();
//         HandleAutoDestroy();
//     }

//     private void HandleMovement()
//     {
//         transform.Translate(Vector2.down * currentSpeed * Time.deltaTime);
//         if (currentSpeed > minSpeed)
//             currentSpeed -= slowDownRate * Time.deltaTime;
//     }

//     private void HandleScaling()
//     {
//         if (scalingDone) return;
//         transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.deltaTime * scaleSpeed);
//         if (Vector3.Distance(transform.localScale, Vector3.one * targetScale) < 0.01f)
//         {
//             transform.localScale = Vector3.one * targetScale;
//             scalingDone = true;
//         }
//     }

//     private void HandleAutoDestroy()
//     {
//         if (hasBeenTapped) return;

//         lifeTimer += Time.deltaTime;
//         if (lifeTimer >= autoDestroyTime)
//         {
//             OnColorDestroyed?.Invoke(colorName);
//             Destroy(gameObject);
//         }
//     }

//     private void OnMouseDown()
//     {
//         if (hasBeenTapped) return;
//         hasBeenTapped = true;

//         // 🎵 Play bubble pop + sparkle chime combo
//         if (AudioManager.Instance != null)
//             AudioManager.Instance.PlayLayeredSFX(colorSound, sparkleChime, sfxVolume);

//         // 🎉 bounce + shrink + effects
//         transform.DOScale(targetScale * 1.3f, 0.2f).SetEase(Ease.OutBack)
//             .OnComplete(() => transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));

//         SpawnConfetti();
//         OnColorDestroyed?.Invoke(colorName);

//         Destroy(gameObject, 0.5f);
//     }

//     private void SpawnConfetti()
//     {
//         GameObject fx = confettiFXPrefab != null ? confettiFXPrefab : Resources.Load<GameObject>("ConfettiFX");
//         if (fx != null)
//         {
//             GameObject spawned = Instantiate(fx, transform.position, Quaternion.identity);
//             spawned.transform.localScale = Vector3.one * 1.2f;
//             Destroy(spawned, 2f);
//         }

//         GameObject pulse = Resources.Load<GameObject>("TapPulseFX");
//         if (pulse)
//         {
//             GameObject p = Instantiate(pulse, transform.position, Quaternion.identity);
//             Destroy(p, 0.4f);
//         }
//     }

//     public void SetColorInfo(string name, Color color, AudioClip clip)
//     {
//         colorName = name;
//         colorSound = clip;

//         if (sr == null)
//             sr = GetComponent<SpriteRenderer>();

//         if (sr != null)
//         {
//             color.a = 1f;
//             sr.color = color;
//             sr.sortingOrder = 5;
//         }
//     }
// }


using UnityEngine;
using DG.Tweening;

public class ColorManager : MonoBehaviour
{
    [Header("Color Info")]
    public string colorName;
    public AudioClip colorSound; // main bubble pop

    [Header("Clips")]
    public AudioClip sparkleChime; // secondary chime layer
    
    public AudioClip BubblePop;
    public AudioClip BubbleSpawn;

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

    [Header("SFX Settings")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private SpriteRenderer sr;
    private float lifeTimer = 0f;

    public System.Action<string> OnColorDestroyed;

    public GameObject sparkleFXPrefab;
    public GameObject confettiFXPrefab;
    public GameObject colorTrailPrefab;

    private void Start()
    {
        currentSpeed = startSpeed;
        transform.localScale = Vector3.one * startScale;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) sr.sortingOrder = 5;

        // // spawn animation
        // transform.localScale = Vector3.zero;
        // transform.DOScale(targetScale, 0.6f).SetEase(Ease.OutElastic);
        // transform.DORotate(new Vector3(0, 0, Random.Range(-10f, 10f)), 0.5f)
        //          .SetEase(Ease.OutSine);

        // spawn sparkles and trail
        SpawnSparkles();
        SpawnTrail();
        if (AudioManager.Instance != null)
        {
            AudioClip spawnClip = BubbleSpawn;//Resources.Load<AudioClip>("BubbleSpawn"); // optional
            if (spawnClip != null)
                AudioManager.Instance.PlaySFX(spawnClip, 0.3f);
        }
    }

    private void SpawnTrail()
    {
        GameObject fx = colorTrailPrefab != null ? colorTrailPrefab : Resources.Load<GameObject>("ColorTrailFX");
        if (fx != null)
        {
            GameObject trail = Instantiate(fx, transform);
            var ps = trail.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                if (sr != null) main.startColor = sr.color * 1.2f;
            }
        }
    }

    private void SpawnSparkles()
    {
        GameObject fx = sparkleFXPrefab != null ? sparkleFXPrefab : Resources.Load<GameObject>("SparkleFX");
        if (fx != null)
        {
            GameObject spawned = Instantiate(fx, transform.position, Quaternion.identity);
            spawned.transform.localScale = Vector3.one * 0.7f;
            Destroy(spawned, 1.5f);
        }
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
        transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * targetScale, Time.deltaTime * scaleSpeed);
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
            if (AudioManager.Instance != null)
            {
                AudioClip vanishClip = BubblePop;//Resources.Load<AudioClip>("BubbleVanish"); // optional
                if (vanishClip != null)
                    AudioManager.Instance.PlaySFX(vanishClip, 0.4f);
            }
            Destroy(gameObject);
        }
    }

    private void OnMouseDown()
    {
        if (hasBeenTapped) return;
        hasBeenTapped = true;

        // 🎵 Play bubble pop + sparkle chime combo
        // if (AudioManager.Instance != null)
        //     AudioManager.Instance.PlayLayeredSFX(colorSound, sparkleChime, sfxVolume);

        if (AudioManager.Instance != null)
        {
            // 1️⃣ Play the universal pop first
            AudioClip popClip = BubblePop;//Resources.Load<AudioClip>("BubblePop");
            if (popClip != null)
                AudioManager.Instance.PlaySFX(popClip, 0.8f);

            // 2️⃣ After short delay, play the color’s voice clip
            if (colorSound != null)
                StartCoroutine(PlayColorSoundDelayed(0.25f));
        }


        // 🎉 bounce + shrink + effects
        transform.DOScale(targetScale * 1.3f, 0.2f).SetEase(Ease.OutBack)
            .OnComplete(() => transform.DOScale(0f, 0.3f).SetEase(Ease.InBack));

        SpawnConfetti();
        OnColorDestroyed?.Invoke(colorName);

        Destroy(gameObject, 0.5f);
    }
    
    private System.Collections.IEnumerator PlayColorSoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.Instance?.PlaySFX(colorSound, sfxVolume);
    }

    private void SpawnConfetti()
    {
        GameObject fx = confettiFXPrefab != null ? confettiFXPrefab : Resources.Load<GameObject>("ConfettiFX");
        if (fx != null)
        {
            GameObject spawned = Instantiate(fx, transform.position, Quaternion.identity);
            spawned.transform.localScale = Vector3.one * 1.2f;
            Destroy(spawned, 2f);
        }

        GameObject pulse = Resources.Load<GameObject>("TapPulseFX");
        if (pulse)
        {
            GameObject p = Instantiate(pulse, transform.position, Quaternion.identity);
            Destroy(p, 0.4f);
        }
    }

    public void SetColorInfo(string name, Color color, AudioClip clip, AudioClip chime = null)
    {
        colorName = name;
        colorSound = clip;
        sparkleChime = chime;

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
