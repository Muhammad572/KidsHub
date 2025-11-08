// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections;
// using System.Collections.Generic;
// using DG.Tweening;

// [System.Serializable]
// public class LetterData
// {
//     public Sprite sprite;   // Letter image (A–Z)
//     public AudioClip sound; // Letter sound (A–Z)
// }

// public class WordManager : MonoBehaviour
// {
//     public static WordManager instance;
//     public List<Transform> placedLetters = new List<Transform>();

//     [Header("Slot Setup")]
//     public GameObject slotPrefab;
//     public Transform slotsParent;

//     [Header("Letter Data (A–Z in order)")]
//     [Tooltip("Assign 26 letter sprites and sounds (A–Z).")]
//     public List<LetterData> letters = new List<LetterData>(26);

//     [Header("Layout Settings")]
//     public float spacing = 100f;
//     public float slotScale = 1.0f;

//     [Header("Sounds")]
//     public AudioClip wrongLetterSound;
//     [Range(0f, 1f)] public float soundVolume = 0.8f;

//     [Header("✅ Complete Sound Settings")]
//     public AudioClip completeSound;
//     [Range(0f, 1f)] public float completeSoundVolume = 1.0f;

//     [Header("🎵 Background Music Settings")]
//     public AudioClip backgroundMusic;
//     [Range(0f, 1f)] public float backgroundMusicVolume = 0.5f;
//     private AudioSource bgMusicSource;

//     [Header("🎉 FX Settings")]
//     [Tooltip("Optional confetti prefab for word completion.")]
//     public GameObject confettiPrefab;
//     [Tooltip("Parent transform for confetti spawn, usually the same as slotsParent.")]
//     public Transform wordArea;

//     public System.Action OnWordCompleted;

//     private List<LetterSlot> activeSlots = new List<LetterSlot>();
//     private Dictionary<char, LetterData> letterDict = new Dictionary<char, LetterData>();

//     private int nextLetterIndex = 0;
//     private string currentWord = "LION";

//     public string CurrentWord => currentWord;

//     private int currentFilledIndex = 0;
//     public int CurrentFilledIndex => currentFilledIndex;

//     private void Awake()
//     {
//         instance = this;
//         BuildLetterDictionary();
//         SetupBackgroundMusic();
//     }

//     // 🎶 Setup and play background music
//     private void SetupBackgroundMusic()
//     {
//         if (backgroundMusic == null) return;

//         if (AudioManager.Instance != null)
//             AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
//         else
//         {
//             bgMusicSource = gameObject.AddComponent<AudioSource>();
//             bgMusicSource.clip = backgroundMusic;
//             bgMusicSource.loop = true;
//             bgMusicSource.volume = backgroundMusicVolume;
//             bgMusicSource.playOnAwake = false;
//             bgMusicSource.Play();
//         }
//     }

//     // 🧩 Build A–Z sprite/sound lookup
//     private void BuildLetterDictionary()
//     {
//         letterDict.Clear();

//         for (int i = 0; i < letters.Count; i++)
//         {
//             char c = (char)('A' + i);
//             letterDict[c] = letters[i];
//         }

//         if (letters.Count < 26)
//             Debug.LogWarning($"⚠️ Only {letters.Count}/26 letters assigned!");
//         else
//             Debug.Log("✅ Loaded all 26 letter sprites and sounds!");
//     }

//     // 🧱 Create slots for the current word
//     public void CreateSlots(string word)
//     {
//         if (slotPrefab == null || slotsParent == null)
//         {
//             Debug.LogError("❌ Slot prefab or parent missing!");
//             return;
//         }

//         foreach (Transform child in slotsParent)
//             Destroy(child.gameObject);

//         activeSlots.Clear();

//         currentWord = word.ToUpper();
//         currentFilledIndex = 0;
//         nextLetterIndex = 0;

//         float totalWidth = (word.Length - 1) * spacing;
//         float startX = -totalWidth / 2f;

//         for (int i = 0; i < word.Length; i++)
//         {
//             char c = currentWord[i];
//             GameObject slotObj = Instantiate(slotPrefab, slotsParent);
//             slotObj.name = $"Slot_{c}_{i}";
//             slotObj.transform.localScale = Vector3.one * slotScale;

//             RectTransform rt = slotObj.GetComponent<RectTransform>();
//             if (rt != null)
//                 rt.anchoredPosition = new Vector2(startX + i * spacing, 0);

//             if (letterDict.TryGetValue(c, out LetterData data))
//             {
//                 Image img = slotObj.GetComponent<Image>();
//                 if (img != null && data.sprite != null)
//                     img.sprite = data.sprite;
//             }

//             LetterSlot slotScript = slotObj.GetComponent<LetterSlot>();
//             // if (slotScript != null)
//             // {
//             //     slotScript.correctLetter = c;
//             //     activeSlots.Add(slotScript);
//             // }

//             if (slotScript != null)
//             {
//                 slotScript.correctLetter = c;
//                 slotScript.slotIndex = i; // 👈 auto-assign index
//                 activeSlots.Add(slotScript);
//             }
//         }

//         Debug.Log($"🧩 Created {activeSlots.Count} slots for word: {word}");
//     }

//     // 🔠 Check if this letter can be placed
//     public bool CanPlaceLetter(char letter)
//     {
//         if (nextLetterIndex >= currentWord.Length)
//             return false;

//         char expected = currentWord[nextLetterIndex];
//         return char.ToUpper(letter) == expected;
//     }

//     public void OnCorrectLetterPlaced(float letterClipLength)
//     {
//         nextLetterIndex++;

//         // ✅ Word finished
//         if (nextLetterIndex >= currentWord.Length)
//         {
//             Debug.Log($"🎉 Word Complete: {currentWord}");
//             StartCoroutine(InvokeWordCompleteAfter(letterClipLength));
//         }
//     }

//     // private IEnumerator InvokeWordCompleteAfter(float delay)
//     // {
//     //     yield return new WaitForSeconds(delay + 0.1f);

//     //     PlayCompleteSound(); // 👈 Play complete sound
//     //     OnWordCompleted?.Invoke();

//     //     // 🕹️ Animate placed letters
//     //     int index = 0;
//     //     foreach (var letter in placedLetters)
//     //     {
//     //         if (letter != null)
//     //         {
//     //             letter.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 6, 0.5f)
//     //                 .SetDelay(index * 0.1f);
//     //             index++;
//     //         }
//     //     }

//     //     // 🎉 Optional confetti
//     //     if (confettiPrefab != null && wordArea != null)
//     //     {
//     //         Instantiate(confettiPrefab, wordArea.position, Quaternion.identity);
//     //     }

//     //     // 💥 Optional camera shake
//     //     Camera.main?.DOShakePosition(0.4f, 10f, 10, 90f, false);
//     // }

//     // 🔊 Play letter sound and return its length
    
//     private IEnumerator InvokeWordCompleteAfter(float delay)
//     {
//         yield return new WaitForSeconds(delay + 0.1f);

//         // ❌ Remove this line:
//         // PlayCompleteSound(); 

//         OnWordCompleted?.Invoke(); // still needed — this tells AnimalManager the word is done

//         // keep animations, confetti, etc.
//         int index = 0;
//         foreach (var letter in placedLetters)
//         {
//             if (letter != null)
//             {
//                 letter.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 6, 0.5f)
//                     .SetDelay(index * 0.1f);
//                 index++;
//             }
//         }

//         if (confettiPrefab != null && wordArea != null)
//             Instantiate(confettiPrefab, wordArea.position, Quaternion.identity);

//         Camera.main?.DOShakePosition(0.4f, 10f, 10, 90f, false);
//     }


//     public float PlayLetterSound(char letter)
//     {
//         letter = char.ToUpper(letter);
//         if (letterDict.TryGetValue(letter, out LetterData data) && data.sound != null)
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlaySFX(data.sound, soundVolume);
//             return data.sound.length;
//         }
//         else
//         {
//             Debug.LogWarning($"⚠️ Missing sound for letter: {letter}");
//             return 0f;
//         }
//     }

//     // ❌ Play wrong letter sound
//     public void PlayWrongSound()
//     {
//         if (wrongLetterSound != null)
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlaySFX(wrongLetterSound, soundVolume);
//         }
//     }

//     // 🔊 Play complete sound when word finishes
//     private void PlayCompleteSound()
//     {
//         if (completeSound != null)
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlaySFX(completeSound, completeSoundVolume);
//         }
//         else
//             Debug.LogWarning("⚠️ No complete sound assigned!");
//     }

//     public void AdvanceProgress()
//     {
//         currentFilledIndex++;
//     }
// }



using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using System; // Added for Action delegate

[System.Serializable]
public class LetterData
{
    public Sprite sprite;   // Letter image (A–Z)
    public AudioClip sound; // Letter sound (A–Z)
}

public class WordManager : MonoBehaviour
{
    public static WordManager instance;
    public List<Transform> placedLetters = new List<Transform>();

    [Header("Slot Setup")]
    public GameObject slotPrefab;
    public Transform slotsParent;

    [Header("Letter Data (A–Z in order)")]
    [Tooltip("Assign 26 letter sprites and sounds (A–Z).")]
    public List<LetterData> letters = new List<LetterData>(26);

    [Header("Layout Settings")]
    public float spacing = 100f;
    public float slotScale = 1.0f;

    [Header("Sounds")]
    public AudioClip wrongLetterSound;
    [Range(0f, 1f)] public float soundVolume = 0.8f;

    [Header("✅ Complete Sound Settings")]
    public AudioClip completeSound;
    [Range(0f, 1f)] public float completeSoundVolume = 1.0f;

    [Header("🎵 Background Music Settings")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.5f;
    private AudioSource bgMusicSource;

    [Header("🎉 FX Settings")]
    [Tooltip("Optional confetti prefab for word completion.")]
    public GameObject confettiPrefab;
    [Tooltip("Parent transform for confetti spawn, usually the same as slotsParent.")]
    public Transform wordArea;

    public System.Action OnWordCompleted;

    private List<LetterSlot> activeSlots = new List<LetterSlot>();
    private Dictionary<char, LetterData> letterDict = new Dictionary<char, LetterData>();

    private int nextLetterIndex = 0;
    private string currentWord = "LION";

    public string CurrentWord => currentWord;

    private int currentFilledIndex = 0;
    public int CurrentFilledIndex => currentFilledIndex;

    private void Awake()
    {
        instance = this;
        BuildLetterDictionary();
        SetupBackgroundMusic();
    }

    // 🎶 Setup and play background music
    private void SetupBackgroundMusic()
    {
        if (backgroundMusic == null) return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
        else
        {
            bgMusicSource = gameObject.AddComponent<AudioSource>();
            bgMusicSource.clip = backgroundMusic;
            bgMusicSource.loop = true;
            bgMusicSource.volume = backgroundMusicVolume;
            bgMusicSource.playOnAwake = false;
            bgMusicSource.Play();
        }
    }

    // 🧩 Build A–Z sprite/sound lookup
    private void BuildLetterDictionary()
    {
        letterDict.Clear();

        for (int i = 0; i < letters.Count; i++)
        {
            char c = (char)('A' + i);
            letterDict[c] = letters[i];
        }

        if (letters.Count < 26)
            Debug.LogWarning($"⚠️ Only {letters.Count}/26 letters assigned!");
        else
            Debug.Log("✅ Loaded all 26 letter sprites and sounds!");
    }

    // 🧱 Create slots for the current word
    public void CreateSlots(string word)
    {
        if (slotPrefab == null || slotsParent == null)
        {
            Debug.LogError("❌ Slot prefab or parent missing!");
            return;
        }

        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);

        activeSlots.Clear();

        currentWord = word.ToUpper();
        currentFilledIndex = 0;
        nextLetterIndex = 0;

        float totalWidth = (word.Length - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < word.Length; i++)
        {
            char c = currentWord[i];
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            slotObj.name = $"Slot_{c}_{i}";
            slotObj.transform.localScale = Vector3.one * slotScale;

            RectTransform rt = slotObj.GetComponent<RectTransform>();
            if (rt != null)
                rt.anchoredPosition = new Vector2(startX + i * spacing, 0);

            if (letterDict.TryGetValue(c, out LetterData data))
            {
                Image img = slotObj.GetComponent<Image>();
                if (img != null && data.sprite != null)
                    img.sprite = data.sprite;
            }

            LetterSlot slotScript = slotObj.GetComponent<LetterSlot>();
            
            if (slotScript != null)
            {
                slotScript.correctLetter = c;
                slotScript.slotIndex = i; // 👈 auto-assign index
                activeSlots.Add(slotScript);
            }
        }

        Debug.Log($"🧩 Created {activeSlots.Count} slots for word: {word}");
    }

    // 🔠 Check if this letter can be placed
    public bool CanPlaceLetter(char letter)
    {
        if (nextLetterIndex >= currentWord.Length)
            return false;

        char expected = currentWord[nextLetterIndex];
        return char.ToUpper(letter) == expected;
    }

    public void OnCorrectLetterPlaced(float letterClipLength)
    {
        nextLetterIndex++;

        // ✅ Word finished
        if (nextLetterIndex >= currentWord.Length)
        {
            Debug.Log($"🎉 Word Complete: {currentWord}");
            StartCoroutine(InvokeWordCompleteAfter(letterClipLength));
        }
    }

    private IEnumerator InvokeWordCompleteAfter(float delay)
    {
        // Wait for the final letter sound to finish
        yield return new WaitForSeconds(delay + 0.1f);

        // 1. We rely on the 'OnWordCompleted' subscriber to play the sound.
        // ❌ REMOVED: PlayCompleteSound(); // Prevented the double-sound issue.

        // 🕹️ Animate placed letters
        int index = 0;
        foreach (var letter in placedLetters)
        {
            if (letter != null)
            {
                letter.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 6, 0.5f)
                    .SetDelay(index * 0.1f);
                index++;
            }
        }

        // 🎉 Optional confetti
        if (confettiPrefab != null && wordArea != null)
            Instantiate(confettiPrefab, wordArea.position, Quaternion.identity);

        // 💥 Optional camera shake
        Camera.main?.DOShakePosition(0.4f, 10f, 10, 90f, false);

        // Wait for animations to finish before showing the ad
        yield return new WaitForSeconds(0.6f); 

        // 2. Check and Show Interstitial Ad (New Logic)
        if (Interstitial.instance != null && Interstitial.instance.IsAdAvailable())
        {
            Debug.Log("Ad available after word completion. Showing interstitial and deferring next word...");

            // Show ad, and only call OnWordCompleted() after the ad closes (via the callback)
            Interstitial.instance.ShowInterstitialAd(OnWordCompleted);
        }
        else
        {
            Debug.Log("Ad not available or Interstitial manager missing. Starting next word immediately.");
            // If ad is not ready, execute the word completion delegate immediately
            OnWordCompleted?.Invoke();
        }
    }

    // 🔊 Play letter sound and return its length
    public float PlayLetterSound(char letter)
    {
        letter = char.ToUpper(letter);
        if (letterDict.TryGetValue(letter, out LetterData data) && data.sound != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(data.sound, soundVolume);
            return data.sound.length;
        }
        else
        {
            Debug.LogWarning($"⚠️ Missing sound for letter: {letter}");
            return 0f;
        }
    }

    // ❌ Play wrong letter sound
    public void PlayWrongSound()
    {
        if (wrongLetterSound != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(wrongLetterSound, soundVolume);
        }
    }

    // 🔊 Play complete sound when word finishes
    // This method is now intended to be called by the subscriber of OnWordCompleted.
    public void PlayCompleteSound()
    {
        if (completeSound != null)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(completeSound, completeSoundVolume);
        }
        else
            Debug.LogWarning("⚠️ No complete sound assigned!");
    }

    public void AdvanceProgress()
    {
        currentFilledIndex++;
    }
}