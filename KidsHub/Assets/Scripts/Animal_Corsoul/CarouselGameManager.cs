//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;

//public class AnimalGridGameManager : MonoBehaviour
//{
//    [Header("UI References")]
//    public TextMeshProUGUI topAnimalNameText;
//    public RectTransform gridParent;
//    public GameObject animalCardPrefab;
//    public TextMeshProUGUI resultText;

//    [Header("Data")]
//    public List<AnimalDataCorsoul> allAnimals;
//    private const int totalOptions = 4;

//    private string correctAnimalName;
//    private string lastAnimalName;
//    private AudioClip correctAnimalClip; // ✅ current correct animal sound

//    [Header("Layout Settings")]
//    public Vector2 cardSize = new Vector2(250f, 250f);
//    public float spacing = 60f;

//    [Header("Shake Settings")]
//    public float shakeDuration = 0.6f;
//    public float shakeMagnitude = 12f;
//    public float shakeFrequency = 0.02f;
//    public float shakeScaleAmount = 0.06f;

//    [Header("Audio Settings")]
//    public AudioClip backgroundMusic;
//    [Range(0f, 1f)]
//    public float musicVolume = 0.6f;
//    public AudioClip wrongAnswerClip;

//    // Runtime
//    private List<RectTransform> spawnedCardRects = new List<RectTransform>();
//    private List<Vector2> originalAnchoredPositions = new List<Vector2>();
//    private Coroutine shakeCoroutine = null;

//    void Start()
//    {
//        // ✅ play background music
//        if (AudioManager.Instance != null && backgroundMusic != null)
//        {
//            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
//            AudioManager.Instance.SetMusicVolume(musicVolume);
//        }

//        GenerateRound();
//    }

//    public void GenerateRound()
//    {
//        // stop any shake
//        if (shakeCoroutine != null)
//        {
//            StopCoroutine(shakeCoroutine);
//            shakeCoroutine = null;
//        }

//        // clear old cards
//        spawnedCardRects.Clear();
//        originalAnchoredPositions.Clear();
//        foreach (Transform c in gridParent)
//            Destroy(c.gameObject);

//        if (allAnimals == null || allAnimals.Count < totalOptions)
//        {
//            Debug.LogError("Not enough animals in list!");
//            return;
//        }

//        // pick correct + others
//        var available = new List<AnimalDataCorsoul>(allAnimals);
//        if (!string.IsNullOrEmpty(lastAnimalName))
//            available.RemoveAll(a => a.name == lastAnimalName);
//        if (available.Count == 0) available = new List<AnimalDataCorsoul>(allAnimals);

//        var correct = available[Random.Range(0, available.Count)];
//        correctAnimalName = correct.name;
//        correctAnimalClip = correct.animalSound;
//        lastAnimalName = correctAnimalName;
//        topAnimalNameText.text = correctAnimalName;
//        resultText.text = "";

//        var others = allAnimals.Where(a => a.name != correctAnimalName)
//                               .OrderBy(x => Random.value)
//                               .Take(totalOptions - 1)
//                               .ToList();

//        var finalList = new List<AnimalDataCorsoul>(others);
//        finalList.Insert(Random.Range(0, finalList.Count + 1), correct);

//        // layout
//        int rows = 2, cols = 2;
//        float totalWidth = cols * cardSize.x + (cols - 1) * spacing;
//        float totalHeight = rows * cardSize.y + (rows - 1) * spacing;
//        Vector2 startPos = new Vector2(-totalWidth / 2 + cardSize.x / 2, totalHeight / 2 - cardSize.y / 2);
//        gridParent.anchorMin = gridParent.anchorMax = gridParent.pivot = new Vector2(0.5f, 0.5f);

//        // spawn 4 cards
//        for (int i = 0; i < finalList.Count; i++)
//        {
//            int row = i / cols;
//            int col = i % cols;

//            Vector2 localPos = new Vector2(
//                startPos.x + col * (cardSize.x + spacing),
//                startPos.y - row * (cardSize.y + spacing)
//            );

//            GameObject card = Instantiate(animalCardPrefab, gridParent);
//            RectTransform rt = card.GetComponent<RectTransform>();
//            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
//            rt.sizeDelta = cardSize;
//            rt.anchoredPosition = localPos;

//            // ✅ find the CHILD image (not parent)
//            Image childImage = null;
//            foreach (Transform child in card.transform)
//            {
//                if (child.GetComponent<Image>() != null)
//                {
//                    childImage = child.GetComponent<Image>();
//                    break;
//                }
//            }

//            if (childImage != null)
//            {
//                childImage.sprite = finalList[i].image;

//                // ensure parent not clickable
//                Button parentBtn = card.GetComponent<Button>();
//                if (parentBtn != null)
//                    Destroy(parentBtn);

//                // ensure button on CHILD
//                Button imageButton = childImage.GetComponent<Button>();
//                if (imageButton == null)
//                    imageButton = childImage.gameObject.AddComponent<Button>();

//                string nameCopy = finalList[i].name;
//                AudioClip soundCopy = finalList[i].animalSound;

//                imageButton.onClick.RemoveAllListeners();
//                imageButton.onClick.AddListener(() => OnAnimalSelected(nameCopy, soundCopy));
//            }
//            else
//            {
//                Debug.LogWarning("⚠️ No child Image found for prefab!");
//            }

//            // track for shaking
//            spawnedCardRects.Add(rt);
//            originalAnchoredPositions.Add(rt.anchoredPosition);
//        }
//    }

//    void OnAnimalSelected(string selectedName, AudioClip sound)
//    {
//        if (selectedName == correctAnimalName)
//        {
//            // stop shake
//            if (shakeCoroutine != null)
//            {
//                StopCoroutine(shakeCoroutine);
//                ResetCardPositions();
//                shakeCoroutine = null;
//            }

//            resultText.text = "✅ Correct!";

//            // ✅ play correct animal sound
//            if (AudioManager.Instance != null && sound != null)
//                AudioManager.Instance.PlaySFX(sound);

//            Invoke(nameof(GenerateRound), 1.8f);
//        }
//        else
//        {
//            resultText.text = "❌ Try Again!";

//            // ✅ wrong answer clip
//            if (AudioManager.Instance != null && wrongAnswerClip != null)
//                AudioManager.Instance.PlaySFX(wrongAnswerClip);

//            if (shakeCoroutine != null)
//                StopCoroutine(shakeCoroutine);
//            shakeCoroutine = StartCoroutine(ShakeAllCards(shakeDuration));
//        }
//    }

//    private IEnumerator ShakeAllCards(float duration)
//    {
//        float elapsed = 0f;
//        for (int i = 0; i < spawnedCardRects.Count; i++)
//            if (spawnedCardRects[i] != null)
//                originalAnchoredPositions[i] = spawnedCardRects[i].anchoredPosition;

//        while (elapsed < duration)
//        {
//            elapsed += shakeFrequency;
//            float t = elapsed / duration;
//            float falloff = 1f - Mathf.Clamp01(t);

//            for (int i = 0; i < spawnedCardRects.Count; i++)
//            {
//                RectTransform rt = spawnedCardRects[i];
//                if (rt == null) continue;

//                Vector2 basePos = originalAnchoredPositions[i];
//                Vector2 offset = Random.insideUnitCircle * (shakeMagnitude * falloff);
//                rt.anchoredPosition = basePos + offset;

//                float scale = 1f + (Mathf.Sin(Time.time * 25f + i) * 0.5f + 0.5f) * (shakeScaleAmount * falloff);
//                rt.localScale = Vector3.one * scale;
//            }

//            yield return new WaitForSeconds(shakeFrequency);
//        }

//        ResetCardPositions();
//        shakeCoroutine = null;
//    }

//    private void ResetCardPositions()
//    {
//        for (int i = 0; i < spawnedCardRects.Count; i++)
//        {
//            RectTransform rt = spawnedCardRects[i];
//            if (rt == null) continue;

//            rt.anchoredPosition = originalAnchoredPositions[i];
//            rt.localScale = Vector3.one;
//        }
//    }
//}















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

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)]
    public float musicVolume = 0.6f;
    public AudioClip wrongAnswerClip;

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

    void Start()
    {
        // ✅ Play background music
        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
            AudioManager.Instance.SetMusicVolume(musicVolume);
        }

        // ✅ Start game with loading screen
        StartCoroutine(ShowLoadingThenGenerate());
    }

    private IEnumerator ShowLoadingThenGenerate()
    {
        yield return StartCoroutine(ShowLoadingScreen());
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
        // Stop shake
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        // Clear old cards
        spawnedCardRects.Clear();
        originalAnchoredPositions.Clear();
        foreach (Transform c in gridParent)
            Destroy(c.gameObject);

        if (allAnimals == null || allAnimals.Count < totalOptions)
        {
            Debug.LogError("Not enough animals in list!");
            return;
        }

        // Pick correct + others
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

        // Layout setup
        int rows = 2, cols = 2;
        float totalWidth = cols * cardSize.x + (cols - 1) * spacing;
        float totalHeight = rows * cardSize.y + (rows - 1) * spacing;
        Vector2 startPos = new Vector2(-totalWidth / 2 + cardSize.x / 2, totalHeight / 2 - cardSize.y / 2);
        gridParent.anchorMin = gridParent.anchorMax = gridParent.pivot = new Vector2(0.5f, 0.5f);

        // Spawn 4 cards
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

            // ✅ find child image
            Image childImage = null;
            foreach (Transform child in card.transform)
            {
                if (child.GetComponent<Image>() != null)
                {
                    childImage = child.GetComponent<Image>();
                    break;
                }
            }

            if (childImage != null)
            {
                childImage.sprite = finalList[i].image;

                // Remove parent button
                Button parentBtn = card.GetComponent<Button>();
                if (parentBtn != null) Destroy(parentBtn);

                // Add button to child
                Button imageButton = childImage.GetComponent<Button>();
                if (imageButton == null)
                    imageButton = childImage.gameObject.AddComponent<Button>();

                string nameCopy = finalList[i].name;
                AudioClip soundCopy = finalList[i].animalSound;
                imageButton.onClick.RemoveAllListeners();
                imageButton.onClick.AddListener(() => OnAnimalSelected(nameCopy, soundCopy));
            }
            else
            {
                Debug.LogWarning("⚠️ No child Image found for prefab!");
            }

            spawnedCardRects.Add(rt);
            originalAnchoredPositions.Add(rt.anchoredPosition);
        }
    }

    void OnAnimalSelected(string selectedName, AudioClip sound)
    {
        if (selectedName == correctAnimalName)
        {
            // Stop shake
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                ResetCardPositions();
                shakeCoroutine = null;
            }

            resultText.text = "✅ Correct!";

            // ✅ play correct animal sound
            if (AudioManager.Instance != null && sound != null)
                AudioManager.Instance.PlaySFX(sound);

            // ✅ Show loading screen then next round
            StartCoroutine(ShowLoadingThenGenerate());
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




