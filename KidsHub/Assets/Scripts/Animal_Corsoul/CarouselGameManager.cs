using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AnimalGridGameManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI topAnimalNameText;
    public RectTransform gridParent;
    public GameObject animalCardPrefab;
    public TextMeshProUGUI resultText;

    [Header("Data")]
    public List<AnimalDataCorsoul> allAnimals; 
    private const int totalOptions = 4;

    private string correctAnimalName;
    private string lastAnimalName;
    private AudioClip correctAnimalClip;

    [Header("Layout Settings")]
    public Vector2 cardSize = new Vector2(250f, 250f);
    public float spacing = 60f;

    [Header("Shake Settings")]
    public float shakeDuration = 0.6f;
    public float shakeMagnitude = 12f;
    public float shakeFrequency = 0.02f;
    public float shakeScaleAmount = 0.06f;

    [Header("Flip Settings")] 
    public float flipDuration = 0.5f;

    [Header("Scale Up Settings")]
    [Tooltip("Duration for the correct card to scale up to cover the grid.")]
    public float scaleUpDuration = 0.3f;
    public float scaleSize = 20f;

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.6f;
    public AudioClip wrongAnswerClip;
    public AudioClip successAnswerClip; 

    [Header("Loading Screen Settings")]
    [Tooltip("Assign your loading screen prefab here.")]
    public GameObject loadingScreenPrefab;
    [Tooltip("How long the loading screen should stay visible (in seconds).")]
    public float loadingScreenDuration = 2f;

    // Runtime
    private GameObject loadingScreenInstance;
    private List<RectTransform> spawnedCardRects = new List<RectTransform>();
    private List<Vector2> originalAnchoredPositions = new List<Vector2>();
    private Coroutine shakeCoroutine = null;
    private RectTransform correctCardRect; 
    private Vector2 gridFullSize; 
    

    void Start()
    {
        // Play background music
        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
            AudioManager.Instance.SetMusicVolume(musicVolume);
        }

        // Calculate the target size for the scale-up (the size of the entire grid area)
        int rows = 2, cols = 2;
        float totalWidth = cols * cardSize.x + (cols - 1) * spacing;
        float totalHeight = rows * cardSize.y + (rows - 1) * spacing;
        gridFullSize = new Vector2(totalWidth+scaleSize, totalHeight+scaleSize);

        // Start game with loading screen
        StartCoroutine(ShowLoadingThenGenerate(true)); 
    }
    
    private IEnumerator ShowLoadingThenGenerate(bool initialLoad = false)
    {
        if (initialLoad)
        {
            yield return StartCoroutine(ShowLoadingScreen());
        }
        
        GenerateRound();
    }

    private IEnumerator ShowLoadingScreen()
    {
        if (loadingScreenPrefab == null)
        {
            Debug.LogWarning("⚠️ No loading screen prefab assigned!");
            yield break;
        }

        if (loadingScreenInstance != null)
            Destroy(loadingScreenInstance);

        loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);
        loadingScreenInstance.SetActive(true);

        yield return new WaitForSeconds(loadingScreenDuration);

        if (loadingScreenInstance != null)
            Destroy(loadingScreenInstance);
    }

    public void GenerateRound()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            ResetCardPositions();
            shakeCoroutine = null;
        }
        if (correctCardRect != null)
        {
            correctCardRect.localEulerAngles = Vector3.zero;
            correctCardRect.sizeDelta = cardSize;
        }
        correctCardRect = null;
        spawnedCardRects.Clear();
        originalAnchoredPositions.Clear();

        // Clear existing cards
        foreach (Transform c in gridParent)
            Destroy(c.gameObject);

        if (allAnimals == null || allAnimals.Count < totalOptions)
        {
            Debug.LogError("Not enough animals in list!");
            return;
        }

        // Pick correct and wrong animals
        var available = new List<AnimalDataCorsoul>(allAnimals);
        if (!string.IsNullOrEmpty(lastAnimalName))
            available.RemoveAll(a => a.name == lastAnimalName);
        if (available.Count == 0) available = new List<AnimalDataCorsoul>(allAnimals);

        var correct = available[Random.Range(0, available.Count)];
        correctAnimalName = correct.name;
        correctAnimalClip = correct.animalSound;
        lastAnimalName = correctAnimalName;
        topAnimalNameText.text = correctAnimalName;
        resultText.text = "";

        var others = allAnimals.Where(a => a.name != correctAnimalName)
                            .OrderBy(x => Random.value)
                            .Take(totalOptions - 1)
                            .ToList();

        var finalList = new List<AnimalDataCorsoul>(others);
        finalList.Insert(Random.Range(0, finalList.Count + 1), correct);

        // Grid setup
        int rows = 2, cols = 2;
        float totalWidth = cols * cardSize.x + (cols - 1) * spacing;
        float totalHeight = rows * cardSize.y + (rows - 1) * spacing;
        Vector2 startPos = new Vector2(-totalWidth / 2 + cardSize.x / 2, totalHeight / 2 - cardSize.y / 2);
        gridParent.anchorMin = gridParent.anchorMax = gridParent.pivot = new Vector2(0.5f, 0.5f);

        // Spawn cards
        for (int i = 0; i < finalList.Count; i++)
        {
            int row = i / cols;
            int col = i % cols;

            Vector2 localPos = new Vector2(
                startPos.x + col * (cardSize.x + spacing),
                startPos.y - row * (cardSize.y + spacing)
            );

            GameObject card = Instantiate(animalCardPrefab, gridParent);
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = cardSize;
            rt.anchoredPosition = localPos;

            if (finalList[i].name == correctAnimalName)
                correctCardRect = rt;

            // Find children
            Transform backChild = card.transform.Find("BackImage");
            Transform animalChild = card.transform.Find("AnimalImage");

            Image backImage = backChild != null ? backChild.GetComponent<Image>() : null;
            Image animalImage = animalChild != null ? animalChild.GetComponent<Image>() : null;

            // Assign sprite if found
            if (animalImage != null)
                animalImage.sprite = finalList[i].image;
            else
                Debug.LogWarning("⚠️ AnimalImage child not found in card prefab!");

            // 🔹 Add click event to the card (not the child)
            Button button = card.GetComponent<Button>();
            if (button == null)
                button = card.AddComponent<Button>();

            string nameCopy = finalList[i].name;
            AudioClip soundCopy = finalList[i].animalSound;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnAnimalSelected(nameCopy, soundCopy));

            // Track for shaking/reset
            spawnedCardRects.Add(rt);
            originalAnchoredPositions.Add(rt.anchoredPosition);
        }
    }


    void OnAnimalSelected(string selectedName, AudioClip sound)
    {
        if (selectedName == correctAnimalName)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                ResetCardPositions();
                shakeCoroutine = null;
            }
            resultText.text = "✅ Correct!";
            StartCoroutine(CorrectAnswerSequence(sound));
        }
        else
        {
            resultText.text = "❌ Try Again!";
            if (AudioManager.Instance != null && wrongAnswerClip != null)
                AudioManager.Instance.PlaySFX(wrongAnswerClip);
            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);
            shakeCoroutine = StartCoroutine(ShakeAllCards(shakeDuration));
        }
    }

    // ✅ UPDATED SEQUENCE VERSION
    private IEnumerator CorrectAnswerSequence(AudioClip animalSound)
    {
        if (correctCardRect != null)
        {
            ToggleButtons(false);

            // 1️⃣ Scale up first (make it big)
            yield return StartCoroutine(ScaleCardUp(correctCardRect, scaleUpDuration, gridFullSize));

            // 2️⃣ Play animal sound & flip at the same time
            if (AudioManager.Instance != null && animalSound != null)
            {
                AudioManager.Instance.PlaySFX(animalSound);
            }
            yield return StartCoroutine(FlipCard(correctCardRect, flipDuration));

            // Wait for the rest of animal sound (if longer than flip)
            if (AudioManager.Instance != null && animalSound != null)
            {
                float remaining = animalSound.length - flipDuration;
                if (remaining > 0f)
                    yield return new WaitForSeconds(remaining);
            }

            // 3️⃣ Immediately play success sound
            if (AudioManager.Instance != null && successAnswerClip != null)
            {
                AudioManager.Instance.PlaySFX(successAnswerClip);
            }

            // Small pause (optional)
            yield return new WaitForSeconds(0.3f);

            // 4️⃣ Show loading screen
            if (loadingScreenPrefab != null)
            {
                if (loadingScreenInstance != null)
                    Destroy(loadingScreenInstance);

                loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);
                loadingScreenInstance.SetActive(true);

                yield return new WaitForSeconds(loadingScreenDuration);

                if (loadingScreenInstance != null)
                    Destroy(loadingScreenInstance);
            }
            else
            {
                Debug.LogWarning("⚠️ No loading screen prefab assigned! Skipping loading screen.");
            }

            ToggleButtons(true);
        }

        // 5️⃣ Generate the next round
        GenerateRound();
    }

    private IEnumerator ScaleCardUp(RectTransform card, float duration, Vector2 targetSize)
    {
        float elapsed = 0f;
        Vector2 startSize = card.sizeDelta;
        Vector3 startPos = card.anchoredPosition;
        Vector3 targetPos = Vector3.zero;

        card.SetAsLastSibling();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            card.sizeDelta = Vector2.Lerp(startSize, targetSize, t);
            card.anchoredPosition = Vector3.Lerp(startPos, targetPos, t);

            yield return null;
        }

        card.sizeDelta = targetSize;
        card.anchoredPosition = targetPos;
    }

    private IEnumerator FlipCard(RectTransform card, float duration)
    {
        float elapsed = 0f;
        Vector3 originalScale = card.localScale;

        // Find Back and Animal images
        Image backImage = card.transform.Find("BackImage")?.GetComponent<Image>();
        Image animalImage = card.transform.Find("AnimalImage")?.GetComponent<Image>();

        if (backImage == null || animalImage == null)
        {
            Debug.LogWarning("⚠️ Missing BackImage or AnimalImage in prefab!");
            yield break;
        }

        // bool isBackVisible = backImage.enabled; // true at start

        // 1️⃣ Shrink to zero width
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            float scaleX = Mathf.Lerp(1f, 0f, t);
            card.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        // // 2️⃣ Swap visibility (back <-> animal)
        // backImage.enabled = !isBackVisible;
        // animalImage.enabled = isBackVisible;

        // 3️⃣ Expand back to full width
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            float scaleX = Mathf.Lerp(0f, 1f, t);
            card.localScale = new Vector3(scaleX, 1f, 1f);
            yield return null;
        }

        card.localScale = originalScale;
    }



    private void ToggleButtons(bool interactable)
    {
        foreach (Transform card in gridParent)
        {
            Button button = card.GetComponentInChildren<Button>();
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
    }

    private IEnumerator ShakeAllCards(float duration)
    {
        float elapsed = 0f;
        for (int i = 0; i < spawnedCardRects.Count; i++)
            if (spawnedCardRects[i] != null)
                originalAnchoredPositions[i] = spawnedCardRects[i].anchoredPosition;

        while (elapsed < duration)
        {
            elapsed += shakeFrequency;
            float t = elapsed / duration;
            float falloff = 1f - Mathf.Clamp01(t);

            for (int i = 0; i < spawnedCardRects.Count; i++)
            {
                RectTransform rt = spawnedCardRects[i];
                if (rt == null) continue;

                Vector2 basePos = originalAnchoredPositions[i];
                Vector2 offset = Random.insideUnitCircle * (shakeMagnitude * falloff);
                rt.anchoredPosition = basePos + offset;

                float scale = 1f + (Mathf.Sin(Time.time * 25f + i) * 0.5f + 0.5f) * (shakeScaleAmount * falloff);
                rt.localScale = Vector3.one * scale;
            }

            yield return new WaitForSeconds(shakeFrequency);
        }

        ResetCardPositions();
        shakeCoroutine = null;
    }

    private void ResetCardPositions()
    {
        for (int i = 0; i < spawnedCardRects.Count; i++)
        {
            RectTransform rt = spawnedCardRects[i];
            if (rt == null) continue;
            rt.anchoredPosition = originalAnchoredPositions[i];
            rt.localScale = Vector3.one;
        }
    }
}
