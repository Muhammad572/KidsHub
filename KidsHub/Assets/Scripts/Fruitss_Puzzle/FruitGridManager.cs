using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening; // Requires DOTween asset

public class FruitGridManager : MonoBehaviour
{
    [Header("Animation Settings")]
    public float fruitPopDuration = 0.4f;
    public float correctBounceScale = 1.2f;
    public float wrongShakeStrength = 10f;
    public GameObject correctParticlePrefab;
    public GameObject wrongParticlePrefab;
    public CanvasGroup targetCanvasGroup; // fade target name between rounds


    [Header("Grid Settings")]
    public int rows = 5;
    public int columns = 4;
    public Vector2 spacing = new Vector2(10f, 10f);
    public RectTransform gridParent;
    public GameObject fruitPrefab;

    [Header("Fruits")]
    public List<Sprite> fruitSprites;
    public List<Sprite> selectedFruitSprites;
    public List<Sprite> targetFruitNameSprites;

    [Header("UI")]
    public Image targetFruitImage;
    [Range(0f, 1f)] public float backgroundMusicVolume = 0.6f;

    [Header("Audio")]
    public AudioClip backgroundMusic;
    public AudioClip correctSound;
    public AudioClip wrongSound;

    [Header("Loading Screen Settings")]
    [Tooltip("Assign your loading screen prefab here.")]
    public GameObject loadingScreenPrefab;
    [Tooltip("Duration for the loading screen at game start.")]
    public float initialLoadingDuration = 3f;

    private GameObject loadingScreenInstance;
    private List<FruitItem> fruits = new List<FruitItem>();
    private int targetFruitID = -1;
    private int lastTargetFruitID = -1;
    private int totalFruits => fruitSprites?.Count ?? 0;

    void Start()
    {
        StartCoroutine(ShowInitialLoadingScreen());
    }

    IEnumerator ShowInitialLoadingScreen()
    {
        if (loadingScreenPrefab != null)
        {
            loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);
            loadingScreenInstance.SetActive(true);
        }

        yield return new WaitForSeconds(initialLoadingDuration);

        if (loadingScreenInstance != null)
            loadingScreenInstance.SetActive(false);

        // 🎵 Start background music immediately after first loading
        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            if (!AudioManager.Instance.IsMusicPlaying())
            {
                AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
                AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
                // AudioManager.Instance.musicSource.DOFade(backgroundMusicVolume, 1.5f).From(0f);
            }
        }

        InitializeGame();
    }


    void InitializeGame()
    {
        EnsureEventSystemAndRaycaster();
        EnsureGridLayoutGroup();

        // Background music will start after first full round completion

        PickNewTarget();
        CreateGrid();
        AdjustGridCellSize(); // ✅ dynamically fit grid to screen
    }

    void EnsureEventSystemAndRaycaster()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Debug.Log("[FruitGridManager] Created EventSystem automatically.");
        }

        Canvas parentCanvas = gridParent != null ? gridParent.GetComponentInParent<Canvas>() : null;
        if (parentCanvas != null)
        {
            var gr = parentCanvas.GetComponent<GraphicRaycaster>();
            if (gr == null)
            {
                parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
                Debug.Log("[FruitGridManager] Added GraphicRaycaster to Canvas.");
            }
        }
        else
        {
            Debug.LogWarning("[FruitGridManager] gridParent is not under a Canvas.");
        }
    }

    void EnsureGridLayoutGroup()
    {
        if (gridParent == null)
        {
            Debug.LogError("[FruitGridManager] gridParent is not assigned.");
            return;
        }

        var gl = gridParent.GetComponent<GridLayoutGroup>();
        if (gl == null)
        {
            gl = gridParent.gameObject.AddComponent<GridLayoutGroup>();
            Debug.Log("[FruitGridManager] Added GridLayoutGroup automatically.");
        }

        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gl.constraintCount = columns;
        gl.startAxis = GridLayoutGroup.Axis.Horizontal;
        gl.childAlignment = TextAnchor.MiddleCenter;
        gl.spacing = spacing;
    }

    void CreateGrid()
    {
        if (fruitPrefab == null || gridParent == null)
        {
            Debug.LogError("[FruitGridManager] Assign fruitPrefab and gridParent.");
            return;
        }

        for (int i = gridParent.childCount - 1; i >= 0; i--)
            DestroyImmediate(gridParent.GetChild(i).gameObject);
        fruits.Clear();

        int totalCells = rows * columns;

        for (int i = 0; i < totalCells; i++)
        {
            GameObject obj = Instantiate(fruitPrefab, gridParent, false);
            var fruit = obj.GetComponent<FruitItem>();
            if (fruit == null)
            {
                Debug.LogError("[FruitGridManager] fruitPrefab must have a FruitItem component.");
                Destroy(obj);
                continue;
            }

            if (fruit.fruitImage == null)
                fruit.fruitImage = obj.GetComponentInChildren<Image>();

            int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
            fruit.Init(this, fruitSprites[randomIndex], randomIndex);
            fruits.Add(fruit);
        }

        EnsureTargetFruitExists();
        Debug.Log($"[FruitGridManager] Created {fruits.Count} fruit cells.");
        // ✨ Animate fruit pop-in
        foreach (var fruit in fruits)
        {
            fruit.transform.localScale = Vector3.zero;
            fruit.transform.DOScale(1f, fruitPopDuration)
                .SetEase(Ease.OutBack)
                .SetDelay(Random.Range(0f, 0.4f));
        }
    }

    void EnsureTargetFruitExists()
    {
        if (targetFruitID < 0 || targetFruitID >= totalFruits || fruits.Count == 0)
            return;

        bool hasTarget = false;
        foreach (var f in fruits)
            if (f.fruitID == targetFruitID) { hasTarget = true; break; }

        if (!hasTarget)
        {
            int replaceIndex = Random.Range(0, fruits.Count);
            fruits[replaceIndex].Init(this, fruitSprites[targetFruitID], targetFruitID);
            Debug.Log($"[FruitGridManager] Target fruit added at cell {replaceIndex}");
        }
    }

    void PickNewTarget()
    {
        if (totalFruits == 0)
        {
            Debug.LogError("[FruitGridManager] No fruit sprites assigned.");
            return;
        }

        int newTarget;
        do
        {
            newTarget = Random.Range(0, totalFruits);
        }
        while (newTarget == lastTargetFruitID && totalFruits > 1);

        lastTargetFruitID = newTarget;
        targetFruitID = newTarget;

        if (targetFruitImage != null && targetFruitNameSprites.Count > targetFruitID)
            targetFruitImage.sprite = targetFruitNameSprites[targetFruitID];

        Debug.Log($"[FruitGridManager] New target ID = {targetFruitID}");
        if (targetCanvasGroup != null)
        {
            targetCanvasGroup.alpha = 0f;
            targetCanvasGroup.DOFade(1f, 0.6f).SetEase(Ease.OutQuad);
            targetCanvasGroup.transform
                .DOScale(1.1f, 0.4f)
                .From(0.8f)
                .SetEase(Ease.OutBack);
        }

    }

    public void OnFruitClicked(FruitItem clicked)
    {
        if (clicked == null) return;

        Debug.Log($"[FruitGridManager] OnFruitClicked clickedID={clicked.fruitID} targetID={targetFruitID}");

        if (clicked.fruitID == targetFruitID && !clicked.IsSelected())
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(correctSound);

            Sprite selected = selectedFruitSprites.Count > targetFruitID
                ? selectedFruitSprites[targetFruitID]
                : null;
            clicked.MarkAsSelected(selected);

            // ✨ Animate correct selection
            clicked.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 8, 0.5f);
            if (correctParticlePrefab != null)
                Instantiate(correctParticlePrefab, clicked.transform.position, Quaternion.identity, gridParent);

            // ✅ Only trigger round end when all target fruits are found
            if (!HasUnselectedTargetFruits())
                StartCoroutine(HandleRoundCompletion());
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(wrongSound);

            // 🚫 Shake + red tint flash
            clicked.transform.DOShakePosition(0.3f, wrongShakeStrength, 10, 90f, false, true);
            if (wrongParticlePrefab != null)
                Instantiate(wrongParticlePrefab, clicked.transform.position, Quaternion.identity, gridParent);
        }
    }

    IEnumerator HandleRoundCompletion()
    {
        // 🕐 Small pause before showing loading screen (let animations play)
        yield return new WaitForSeconds(1.5f);

        // ✅ Show loading screen
        if (loadingScreenPrefab != null)
        {
            if (loadingScreenInstance == null)
                loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);

            // Optional: start invisible and fade in
            CanvasGroup cg = loadingScreenInstance.GetComponent<CanvasGroup>();
            loadingScreenInstance.SetActive(true);

            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.6f).SetEase(Ease.OutQuad);
            }
        }

        // 🕐 Show loading for a moment
        yield return new WaitForSeconds(2f);

        // 🎵 Start background music (first time only)
        // NOTE: This logic is redundant since music is started in ShowInitialLoadingScreen,
        // but kept for robustness if initial loading is skipped.
        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            if (!AudioManager.Instance.IsMusicPlaying())
            {
                AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
                AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
            }
        }

        // ✅ Hide loading and move to next round
        if (loadingScreenInstance != null)
        {
            CanvasGroup cg = loadingScreenInstance.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(0f, 0.5f).OnComplete(() => loadingScreenInstance.SetActive(false));
            else
                loadingScreenInstance.SetActive(false);
        }

        NextRound();
    }


    IEnumerator ShowLoadingAfterCorrectClick()
    {
        if (loadingScreenPrefab != null)
        {
            if (loadingScreenInstance == null)
                loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);

            loadingScreenInstance.SetActive(true);
            yield return new WaitForSeconds(2f);
            loadingScreenInstance.SetActive(false);
        }
    }

    void NextRound()
    {
        StartCoroutine(TransitionToNextRound());
    }

    IEnumerator TransitionToNextRound()
    {
        // Fade out target and fruits
        if (targetCanvasGroup != null)
            targetCanvasGroup.DOFade(0f, 0.4f);

        foreach (var fruit in fruits)
            fruit.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);

        yield return new WaitForSeconds(0.5f);

        // Shuffle and new target
        PickNewTarget();
        ShuffleGrid();
        EnsureTargetFruitExists();

        // Pop-in new fruits
        foreach (var fruit in fruits)
        {
            fruit.transform.localScale = Vector3.zero;
            fruit.transform.DOScale(1f, 0.4f)
                .SetEase(Ease.OutBack)
                .SetDelay(Random.Range(0f, 0.3f));
        }

        if (targetCanvasGroup != null)
            targetCanvasGroup.DOFade(1f, 0.4f);

        if (correctParticlePrefab != null)
        {
            var confetti = Instantiate(correctParticlePrefab, targetFruitImage.transform.position, Quaternion.identity, transform);
            Destroy(confetti, 2f);
        }

    }

    bool HasUnselectedTargetFruits()
    {
        foreach (var f in fruits)
            if (f.fruitID == targetFruitID && !f.IsSelected())
                return true;
        return false;
    }

    void ShuffleGrid()
    {
        for (int i = 0; i < fruits.Count; i++)
        {
            int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
            fruits[i].Init(this, fruitSprites[randomIndex], randomIndex);
        }
    }

    public void SetMusicVolume(float value)
    {
        backgroundMusicVolume = Mathf.Clamp01(value);
        if (AudioManager.Instance != null)
            AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
    }

    // 🧩 NEW: Automatically resize grid cells to fit screen
    void AdjustGridCellSize()
    {
        var gl = gridParent.GetComponent<GridLayoutGroup>();
        if (gl == null) return;

        // Use RectTransform dimensions for sizing
        float parentWidth = gridParent.rect.width;
        float parentHeight = gridParent.rect.height;

        float totalSpacingX = (columns - 1) * gl.spacing.x;
        float totalSpacingY = (rows - 1) * gl.spacing.y;

        float cellWidth = (parentWidth - totalSpacingX - gl.padding.left - gl.padding.right) / columns;
        float cellHeight = (parentHeight - totalSpacingY - gl.padding.top - gl.padding.bottom) / rows;

        // Ensure cell size is positive
        cellWidth = Mathf.Max(0, cellWidth);
        cellHeight = Mathf.Max(0, cellHeight);
        
        gl.cellSize = new Vector2(cellWidth, cellHeight);

        Debug.Log($"[FruitGridManager] Adjusted cell size to {gl.cellSize}");
    }

    // ✅ Update layout automatically on resolution change (often called on RectTransform hierarchy changes too)
    void OnRectTransformDimensionsChange()
    {
        AdjustGridCellSize();
    }
}