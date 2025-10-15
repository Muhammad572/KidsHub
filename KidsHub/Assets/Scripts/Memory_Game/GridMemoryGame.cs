using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GridMemoryGame : MonoBehaviour
{
    [Header("UI References")]
    public GameObject buttonPrefab;    // Prefab with "CardImage" + "NumberImage"
    public Transform gridContainer;    // Parent with GridLayoutGroup
    public Image topBoxImage;          // The top target image
    public Text timerText;             // UI label for timer

    [Header("Grid Settings")]
    public int rows = 2;
    public int columns = 3;
    public Vector2 cardSize = new Vector2(200, 200);
    public Vector2 spacing = new Vector2(20, 20);
    public float showTime = 10f;

    [Header("Sprites")]
    public List<Sprite> imageList = new List<Sprite>();
    public List<Sprite> boxImageList = new List<Sprite>();
    public Sprite defaultBoxImage; // top default before 10 sec
    public List<Sprite> tempNumberSprites = new List<Sprite>(); // numbered 1–6 images

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

        var grid = gridContainer.GetComponent<GridLayoutGroup>();
        if (grid == null) grid = gridContainer.gameObject.AddComponent<GridLayoutGroup>();

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;
        grid.cellSize = cardSize;
        grid.spacing = spacing;
        grid.childAlignment = TextAnchor.MiddleCenter;

        StartCoroutine(StartNewRound());
    }

    IEnumerator StartNewRound()
    {
        canClick = false;

        // Clear previous cards
        foreach (Transform child in gridContainer)
            Destroy(child.gameObject);
        allButtons.Clear();
        assignedSprites.Clear();

        // Pick correct image (don’t show yet)
        if (boxImageList.Count == 0)
            boxImageList = imageList;
        correctSprite = boxImageList[Random.Range(0, boxImageList.Count)];

        topBoxImage.sprite = defaultBoxImage;

        // Choose 6 unique random sprites
        List<Sprite> shuffled = new List<Sprite>(imageList);
        Shuffle(shuffled);
        int totalCards = rows * columns;
        List<Sprite> chosenSprites = new List<Sprite>();
        for (int i = 0; i < totalCards && i < shuffled.Count; i++)
            chosenSprites.Add(shuffled[i]);

        // Ensure one matches top image
        int matchIndex = Random.Range(0, totalCards);
        chosenSprites[matchIndex] = correctSprite;

        // Create grid cards
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

            // Hide NumberImage for first 10s
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

            int index = i; // for listener
            Button btn = card.GetComponent<Button>();
            if (btn)
                btn.onClick.AddListener(() => OnCardClicked(index));

            allButtons.Add(card);
        }

        // Show memorization phase
        float timeLeft = showTime;
        while (timeLeft > 0)
        {
            timerText.text = $"Memorize: {Mathf.CeilToInt(timeLeft)}s";
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        // After 10 sec → hide CardImage, show NumberImage
        for (int i = 0; i < allButtons.Count; i++)
        {
            Transform cardImage = allButtons[i].transform.Find("CardImage");
            Transform numberImage = allButtons[i].transform.Find("NumberImage");

            if (cardImage)
                cardImage.gameObject.SetActive(false);

            if (numberImage)
                numberImage.gameObject.SetActive(true);
        }

        // Show correct top image now
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
            StartCoroutine(NextRoundDelay());
        }
        else
        {
            Debug.Log("❌ Wrong!");
            timerText.text = "❌ Wrong! Try again.";
        }
    }

    IEnumerator NextRoundDelay()
    {
        canClick = false;
        yield return new WaitForSeconds(1.5f);
        StartCoroutine(StartNewRound());
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