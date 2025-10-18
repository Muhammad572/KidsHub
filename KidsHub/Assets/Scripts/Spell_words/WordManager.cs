// using UnityEngine;
// using UnityEngine.UI;
// using System.Collections.Generic;

// [System.Serializable]
// public class LetterData
// {
//     public Sprite sprite;   // Letter image (A–Z)
//     public AudioClip sound; // Letter sound (A–Z)
// }

// public class WordManager : MonoBehaviour
// {
//     public static WordManager instance;

//     [Header("Slot Setup")]
//     public GameObject slotPrefab;
//     public Transform slotsParent;

//     [Header("Letter Data (A–Z in order)")]
//     [Tooltip("Assign 26 letter sprites and sounds (A–Z).")]
//     public List<LetterData> letters = new List<LetterData>(26);

//     [Header("Layout Settings")]
//     public float spacing = 100f;
//     public float slotScale = 1.0f;

//     [Header("Wrong Sound")]
//     public AudioClip wrongLetterSound;
//     [Range(0f, 1f)] public float soundVolume = 0.8f;

//     public System.Action OnWordCompleted;

//     private List<LetterSlot> slots = new List<LetterSlot>();
//     private Dictionary<char, LetterData> letterDict = new Dictionary<char, LetterData>();

//     private void Awake()
//     {
//         instance = this;
//         BuildLetterDictionary();
//     }

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

//     public void CreateSlots(string word)
//     {
//         // Clear previous slots
//         foreach (Transform child in slotsParent)
//             Destroy(child.gameObject);
//         slots.Clear();

//         float totalWidth = (word.Length - 1) * spacing;
//         float startX = -totalWidth / 2f;

//         for (int i = 0; i < word.Length; i++)
//         {
//             char c = char.ToUpper(word[i]);
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
//             if (slotScript != null)
//                 slotScript.correctLetter = c;

//             slots.Add(slotScript);
//         }

//         Debug.Log($"🧩 Created UI slots for word: {word}");
//     }

//     public void CheckCompletion()
//     {
//         string formedWord = "";
//         string targetWord = "";

//         foreach (LetterSlot slot in slots)
//         {
//             if (slot.IsFilled())
//                 formedWord += slot.CurrentLetter();
//             else
//                 formedWord += "_";

//             targetWord += slot.correctLetter;
//         }

//         if (formedWord == targetWord)
//         {
//             Debug.Log($"🎉 Word complete: {targetWord}");
//             OnWordCompleted?.Invoke();
//         }
//     }

//     public int GetSlotIndex(LetterSlot slot) => slots.IndexOf(slot);

//     public bool CanFillSlot(int index)
//     {
//         for (int i = 0; i < index; i++)
//         {
//             if (!slots[i].IsFilled())
//                 return false;
//         }
//         return true;
//     }

//     // ✅ Plays the sound for a specific letter (A–Z)
//     public void PlayLetterSound(char letter)
//     {
//         letter = char.ToUpper(letter);
//         if (letterDict.TryGetValue(letter, out LetterData data) && data.sound != null)
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlaySFX(data.sound, soundVolume);
//             else
//                 AudioSource.PlayClipAtPoint(data.sound, Vector3.zero, soundVolume);
//         }
//         else
//         {
//             Debug.LogWarning($"⚠️ No sound found for letter: {letter}");
//         }
//     }

//     // ✅ Plays wrong sound (used by slots)
//     public void PlayWrongSound()
//     {
//         if (wrongLetterSound != null)
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlaySFX(wrongLetterSound, soundVolume);
//             else
//                 AudioSource.PlayClipAtPoint(wrongLetterSound, Vector3.zero, soundVolume);
//         }
//     }
// }


//using UnityEngine;
//using UnityEngine.UI;
//using System.Collections; 
//using System.Collections.Generic;

//[System.Serializable]
//public class LetterData
//{
//    public Sprite sprite;   // Letter image (A–Z)
//    public AudioClip sound; // Letter sound (A–Z)
//}

//public class WordManager : MonoBehaviour
//{
//    public static WordManager instance;

//    [Header("Slot Setup")]
//    public GameObject slotPrefab;
//    public Transform slotsParent;

//    [Header("Letter Data (A–Z in order)")]
//    [Tooltip("Assign 26 letter sprites and sounds (A–Z).")]
//    public List<LetterData> letters = new List<LetterData>(26);

//    [Header("Layout Settings")]
//    public float spacing = 100f;
//    public float slotScale = 1.0f;

//    [Header("Sounds")]
//    public AudioClip wrongLetterSound;
//    [Range(0f, 1f)] public float soundVolume = 0.8f;

//    public System.Action OnWordCompleted;

//    private List<LetterSlot> slots = new List<LetterSlot>();
//    private Dictionary<char, LetterData> letterDict = new Dictionary<char, LetterData>();

//    private int nextLetterIndex = 0;
//    private string currentWord = "LION";

//    private void Awake()
//    {
//        instance = this;
//        BuildLetterDictionary();
//    }

//    // 🧩 Build A–Z sprite/sound lookup
//    private void BuildLetterDictionary()
//    {
//        letterDict.Clear();

//        for (int i = 0; i < letters.Count; i++)
//        {
//            char c = (char)('A' + i);
//            letterDict[c] = letters[i];
//        }

//        if (letters.Count < 26)
//            Debug.LogWarning($"⚠️ Only {letters.Count}/26 letters assigned!");
//        else
//            Debug.Log("✅ Loaded all 26 letter sprites and sounds!");
//    }

//    // 🧱 Create slots for the current word
//    public void CreateSlots(string word)
//    {
//        // Cleanup previous
//        foreach (Transform child in slotsParent)
//            Destroy(child.gameObject);
//        slots.Clear();

//        currentWord = word.ToUpper();
//        nextLetterIndex = 0;

//        float totalWidth = (word.Length - 1) * spacing;
//        float startX = -totalWidth / 2f;

//        for (int i = 0; i < word.Length; i++)
//        {
//            char c = currentWord[i];
//            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
//            slotObj.name = $"Slot_{c}_{i}";
//            slotObj.transform.localScale = Vector3.one * slotScale;

//            RectTransform rt = slotObj.GetComponent<RectTransform>();
//            if (rt != null)
//                rt.anchoredPosition = new Vector2(startX + i * spacing, 0);

//            // ✅ Assign correct sprite if available
//            if (letterDict.TryGetValue(c, out LetterData data))
//            {
//                Image img = slotObj.GetComponent<Image>();
//                if (img != null && data.sprite != null)
//                    img.sprite = data.sprite;
//            }

//            LetterSlot slotScript = slotObj.GetComponent<LetterSlot>();
//            slotScript.correctLetter = c;

//            slots.Add(slotScript);
//        }

//        Debug.Log($"🧩 Created slots for word: {word}");
//    }

//    // 🔠 Check if this letter can be placed (sequence rule)
//    public bool CanPlaceLetter(char letter)
//    {
//        if (nextLetterIndex >= currentWord.Length)
//            return false;

//        char expected = currentWord[nextLetterIndex];
//        return char.ToUpper(letter) == expected;
//    }

//    public void OnCorrectLetterPlaced(float letterClipLength)
//    {
//        nextLetterIndex++;

//        // if the word is finished
//        if (nextLetterIndex >= currentWord.Length)
//        {
//            Debug.Log($"🎉 Word Complete: {currentWord}");

//            // Delay word complete callback until letter sound finishes
//            StartCoroutine(InvokeWordCompleteAfter(letterClipLength));
//        }
//    }

//    private IEnumerator InvokeWordCompleteAfter(float delay)
//    {
//        yield return new WaitForSeconds(delay + 0.1f); // small buffer
//        OnWordCompleted?.Invoke();
//    }


//    // 🔊 Play correct letter sound and return its length
//    public float PlayLetterSound(char letter)
//    {
//        letter = char.ToUpper(letter);
//        if (letterDict.TryGetValue(letter, out LetterData data) && data.sound != null)
//        {
//            AudioSource.PlayClipAtPoint(data.sound, Vector3.zero, soundVolume);
//            return data.sound.length;
//        }
//        else
//        {
//            Debug.LogWarning($"⚠️ Missing sound for letter: {letter}");
//            return 0f;
//        }
//    }


//    // ❌ Play wrong letter sound
//    public void PlayWrongSound()
//    {
//        if (wrongLetterSound != null)
//            AudioSource.PlayClipAtPoint(wrongLetterSound, Vector3.zero, soundVolume);
//    }
//}






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

    private List<LetterSlot> slots = new List<LetterSlot>();
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

    // 🧱 Create slots for the current word
    public void CreateSlots(string word)
    {
        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);
        slots.Clear();

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
            slotScript.correctLetter = c;

            slots.Add(slotScript);
        }

        Debug.Log($"🧩 Created slots for word: {word}");
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








