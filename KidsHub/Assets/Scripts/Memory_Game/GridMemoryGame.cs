using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GridMemoryGame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buttonPrefab;    
    public Transform gridContainer;    
    public Image topBoxImage;          
    public Text timerText;             

    [Header("Grid Settings")]
    public int rows = 2;
    public int columns = 3;
    public Vector2 cardSize = new Vector2(200, 200);
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
    [Tooltip("Assign your loading screen prefab here.")]
    public GameObject loadingScreenPrefab;
    [Tooltip("How long the loading screen should stay visible (in seconds).")]
    public float loadingDuration = 2f; // adjustable

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
        // 🔹 Show loading at game start
        yield return StartCoroutine(ShowLoadingScreen());
        yield return StartCoroutine(StartNewRound());
    }

    //void SetupGrid()
    //{
    //    var grid = gridContainer.GetComponent<GridLayoutGroup>();
    //    if (grid == null) grid = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

    //    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    //    grid.constraintCount = columns;
    //    grid.cellSize = cardSize;
    //    grid.spacing = spacing;
    //    grid.childAlignment = TextAnchor.MiddleCenter;
    //}

    void SetupGrid()
{
    var grid = gridContainer.GetComponent<GridLayoutGroup>();
    if (grid == null) grid = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    grid.constraintCount = columns;
    grid.spacing = spacing;
    grid.childAlignment = TextAnchor.MiddleCenter;

    // Dynamically calculate cell size
    RectTransform parentRect = gridContainer.GetComponent<RectTransform>();
    float cellWidth = (parentRect.rect.width - spacing.x * (columns - 1)) / columns;
    float cellHeight = (parentRect.rect.height - spacing.y * (rows - 1)) / rows;
    grid.cellSize = new Vector2(cellWidth, cellHeight);
}


    void SetupAudioSources()
    {
        if (bgMusicSource == null)
            bgMusicSource = gameObject.AddComponent<AudioSource>();

        bgMusicSource.clip = backgroundMusic;
        bgMusicSource.loop = true;
        bgMusicSource.volume = musicVolume;
        bgMusicSource.playOnAwake = false;

        if (backgroundMusic != null)
            bgMusicSource.Play();
        else
            Debug.LogWarning("⚠️ No background music clip assigned.");

        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.loop = false;
        sfxSource.playOnAwake = false;
    }

    IEnumerator StartNewRound()
    {
        canClick = false;

        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);
        allButtons.Clear();
        assignedSprites.Clear();

        if (boxImageList.Count == 0)
            boxImageList = imageList;
        correctSprite = boxImageList[Random.Range(0, boxImageList.Count)];

        topBoxImage.sprite = defaultBoxImage;

        List<Sprite> shuffled = new List<Sprite>(imageList);
        Shuffle(shuffled);
        int totalCards = rows * columns;
        List<Sprite> chosenSprites = new List<Sprite>();
        for (int i = 0; i < totalCards && i < shuffled.Count; i++)
            chosenSprites.Add(shuffled[i]);

        int matchIndex = Random.Range(0, totalCards);
        chosenSprites[matchIndex] = correctSprite;

        for (int i = 0; i < totalCards; i++)
        {
            GameObject card = Instantiate(buttonPrefab, gridContainer);
            assignedSprites.Add(chosenSprites[i]);

            Transform cardImage = card.transform.Find("CardImage");
            Transform numberImage = card.transform.Find("NumberImage");

            if (cardImage)
            {
                Image img = cardImage.GetComponent<Image>();
                if (img != null)
                    img.sprite = chosenSprites[i];
            }

            if (numberImage)
            {
                numberImage.gameObject.SetActive(false);

                if (tempNumberSprites != null && i < tempNumberSprites.Count)
                {
                    Image numImg = numberImage.GetComponent<Image>();
                    if (numImg != null)
                        numImg.sprite = tempNumberSprites[i];
                }
            }

            int index = i;
            Button btn = card.GetComponent<Button>();
            if (btn)
                btn.onClick.AddListener(() => OnCardClicked(index));

            allButtons.Add(card);
        }

        float timeLeft = showTime;
        while (timeLeft > 0)
        {
            timerText.text = $"Memorize: {Mathf.CeilToInt(timeLeft)}s";
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < allButtons.Count; i++)
        {
            Transform cardImage = allButtons[i].transform.Find("CardImage");
            Transform numberImage = allButtons[i].transform.Find("NumberImage");

            if (cardImage)
                cardImage.gameObject.SetActive(false);

            if (numberImage)
                numberImage.gameObject.SetActive(true);
        }

        topBoxImage.sprite = correctSprite;

        timerText.text = "Find the matching card!";
        canClick = true;
    }

    void OnCardClicked(int index)
    {
        if (!canClick) return;

        Sprite clickedSprite = assignedSprites[index];
        if (clickedSprite == correctSprite)
        {
            Debug.Log("✅ Correct!");
            timerText.text = "✅ Correct! Next round...";
            PlaySFX(correctSound);
            StartCoroutine(CorrectCardSequence());
        }
        else
        {
            Debug.Log("❌ Wrong!");
            timerText.text = "❌ Wrong! Try again.";
            PlaySFX(wrongSound);
        }
    }

    IEnumerator CorrectCardSequence()
    {
        canClick = false;

        // 🔹 Show loading screen after correct card
        yield return StartCoroutine(ShowLoadingScreen());

        yield return StartCoroutine(StartNewRound());
    }

    IEnumerator ShowLoadingScreen()
    {
        if (loadingScreenPrefab == null)
        {
            Debug.LogWarning("⚠️ No loading screen prefab assigned!");
            yield break;
        }

        // Instantiate if not already
        if (loadingScreenInstance == null)
            loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);

        loadingScreenInstance.SetActive(true);
        yield return new WaitForSeconds(loadingDuration);
        loadingScreenInstance.SetActive(false);
    }

    void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
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
}


