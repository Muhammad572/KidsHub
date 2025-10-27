// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class AlphabetMatchManager : MonoBehaviour
// {
//     [Header("Alphabet Sprites (In Order A–Z)")]
//     public List<Sprite> inactiveAlphabetSprites; // 26 sprites in order
//     public List<Sprite> activeAlphabetSprites;   // 26 sprites in order

//     [Header("Alphabet Sounds (In Order A–Z)")]
//     public List<AudioClip> alphabetSounds;       // 26 clips (A–Z)

//     [Header("Current UI Slots")]
//     public List<Image> currentAlphabetSlots;     // 4 UI slots on screen

//     [Header("Audio Sources")]
//     public AudioSource musicSource;              // Background music
//     public AudioSource sfxSource;                // Pop/fail/alphabet sounds

//     [Header("Sound Effects")]
//     public AudioClip bubblePopSound;
//     public AudioClip failSound;
//     public AudioClip backgroundMusic;

//     [Header("Volume Controls")]
//     [Range(0f, 1f)] public float musicVolume = 0.5f;
//     [Range(0f, 1f)] public float sfxVolume = 1.0f;

//     [Header("Settings")]
//     public float nextSetDelay = 0.8f;
//     public float appearScale = 1.2f;

//     // --- New Shake Settings ---
//     [Header("Shake Settings")]
//     public float shakeDuration = 0.2f; // How long the shake lasts
//     public float shakeMagnitude = 5f;  // How much the elements move

//     [Header("Loading Screen")]
//     public GameObject loadingScreen;   // Assign a UI panel here
//     public float loadingDuration = 2f; // Show for 2 seconds

//     private char[] currentLetters = new char[4];
//     private bool[] matched = new bool[4];
//     private int matchedCount = 0;

//     void Start()
//     {
//         // --- Start background music ---
//         if (musicSource != null && backgroundMusic != null)
//         {
//             musicSource.clip = backgroundMusic;
//             musicSource.loop = true;
//             musicSource.volume = musicVolume;
//             musicSource.Play();
//         }

//         if (inactiveAlphabetSprites.Count < 26 || activeAlphabetSprites.Count < 26)
//         {
//             Debug.LogError("AlphabetMatchManager: Please assign 26 sprites (A–Z) in both lists!");
//             return;
//         }

//         if (loadingScreen != null)
//             loadingScreen.SetActive(false);

//         // 👇 Show loading screen at game start
//         StartCoroutine(GameStartRoutine());
//     }

//     void Update()
//     {
//         if (musicSource != null)
//             musicSource.volume = musicVolume;

//         if (sfxSource != null)
//             sfxSource.volume = sfxVolume;
//     }

//     private IEnumerator GameStartRoutine()
//     {
//         if (loadingScreen != null)
//             loadingScreen.SetActive(true);

//         yield return new WaitForSeconds(loadingDuration);

//         if (loadingScreen != null)
//             loadingScreen.SetActive(false);

//         SetupNewSet();
//     }

//     public bool OnBubblePopped(char poppedLetter)
//     {
//         for (int i = 0; i < currentLetters.Length; i++)
//         {
//             if (!matched[i] && poppedLetter == currentLetters[i])
//             {
//                 matched[i] = true;
//                 matchedCount++;
//                 ShowActiveAlphabet(i);

//                 PlaySFX(bubblePopSound);
//                 PlayAlphabetSound(poppedLetter);

//                 // 👇 When all 4 matched → show loading screen again
//                 if (matchedCount >= 4)
//                     StartCoroutine(LoadingScreenRoutine());

//                 return true;
//             }
//         }

//         // --- NEW LOGIC FOR WRONG MATCH ---
//         // If the loop finishes without a match, it's a wrong click.
//         StartCoroutine(ShakeAllSlots());
//         // ---------------------------------

//         PlaySFX(failSound);
//         return false;
//     }

//     private void SetupNewSet()
//     {
//         matchedCount = 0;
//         for (int i = 0; i < matched.Length; i++) matched[i] = false;

//         for (int i = 0; i < currentAlphabetSlots.Count; i++)
//         {
//             char randomLetter = (char)('A' + Random.Range(0, 26));
//             currentLetters[i] = randomLetter;
//             currentAlphabetSlots[i].sprite = GetInactiveSprite(randomLetter);
//             currentAlphabetSlots[i].color = Color.white;
//             currentAlphabetSlots[i].transform.localScale = Vector3.one;
//         }
//     }

//     private void ShowActiveAlphabet(int index)
//     {
//         char letter = currentLetters[index];
//         currentAlphabetSlots[index].sprite = GetActiveSprite(letter);
//         StartCoroutine(AnimatePop(currentAlphabetSlots[index]));
//     }

//     private IEnumerator AnimatePop(Image img)
//     {
//         Vector3 startScale = img.transform.localScale;
//         Vector3 targetScale = Vector3.one * appearScale;
//         float t = 0;

//         while (t < 1)
//         {
//             t += Time.deltaTime * 8f;
//             img.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
//             yield return null;
//         }

//         t = 0;
//         while (t < 1)
//         {
//             t += Time.deltaTime * 8f;
//             img.transform.localScale = Vector3.Lerp(targetScale, Vector3.one, t);
//             yield return null;
//         }
//     }

//     // 👇 Loading screen appears for 2 seconds after 4 boxes are filled
//     private IEnumerator LoadingScreenRoutine()
//     {
//         if (loadingScreen != null)
//             loadingScreen.SetActive(true);

//         yield return new WaitForSeconds(loadingDuration);

//         if (loadingScreen != null)
//             loadingScreen.SetActive(false);

//         StartCoroutine(NextSetRoutine());
//     }

//     private IEnumerator NextSetRoutine()
//     {
//         yield return new WaitForSeconds(nextSetDelay);
//         foreach (var slot in currentAlphabetSlots)
//             StartCoroutine(FadeOut(slot));

//         yield return new WaitForSeconds(0.6f);
//         SetupNewSet();
//     }

//     private IEnumerator FadeOut(Image img)
//     {
//         Color c = img.color;
//         float t = 0;
//         while (t < 1)
//         {
//             t += Time.deltaTime * 2f;
//             c.a = Mathf.Lerp(1f, 0f, t);
//             img.color = c;
//             yield return null;
//         }
//         c.a = 1f;
//         img.color = c;
//     }

//     // --- NEW SHAKE COROUTINE ---
//     private IEnumerator ShakeAllSlots()
//     {
//         float elapsed = 0.0f;
//         List<Vector3> originalPositions = new List<Vector3>();
//         foreach (var slot in currentAlphabetSlots)
//         {
//             originalPositions.Add(slot.transform.localPosition);
//         }

//         while (elapsed < shakeDuration)
//         {
//             for (int i = 0; i < currentAlphabetSlots.Count; i++)
//             {
//                 // Only shake slots that haven't been matched yet
//                 if (!matched[i])
//                 {
//                     float x = Random.Range(-1f, 1f) * shakeMagnitude;
//                     float y = Random.Range(-1f, 1f) * shakeMagnitude;

//                     currentAlphabetSlots[i].transform.localPosition = originalPositions[i] + new Vector3(x, y, 0);
//                 }
//             }

//             elapsed += Time.deltaTime;
//             yield return null;
//         }

//         // Reset positions
//         for (int i = 0; i < currentAlphabetSlots.Count; i++)
//         {
//             if (!matched[i])
//             {
//                 currentAlphabetSlots[i].transform.localPosition = originalPositions[i];
//             }
//         }
//     }
//     // ----------------------------

//     private Sprite GetInactiveSprite(char letter)
//     {
//         int index = letter - 'A';
//         return (index >= 0 && index < inactiveAlphabetSprites.Count)
//             ? inactiveAlphabetSprites[index] : null;
//     }

//     private Sprite GetActiveSprite(char letter)
//     {
//         int index = letter - 'A';
//         return (index >= 0 && index < activeAlphabetSprites.Count)
//             ? activeAlphabetSprites[index] : null;
//     }

//     private void PlayAlphabetSound(char letter)
//     {
//         int index = letter - 'A';
//         if (index >= 0 && index < alphabetSounds.Count)
//             PlaySFX(alphabetSounds[index]);
//     }

//     private void PlaySFX(AudioClip clip)
//     {
//         if (clip != null && sfxSource != null)
//             sfxSource.PlayOneShot(clip);
//     }

//     public List<char> GetCurrentLetters() => new List<char>(currentLetters);
// }



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AlphabetMatchManager : MonoBehaviour
{
    [Header("Alphabet Sprites (In Order A–Z)")]
    public List<Sprite> inactiveAlphabetSprites; // 26 sprites in order
    public List<Sprite> activeAlphabetSprites;   // 26 sprites in order

    [Header("Alphabet Sounds (In Order A–Z)")]
    public List<AudioClip> alphabetSounds;       // 26 clips (A–Z)

    [Header("Current UI Slots")]
    public List<Image> currentAlphabetSlots;     // 4 UI slots on screen

    [Header("Sound Effects")]
    public AudioClip bubblePopSound;
    public AudioClip failSound;
    public AudioClip backgroundMusic;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float musicVolume = 0.5f;
    [Range(0f, 1f)] public float sfxVolume = 1.0f;

    [Header("Settings")]
    public float nextSetDelay = 0.8f;
    public float appearScale = 1.2f;

    [Header("Shake Settings")]
    public float shakeDuration = 0.2f;
    public float shakeMagnitude = 5f;

    [Header("Loading Screen")]
    public GameObject loadingScreen;
    public float loadingDuration = 2f;

    private char[] currentLetters = new char[4];
    private bool[] matched = new bool[4];
    private int matchedCount = 0;

    private bool musicStarted = false;

    void Start()
    {
        if (inactiveAlphabetSprites.Count < 26 || activeAlphabetSprites.Count < 26)
        {
            Debug.LogError("AlphabetMatchManager: Please assign 26 sprites (A–Z) in both lists!");
            return;
        }

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // 👇 Start coroutine for loading and setup
        StartCoroutine(GameStartRoutine());
    }

    void Update()
    {
        // Live update volumes
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
        }
    }

    private IEnumerator GameStartRoutine()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        yield return new WaitForSeconds(loadingDuration);

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        // 🎵 Start background music once
        if (AudioManager.Instance != null && backgroundMusic != null && !musicStarted)
        {
            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            musicStarted = true;
        }

        SetupNewSet();
    }

    public bool OnBubblePopped(char poppedLetter)
    {
        for (int i = 0; i < currentLetters.Length; i++)
        {
            if (!matched[i] && poppedLetter == currentLetters[i])
            {
                matched[i] = true;
                matchedCount++;
                ShowActiveAlphabet(i);

                // ✅ Play success sounds
                if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(bubblePopSound, sfxVolume);
                    PlayAlphabetSound(poppedLetter);
                }

                // 👇 When all 4 matched → show loading screen again
                if (matchedCount >= 4)
                    StartCoroutine(LoadingScreenRoutine());

                return true;
            }
        }

        // ❌ Wrong letter
        StartCoroutine(ShakeAllSlots());
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(failSound, sfxVolume);

        return false;
    }

    private void SetupNewSet()
    {
        matchedCount = 0;
        for (int i = 0; i < matched.Length; i++) matched[i] = false;

        for (int i = 0; i < currentAlphabetSlots.Count; i++)
        {
            char randomLetter = (char)('A' + Random.Range(0, 26));
            currentLetters[i] = randomLetter;
            currentAlphabetSlots[i].sprite = GetInactiveSprite(randomLetter);
            currentAlphabetSlots[i].color = Color.white;
            currentAlphabetSlots[i].transform.localScale = Vector3.one;
        }
    }

    private void ShowActiveAlphabet(int index)
    {
        char letter = currentLetters[index];
        currentAlphabetSlots[index].sprite = GetActiveSprite(letter);
        StartCoroutine(AnimatePop(currentAlphabetSlots[index]));
    }

    private IEnumerator AnimatePop(Image img)
    {
        Vector3 startScale = img.transform.localScale;
        Vector3 targetScale = Vector3.one * appearScale;
        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 8f;
            img.transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 8f;
            img.transform.localScale = Vector3.Lerp(targetScale, Vector3.one, t);
            yield return null;
        }
    }

    private IEnumerator LoadingScreenRoutine()
    {
        if (loadingScreen != null)
            loadingScreen.SetActive(true);

        yield return new WaitForSeconds(loadingDuration);

        if (loadingScreen != null)
            loadingScreen.SetActive(false);

        StartCoroutine(NextSetRoutine());
    }

    private IEnumerator NextSetRoutine()
    {
        yield return new WaitForSeconds(nextSetDelay);
        foreach (var slot in currentAlphabetSlots)
            StartCoroutine(FadeOut(slot));

        yield return new WaitForSeconds(0.6f);
        SetupNewSet();
    }

    private IEnumerator FadeOut(Image img)
    {
        Color c = img.color;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            c.a = Mathf.Lerp(1f, 0f, t);
            img.color = c;
            yield return null;
        }
        c.a = 1f;
        img.color = c;
    }

    private IEnumerator ShakeAllSlots()
    {
        float elapsed = 0.0f;
        List<Vector3> originalPositions = new List<Vector3>();
        foreach (var slot in currentAlphabetSlots)
        {
            originalPositions.Add(slot.transform.localPosition);
        }

        while (elapsed < shakeDuration)
        {
            for (int i = 0; i < currentAlphabetSlots.Count; i++)
            {
                if (!matched[i])
                {
                    float x = Random.Range(-1f, 1f) * shakeMagnitude;
                    float y = Random.Range(-1f, 1f) * shakeMagnitude;
                    currentAlphabetSlots[i].transform.localPosition = originalPositions[i] + new Vector3(x, y, 0);
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < currentAlphabetSlots.Count; i++)
        {
            if (!matched[i])
                currentAlphabetSlots[i].transform.localPosition = originalPositions[i];
        }
    }

    private Sprite GetInactiveSprite(char letter)
    {
        int index = letter - 'A';
        return (index >= 0 && index < inactiveAlphabetSprites.Count)
            ? inactiveAlphabetSprites[index]
            : null;
    }

    private Sprite GetActiveSprite(char letter)
    {
        int index = letter - 'A';
        return (index >= 0 && index < activeAlphabetSprites.Count)
            ? activeAlphabetSprites[index]
            : null;
    }

    private void PlayAlphabetSound(char letter)
    {
        int index = letter - 'A';
        if (index >= 0 && index < alphabetSounds.Count && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(alphabetSounds[index], sfxVolume);
        }
    }

    public List<char> GetCurrentLetters() => new List<char>(currentLetters);
}
