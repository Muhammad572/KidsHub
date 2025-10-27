using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;

public class GridMemoryGame : MonoBehaviour
{
    [Header("Message UI")]
    public TextMeshProUGUI messageText; // or TextMeshProUGUI if using TMP

    [Header("UI References")]
    public GameObject buttonPrefab;
    public Transform gridContainer;
    public Image topBoxImage;
    public Text timerText;

    [Header("Grid Settings")]
    public int rows = 2;
    public int columns = 3;
    public Vector2 spacing = new Vector2(20, 20);
    public float showTime = 10f;

    [Header("Sprites")]
    public List<Sprite> imageList = new List<Sprite>();
    public List<Sprite> boxImageList = new List<Sprite>();
    public Sprite defaultBoxImage;
    public List<Sprite> tempNumberSprites = new List<Sprite>();

    [Header("Audio Settings")]
    public AudioSource bgMusicSource;
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.6f;

    [Header("Sound Effects")]
    public AudioClip correctSound;
    public AudioClip wrongSound;
    [Range(0f, 1f)] public float sfxVolume = 0.8f;

    [Header("Loading Screen Settings")]
    public GameObject loadingScreenPrefab;
    public float loadingDuration = 2f;

    public ParticleSystem correctParticle;

    private GameObject loadingScreenInstance;
    private AudioSource sfxSource;
    private Sprite correctSprite;
    private List<GameObject> allButtons = new List<GameObject>();
    private List<Sprite> assignedSprites = new List<Sprite>();
    private bool canClick = false;

    void Start()
    {
        if (!buttonPrefab || !gridContainer || !topBoxImage || !timerText)
        {
            Debug.LogError("❌ Missing references! Assign all in Inspector.");
            return;
        }

        SetupGrid();
        SetupAudioSources();
        StartCoroutine(GameStartSequence());
    }

    IEnumerator GameStartSequence()
    {
        // Show loading at game start
        yield return StartCoroutine(ShowLoadingScreen());
        
        // 1. Populate the grid
        yield return StartCoroutine(StartNewRound()); 
        
        // 2. Run the memorize/play sequence
        yield return StartCoroutine(MemorizeAndPlayPhase());
    }

    void SetupGrid()
    {
        var grid = gridContainer.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.spacing = spacing;
        grid.childAlignment = TextAnchor.MiddleCenter;

        RectTransform parentRect = gridContainer.GetComponent<RectTransform>();
        float cellWidth = (parentRect.rect.width - spacing.x * (columns - 1)) / columns;
        float cellHeight = (parentRect.rect.height - spacing.y * (rows - 1)) / rows;
        grid.cellSize = new Vector2(cellWidth, cellHeight);
    }

    void SetupAudioSources()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.musicSource != null)
            AudioManager.Instance.musicSource.Stop();
        if (bgMusicSource == null)
            bgMusicSource = gameObject.AddComponent<AudioSource>();

        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.volume = musicVolume;
        bgMusicSource.playOnAwake = false;
        if (backgroundMusic != null) bgMusicSource.Play();

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    IEnumerator StartNewRound()
    {
        canClick = false;

        // Clear old grid
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);
        allButtons.Clear();
        assignedSprites.Clear();

        // --- ADD THIS LOGIC BACK ---

        // Choose new correct image
        if (boxImageList.Count == 0)
            boxImageList = imageList;
        correctSprite = boxImageList[Random.Range(0, boxImageList.Count)];

        // Reset top image
        topBoxImage.sprite = defaultBoxImage;

        // Pick unique images for this round
        List<Sprite> shuffled = new List<Sprite>(imageList);
        Shuffle(shuffled);
        int totalCards = rows * columns;
        // This is the line that fixes CS0103
        List<Sprite> chosenSprites = shuffled.GetRange(0, Mathf.Min(totalCards, shuffled.Count));

        // Replace one random image with correctSprite
        int matchIndex = Random.Range(0, chosenSprites.Count);
        chosenSprites[matchIndex] = correctSprite;

        // --- END OF ADDED LOGIC ---

        // Spawn cards
        for (int i = 0; i < chosenSprites.Count; i++)
        {
            // --- ADD THIS LOGIC BACK ---
            GameObject card = Instantiate(buttonPrefab, gridContainer);
            assignedSprites.Add(chosenSprites[i]);

            Transform cardImage = card.transform.Find("CardImage");
            Transform numberImage = card.transform.Find("NumberImage");

            if (cardImage)
            {
                cardImage.gameObject.SetActive(true); // ✅ ensure front is active
                cardImage.GetComponent<Image>().sprite = chosenSprites[i];
            }

            if (numberImage)
            {
                numberImage.gameObject.SetActive(false); // ✅ ensure back is hidden
                if (tempNumberSprites != null && i < tempNumberSprites.Count)
                    numberImage.GetComponent<Image>().sprite = tempNumberSprites[i];
            }

            int index = i;
            card.GetComponent<Button>().onClick.AddListener(() => OnCardClicked(index));

            allButtons.Add(card);
            // --- END OF ADDED LOGIC ---
        }

        // 🔹 Force Canvas to update and render the cards before countdown
        Canvas.ForceUpdateCanvases();
        yield return null;  // Wait a frame so the UI updates

        // This method now correctly stops here.
        // The memorize/play logic is now in MemorizeAndPlayPhase()
    }
    
    IEnumerator CorrectCardSequence()
    {
        canClick = false;

        CanvasGroup group = gridContainer.GetComponent<CanvasGroup>();
        if (group == null)
            group = gridContainer.gameObject.AddComponent<CanvasGroup>();

        // 1. Fade OUT the old grid
        gridContainer.DOScale(0.9f, 0.3f).SetEase(Ease.InOutQuad);
        group.DOFade(0f, 0.3f).SetEase(Ease.InQuad);

        yield return new WaitForSeconds(0.3f); // Wait for fade out

        // 2. Show loading
        yield return StartCoroutine(ShowLoadingScreen());

        // 3. Populate the new grid (it's still invisible)
        yield return StartCoroutine(StartNewRound()); 

        // 4. Set up the fade-in animation
        gridContainer.localScale = Vector3.one * 0.9f;
        group.alpha = 0f;
        gridContainer.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        group.DOFade(1f, 0.3f);
        
        // 5. Wait for the new grid to fade IN
        yield return new WaitForSeconds(0.3f); // Wait for fade-in to complete

        // 6. NOW run the memorize/play phase (cards are visible!)
        yield return StartCoroutine(MemorizeAndPlayPhase());
    }


    IEnumerator FlipAllCardsToNumbers()
    {
        foreach (var card in allButtons)
        {
            Transform front = card.transform.Find("CardImage");
            Transform back = card.transform.Find("NumberImage");
            if (front && back)
                yield return StartCoroutine(FlipCardWithTween(front.gameObject, back.gameObject));
        }
    }


    // IEnumerator FlipCardWithTween(GameObject front, GameObject back)
    // {
    //     if (front == null || back == null) yield break;

    //     front.transform.DORotate(new Vector3(0, 90, 0), 0.2f).SetEase(Ease.InQuad);
    //     yield return new WaitForSeconds(0.2f);

    //     front.SetActive(false);
    //     back.SetActive(true);
    //     back.transform.localEulerAngles = new Vector3(0, 270, 0);
    //     back.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuad);
    // }

    IEnumerator FlipCardWithTween(GameObject front, GameObject back)
    {
        if (front == null || back == null) yield break;

        // Flip front to middle
        front.transform.DORotate(new Vector3(0, 90, 0), 0.2f).SetEase(Ease.InQuad);
        yield return new WaitForSeconds(0.2f);

        // Switch sides
        front.SetActive(false);
        back.SetActive(true);
        back.transform.localEulerAngles = new Vector3(0, 270, 0);

        // Flip back to front
        back.transform.DORotate(Vector3.zero, 0.2f).SetEase(Ease.OutQuad);

        // 🔹 Extra: punch scale for pop effect
        back.transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 8, 1f);

        yield return new WaitForSeconds(0.2f);
    }

    // void OnCardClicked(int index)
    // {
    //     if (!canClick) return;

    //     GameObject clickedCard = allButtons[index];
    //     Sprite clickedSprite = assignedSprites[index];

    //     if (clickedSprite == correctSprite)
    //     {
    //         timerText.text = "✅ Correct! Next round...";
    //         PlaySFX(correctSound);

    //         clickedCard.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 1f);
    //         clickedCard.transform.DOShakeRotation(0.3f, new Vector3(0, 20, 0), 10);

    //         if (correctParticle)
    //         {
    //             ParticleSystem ps = Instantiate(correctParticle, clickedCard.transform.position, Quaternion.identity);
    //             Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
    //         }

    //         StartCoroutine(CorrectCardSequence());
    //     }
    //     else
    //     {
    //         timerText.text = "❌ Wrong! Try again.";
    //         PlaySFX(wrongSound);

    //         clickedCard.transform.DOShakePosition(0.3f, new Vector3(15f, 0, 0), 10, 90, false, true);
    //         clickedCard.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.3f);
    //     }
    // }

    void OnCardClicked(int index)
    {
        if (!canClick) return;

        GameObject clickedCard = allButtons[index];
        Sprite clickedSprite = assignedSprites[index];

        if (clickedSprite == correctSprite)
        {
            timerText.text = "✅ Correct! Next round...";
            PlaySFX(correctSound);

            clickedCard.transform.DOPunchScale(Vector3.one * 0.2f, 0.3f, 8, 1f);
            clickedCard.transform.DOShakeRotation(0.3f, new Vector3(0, 20, 0), 10);

            // --- CHANGES START HERE ---
            if (correctParticle)
            {
                // Instantiate the particle system at the clicked card's position
                ParticleSystem ps = Instantiate(correctParticle, clickedCard.transform.position, Quaternion.identity);
                // Make sure the particle system plays
                ps.Play();
                // Destroy it after its natural duration
                Destroy(ps.gameObject, ps.main.duration + ps.main.startLifetime.constantMax);
            }

            // Immediately disable further clicks to prevent spam while particles play
            canClick = false; 

            // Start the sequence to transition to the next round after a short delay
            // This delay allows the particle effect to be visible before fading out
            StartCoroutine(StartTransitionToNextRound());
            // --- CHANGES END HERE ---
        }
        else
        {
            timerText.text = "❌ Wrong! Try again.";
            PlaySFX(wrongSound);

            clickedCard.transform.DOShakePosition(0.3f, new Vector3(15f, 0, 0), 10, 90, false, true);
            clickedCard.transform.DOPunchRotation(new Vector3(0, 0, 10f), 0.3f);
        }
    }
    IEnumerator StartTransitionToNextRound()
    {
        // Give a very brief moment for particles to appear before the screen starts fading out
        yield return new WaitForSeconds(0.5f); // Adjust this duration as needed

        // Now initiate the sequence that fades out, loads, and brings in the new round
        yield return StartCoroutine(CorrectCardSequence());
    }


    IEnumerator ShowLoadingScreen()
{
    if (loadingScreenPrefab == null)
    {
        Debug.LogWarning("⚠️ No loading screen GameObject assigned!");
        yield break;
    }

    // Just toggle visibility, since it's already in the scene hierarchy
    loadingScreenPrefab.SetActive(true);

    yield return new WaitForSeconds(loadingDuration);

    loadingScreenPrefab.SetActive(false);
}



    void PlaySFX(AudioClip clip)
    {
        if (clip != null)
            sfxSource.PlayOneShot(clip, sfxVolume);
    }

    void Shuffle(List<Sprite> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Sprite temp = list[i];
            int rand = Random.Range(i, list.Count);
            list[i] = list[rand];
            list[rand] = temp;
        }
    }

    // IEnumerator MemorizeAndPlayPhase()
    // {
    //     // 1. Run Memorize Timer
    //     float timeLeft = showTime;
    //     while (timeLeft > 0)
    //     {
    //         timerText.text = $"Memorize: {Mathf.CeilToInt(timeLeft)}s";
    //         timerText.transform.DOKill(true);
    //         timerText.transform.localScale = Vector3.one;
    //         timerText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
    //         timeLeft -= Time.deltaTime;
    //         yield return null;
    //     }

    //     // 2. Flip cards
    //     yield return StartCoroutine(FlipAllCardsToNumbers());

    //     // 3. Start "Find" phase
    //     topBoxImage.sprite = correctSprite;
    //     timerText.text = "Find the matching card!";
    //     canClick = true;
    // }

    IEnumerator MemorizeAndPlayPhase()
    {
        // Show message before timer
        yield return StartCoroutine(ShowMessage("Memorize them carefully!", 2f));

        // 1. Run Memorize Timer
        float timeLeft = showTime;
        while (timeLeft > 0)
        {
            timerText.text = $"Memorize: {Mathf.CeilToInt(timeLeft)}s";
            timerText.transform.DOKill(true);
            timerText.transform.localScale = Vector3.one;
            // timerText.transform.DOPunchScale(Vector3.one * 0.2f, 0.2f);
            timerText.transform.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // 2. Flip cards
        yield return StartCoroutine(FlipAllCardsToNumbers());

        // Show encouraging message
        yield return StartCoroutine(ShowMessage("Find the matching card!", 1.5f));

        // 3. Start "Find" phase
        topBoxImage.sprite = correctSprite;
        canClick = true;
    }


    // IEnumerator ShowMessage(string message, float duration = 2f)
    // {
    //     if (messageText == null) yield break;

    //     messageText.text = message;
    //     messageText.gameObject.SetActive(true);

    //     // Reset scale and alpha
    //     messageText.transform.localScale = Vector3.zero;
    //     messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1f);

    //     // Animate scale with punch
    //     messageText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack);

    //     // Optional: fade out after duration
    //     yield return new WaitForSeconds(duration);

    //     messageText.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack)
    //         .OnComplete(() => messageText.gameObject.SetActive(false));
    // }

    IEnumerator ShowMessage(string message, float duration = 2f)
    {
        gridContainer.GetComponent<CanvasGroup>().DOFade(0.7f, 0.3f).SetLoops(2, LoopType.Yoyo);

        if (messageText == null) yield break;

        messageText.text = message;
        messageText.gameObject.SetActive(true);

        // Reset scale, rotation, and alpha
        messageText.transform.localScale = Vector3.zero;
        messageText.transform.localEulerAngles = Vector3.zero;
        messageText.color = new Color(messageText.color.r, messageText.color.g, messageText.color.b, 1f);

        // Sequence for scale + punch + color flash
        Sequence seq = DOTween.Sequence();
        seq.Append(messageText.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack))
        .Join(messageText.transform.DOPunchRotation(new Vector3(0, 0, 15), 0.5f, 10, 1f))
        .Append(messageText.DOColor(Color.yellow, 0.25f).SetLoops(2, LoopType.Yoyo))
        .AppendInterval(duration)
        .Append(messageText.transform.DOScale(0f, 0.5f).SetEase(Ease.InBack))
        .OnComplete(() => messageText.gameObject.SetActive(false));

        yield return seq.WaitForCompletion();
    }


}
