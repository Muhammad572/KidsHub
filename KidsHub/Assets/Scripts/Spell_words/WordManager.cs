using UnityEngine;
using UnityEngine.UI;
using System.Collections; 
using System.Collections.Generic;

[System.Serializable]
public class LetterData
{
    public Sprite sprite;   // Letter image (A–Z)
    public AudioClip sound; // Letter sound (A–Z)
}

public class WordManager : MonoBehaviour
{
    public static WordManager instance;

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
    [Tooltip("This sound plays when the full word is correctly completed.")]
    public AudioClip completeSound;
    [Range(0f, 1f)] public float completeSoundVolume = 1.0f;

    [Header("🎵 Background Music Settings")]
    [Tooltip("Looping background music for the whole game.")]
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.5f;
    private AudioSource bgMusicSource;

    public System.Action OnWordCompleted;

    // private List<LetterSlot> slots = new List<LetterSlot>();
    private List<LetterSlot> activeSlots = new List<LetterSlot>();
    private Dictionary<char, LetterData> letterDict = new Dictionary<char, LetterData>();

    private int nextLetterIndex = 0;
    private string currentWord = "LION";

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

        bgMusicSource = gameObject.AddComponent<AudioSource>();
        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.volume = backgroundMusicVolume;
        bgMusicSource.playOnAwake = false;
        bgMusicSource.Play();
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

    // // 🧱 Create slots for the current word
    // public void CreateSlots(string word)
    // {
    //     foreach (Transform child in slotsParent)
    //         Destroy(child.gameObject);
    //     slots.Clear();

    //     currentWord = word.ToUpper();
    //     nextLetterIndex = 0;

    //     float totalWidth = (word.Length - 1) * spacing;
    //     float startX = -totalWidth / 2f;

    //     for (int i = 0; i < word.Length; i++)
    //     {
    //         char c = currentWord[i];
    //         GameObject slotObj = Instantiate(slotPrefab, slotsParent);
    //         slotObj.name = $"Slot_{c}_{i}";
    //         slotObj.transform.localScale = Vector3.one * slotScale;

    //         RectTransform rt = slotObj.GetComponent<RectTransform>();
    //         if (rt != null)
    //             rt.anchoredPosition = new Vector2(startX + i * spacing, 0);

    //         if (letterDict.TryGetValue(c, out LetterData data))
    //         {
    //             Image img = slotObj.GetComponent<Image>();
    //             if (img != null && data.sprite != null)
    //                 img.sprite = data.sprite;
    //         }

    //         LetterSlot slotScript = slotObj.GetComponent<LetterSlot>();
    //         slotScript.correctLetter = c;

    //         slots.Add(slotScript);
    //     }

    //     Debug.Log($"🧩 Created slots for word: {word}");
    // }

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
    
    // FIX 1: Use activeSlots instead of the non-existent slots
    activeSlots.Clear();

    currentWord = word.ToUpper();
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
        // Note: You may need a Setup(char c) method here, 
        // but for now we'll match the commented-out code's logic.
        if (slotScript != null)
        {
            slotScript.correctLetter = c;
            
            // FIX 2: Use activeSlots instead of the non-existent slots
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
        yield return new WaitForSeconds(delay + 0.1f);
        PlayCompleteSound(); // 👈 Play the complete sound here
        OnWordCompleted?.Invoke();
    }

    // 🔊 Play letter sound and return its length
    public float PlayLetterSound(char letter)
    {
        letter = char.ToUpper(letter);
        if (letterDict.TryGetValue(letter, out LetterData data) && data.sound != null)
        {
            AudioSource.PlayClipAtPoint(data.sound, Vector3.zero, soundVolume);
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
            AudioSource.PlayClipAtPoint(wrongLetterSound, Vector3.zero, soundVolume);
    }

    // 🔊 Play complete sound when word finishes
    private void PlayCompleteSound()
    {
        if (completeSound != null)
            AudioSource.PlayClipAtPoint(completeSound, Vector3.zero, completeSoundVolume);
        else
            Debug.LogWarning("⚠️ No complete sound assigned!");
    }
}
