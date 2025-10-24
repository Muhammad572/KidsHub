using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

[System.Serializable]
public class AnimalData
{
    public string animalName;       // e.g. "ELEPHANT"
    public Sprite animalSprite;     // Image shown
    public AudioClip animalSound;   // Sound played when word completes
}

public class AnimalManager : MonoBehaviour
{
    [Header("Animal Data")]
    public List<AnimalData> animals;
    public Image animalImage;

    [Header("Letter Sprites (A–Z)")]
    [Tooltip("Assign 26 sprites in correct order (A–Z).")]
    public Sprite[] letterSprites;

    [Header("Managers")]
    public LetterScatterManager letterScatterManager;
    public WordManager wordManager;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float animalSoundVolume = 0.9f;

    private int currentAnimalIndex = -1;
    private int lastAnimalIndex = -1;
    private Dictionary<char, Sprite> letterSpriteDict = new Dictionary<char, Sprite>();

    void Awake()
    {
        LoadLetterSprites();
    }
    void Start()
    {
        if (animalImage == null) Debug.LogError("❌ Animal Image not assigned!");
        if (wordManager == null) Debug.LogError("❌ WordManager not assigned!");
        if (letterScatterManager == null) Debug.LogError("❌ LetterScatterManager not assigned!");

        // Subscribe to word complete event
        wordManager.OnWordCompleted += HandleWordCompleted;

        // 🕹️ Show loading at game start
        LoadingManager.Instance?.ShowLoading(2f);

        // Load the first animal after loading panel
        StartCoroutine(LoadFirstAnimalAfterDelay(2f));
    }

    private IEnumerator LoadFirstAnimalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadNextAnimal();
    }


    void LoadLetterSprites()
    {
        letterSpriteDict.Clear();

        if (letterSprites == null || letterSprites.Length < 26)
        {
            Debug.LogError("❌ Please assign all 26 letter sprites (A–Z) in the Inspector.");
            return;
        }

        for (int i = 0; i < 26; i++)
        {
            char c = (char)('A' + i);
            letterSpriteDict[c] = letterSprites[i];
        }

        Debug.Log($"✅ Loaded {letterSpriteDict.Count} letter sprites from Inspector.");
    }

    public void LoadAnimal(int index)
    {
        if (animals == null || animals.Count == 0)
        {
            Debug.LogError("❌ No animals assigned!");
            return;
        }

        if (index < 0 || index >= animals.Count)
        {
            Debug.LogError($"❌ Invalid animal index: {index}");
            return;
        }

        currentAnimalIndex = index;
        lastAnimalIndex = index;

        AnimalData animal = animals[index];

        if (animal == null)
        {
            Debug.LogError($"❌ Animal at index {index} is null!");
            return;
        }

        if (animal.animalSprite == null)
        {
            Debug.LogError($"❌ Missing sprite for animal: {animal.animalName}");
            return;
        }

        // ✅ Set animal image
        animalImage.sprite = animal.animalSprite;
        animalImage.transform.localScale = Vector3.zero;
        animalImage.DOFade(0, 0f); // if using UI Image

        animalImage.DOFade(1f, 0.6f);
        animalImage.transform.DOScale(1f, 0.6f).SetEase(Ease.OutBack);

        // ✅ Create word slots and letters
        string word = animal.animalName.Trim().ToUpper();
        wordManager.CreateSlots(word);
        letterScatterManager.SetupLetters(word, letterSpriteDict);

        Debug.Log($"✅ Loaded animal: {word}");
    }

    public void LoadNextAnimal()
    {
        if (animals == null || animals.Count == 0)
        {
            Debug.LogError("❌ No animals available!");
            return;
        }

        // 🕹️ Show loading panel before next animal
        LoadingManager.Instance?.ShowLoading(2f);

        StartCoroutine(LoadAnimalAfterDelay(2f));
    }

    private IEnumerator LoadAnimalAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        int nextIndex = Random.Range(0, animals.Count);
        if (animals.Count > 1)
        {
            while (nextIndex == lastAnimalIndex)
                nextIndex = Random.Range(0, animals.Count);
        }

        LoadAnimal(nextIndex);
    }

    private void HandleWordCompleted()
{
    Debug.Log("🎉 Word completed!");

    if (currentAnimalIndex >= 0 && currentAnimalIndex < animals.Count)
    {
        AnimalData currentAnimal = animals[currentAnimalIndex];
        if (currentAnimal.animalSound != null)
        {
            Debug.Log($"🔊 Playing animal sound for: {currentAnimal.animalName}");
            AudioManager.Instance?.PlaySFXSeparate(currentAnimal.animalSound, animalSoundVolume);

            // 💥 Animate the animal image
            animalImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.5f, 3, 0.5f);
            animalImage.DOFade(1f, 0.5f).From(0.7f);

            // ✅ After animal sound, play the complete sound
            StartCoroutine(PlayCompleteSoundAfter(currentAnimal.animalSound.length));
        }
        else
        {
            Debug.LogWarning($"⚠️ No sound assigned for: {currentAnimal.animalName}");
            // If no sound, just play complete sound immediately
            StartCoroutine(PlayCompleteSoundAfter(0f));
        }
    }
}

private IEnumerator PlayCompleteSoundAfter(float delay)
{
    yield return new WaitForSeconds(delay + 0.1f);

    // ✅ Now play the WordManager’s complete sound
    WordManager.instance?.SendMessage("PlayCompleteSound", SendMessageOptions.DontRequireReceiver);

    // ⏳ Load next animal after both sounds
    Invoke(nameof(LoadNextAnimal), 2f);
}





    // private void HandleWordCompleted()
    // {
    //     Debug.Log("🎉 Word completed!");

    //     // ✅ Play animal sound after completion
    //     if (currentAnimalIndex >= 0 && currentAnimalIndex < animals.Count)
    //     {
    //         AnimalData currentAnimal = animals[currentAnimalIndex];
    //         if (currentAnimal.animalSound != null)
    //         {
    //             Debug.Log($"🔊 Playing sound for: {currentAnimal.animalName}");
    //             AudioManager.Instance?.PlaySFXSeparate(currentAnimal.animalSound, animalSoundVolume);
    //             animalImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.5f, 2, 0.5f);
    //             animalImage.transform.DOPunchScale(Vector3.one * 0.15f, 0.6f, 4, 0.5f);
    //             animalImage.DOFade(1f, 0.5f).From(0.7f);

    //         }
    //         else
    //         {
    //             Debug.LogWarning($"⚠️ No sound assigned for: {currentAnimal.animalName}");
    //         }
    //     }

    //     // ⏳ Load next animal after delay
    //     Invoke(nameof(LoadNextAnimal), 2f);
    // }

    private void OnDestroy()
    {
        if (wordManager != null)
            wordManager.OnWordCompleted -= HandleWordCompleted;
    }
}
