// using UnityEngine;
// using System.Collections.Generic;
// using UnityEngine.UI;
// using UnityEngine.EventSystems;
// using System.Collections;
// using DG.Tweening; // Requires DOTween asset

// public class FruitGridManager : MonoBehaviour
// {
//     [Header("Animation Settings")]
//     public float fruitPopDuration = 0.4f;
//     public float correctBounceScale = 1.2f;
//     public float wrongShakeStrength = 10f;
//     public GameObject correctParticlePrefab;
//     public GameObject wrongParticlePrefab;
//     public CanvasGroup targetCanvasGroup; // fade target name between rounds


//     [Header("Grid Settings")]
//     public int rows = 5;
//     public int columns = 4;
//     public Vector2 spacing = new Vector2(10f, 10f);
//     public RectTransform gridParent;
//     public GameObject fruitPrefab;

//     [Header("Fruits")]
//     public List<Sprite> fruitSprites;
//     public List<Sprite> selectedFruitSprites;
//     public List<Sprite> targetFruitNameSprites;

//     [Header("UI")]
//     public Image targetFruitImage;
//     [Range(0f, 1f)] public float backgroundMusicVolume = 0.6f;

//     [Header("Audio")]
//     public AudioClip backgroundMusic;
//     public AudioClip wrongSound;
//     public AudioClip successClip;

//     [Tooltip("List of correct SFX clips — one will be chosen randomly each time.")]
//     public List<AudioClip> correctSounds;

//     [Header("Loading Screen Settings")]
//     [Tooltip("Assign your loading screen prefab here.")]
//     public GameObject loadingScreenPrefab;
//     [Tooltip("Duration for the loading screen at game start.")]
//     public float initialLoadingDuration = 3f;

//     private GameObject loadingScreenInstance;
//     private List<FruitItem> fruits = new List<FruitItem>();
//     private int targetFruitID = -1;
//     private int lastTargetFruitID = -1;
//     private int totalFruits => fruitSprites?.Count ?? 0;


//     private int lastCorrectSoundIndex = -1;

//     void Start()
//     {
//         StartCoroutine(ShowInitialLoadingScreen());
//     }

//     IEnumerator ShowInitialLoadingScreen()
//     {
//         if (loadingScreenPrefab != null)
//         {
//             loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);
//             loadingScreenInstance.SetActive(true);
//         }

//         yield return new WaitForSeconds(initialLoadingDuration);

//         if (loadingScreenInstance != null)
//             loadingScreenInstance.SetActive(false);

//         // 🎵 Start background music immediately after first loading
//         if (AudioManager.Instance != null && backgroundMusic != null)
//         {
//             if (!AudioManager.Instance.IsMusicPlaying())
//             {
//                 AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
//                 AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
//                 // AudioManager.Instance.musicSource.DOFade(backgroundMusicVolume, 1.5f).From(0f);
//             }
//         }

//         InitializeGame();
//     }


//     void InitializeGame()
//     {
//         EnsureEventSystemAndRaycaster();
//         EnsureGridLayoutGroup();

//         // Background music will start after first full round completion

//         PickNewTarget();
//         CreateGrid();
//         AdjustGridCellSize(); // ✅ dynamically fit grid to screen

//         if (correctSounds == null || correctSounds.Count == 0)
//             Debug.LogWarning("[FruitGridManager] No correct sounds assigned! Please add some clips in Inspector.");

//     }

//     void EnsureEventSystemAndRaycaster()
//     {
//         if (FindFirstObjectByType<EventSystem>() == null)
//         {
//             new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
//             Debug.Log("[FruitGridManager] Created EventSystem automatically.");
//         }

//         Canvas parentCanvas = gridParent != null ? gridParent.GetComponentInParent<Canvas>() : null;
//         if (parentCanvas != null)
//         {
//             var gr = parentCanvas.GetComponent<GraphicRaycaster>();
//             if (gr == null)
//             {
//                 parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
//                 Debug.Log("[FruitGridManager] Added GraphicRaycaster to Canvas.");
//             }
//         }
//         else
//         {
//             Debug.LogWarning("[FruitGridManager] gridParent is not under a Canvas.");
//         }
//     }

//     void EnsureGridLayoutGroup()
//     {
//         if (gridParent == null)
//         {
//             Debug.LogError("[FruitGridManager] gridParent is not assigned.");
//             return;
//         }

//         var gl = gridParent.GetComponent<GridLayoutGroup>();
//         if (gl == null)
//         {
//             gl = gridParent.gameObject.AddComponent<GridLayoutGroup>();
//             Debug.Log("[FruitGridManager] Added GridLayoutGroup automatically.");
//         }

//         gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//         gl.constraintCount = columns;
//         gl.startAxis = GridLayoutGroup.Axis.Horizontal;
//         gl.childAlignment = TextAnchor.MiddleCenter;
//         gl.spacing = spacing;
//     }

//     void CreateGrid()
//     {
//         if (fruitPrefab == null || gridParent == null)
//         {
//             Debug.LogError("[FruitGridManager] Assign fruitPrefab and gridParent.");
//             return;
//         }

//         for (int i = gridParent.childCount - 1; i >= 0; i--)
//             DestroyImmediate(gridParent.GetChild(i).gameObject);
//         fruits.Clear();

//         int totalCells = rows * columns;

//         for (int i = 0; i < totalCells; i++)
//         {
//             GameObject obj = Instantiate(fruitPrefab, gridParent, false);
//             var fruit = obj.GetComponent<FruitItem>();
//             if (fruit == null)
//             {
//                 Debug.LogError("[FruitGridManager] fruitPrefab must have a FruitItem component.");
//                 Destroy(obj);
//                 continue;
//             }

//             if (fruit.fruitImage == null)
//                 fruit.fruitImage = obj.GetComponentInChildren<Image>();

//             int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
//             fruit.Init(this, fruitSprites[randomIndex], randomIndex);
//             fruits.Add(fruit);
//         }

//         ForceTargetFruitMultipleTimes();
//         Debug.Log($"[FruitGridManager] Created {fruits.Count} fruit cells.");
//         // ✨ Animate fruit pop-in
//         foreach (var fruit in fruits)
//         {
//             fruit.transform.localScale = Vector3.zero;
//             fruit.transform.DOScale(1f, fruitPopDuration)
//                 .SetEase(Ease.OutBack)
//                 .SetDelay(Random.Range(0f, 0.4f));
//         }
//     }

//     // void EnsureTargetFruitExists()
//     // {
//     //     if (targetFruitID < 0 || targetFruitID >= totalFruits || fruits.Count == 0)
//     //         return;

//     //     bool hasTarget = false;
//     //     foreach (var f in fruits)
//     //         if (f.fruitID == targetFruitID) { hasTarget = true; break; }

//     //     if (!hasTarget)
//     //     {
//     //         int replaceIndex = Random.Range(0, fruits.Count);
//     //         fruits[replaceIndex].Init(this, fruitSprites[targetFruitID], targetFruitID);
//     //         Debug.Log($"[FruitGridManager] Target fruit added at cell {replaceIndex}");
//     //     }
//     // }

//     void ForceTargetFruitMultipleTimes()
// {
//     if (fruits == null || fruits.Count == 0 || targetFruitID < 0)
//         return;

//     int minTarget = 3;
//     int maxTarget = 6;
//     int targetCount = Random.Range(minTarget, maxTarget + 1); // inclusive upper bound

//     // Ensure we don't exceed grid size
//     targetCount = Mathf.Min(targetCount, fruits.Count);

//     // Pick unique random indices
//     List<int> indices = new List<int>();
//     while (indices.Count < targetCount)
//     {
//         int randomIndex = Random.Range(0, fruits.Count);
//         if (!indices.Contains(randomIndex))
//             indices.Add(randomIndex);
//     }

//     // Replace selected cells with the target fruit
//     foreach (int idx in indices)
//     {
//         fruits[idx].Init(this, fruitSprites[targetFruitID], targetFruitID);
//     }

//     Debug.Log($"[FruitGridManager] Target fruit forced {targetCount} times this round.");
// }


//     void PickNewTarget()
//     {
//         if (totalFruits == 0)
//         {
//             Debug.LogError("[FruitGridManager] No fruit sprites assigned.");
//             return;
//         }

//         int newTarget;
//         do
//         {
//             newTarget = Random.Range(0, totalFruits);
//         }
//         while (newTarget == lastTargetFruitID && totalFruits > 1);

//         lastTargetFruitID = newTarget;
//         targetFruitID = newTarget;

//         if (targetFruitImage != null && targetFruitNameSprites.Count > targetFruitID)
//             targetFruitImage.sprite = targetFruitNameSprites[targetFruitID];

//         Debug.Log($"[FruitGridManager] New target ID = {targetFruitID}");
//         if (targetCanvasGroup != null)
//         {
//             targetCanvasGroup.alpha = 0f;
//             targetCanvasGroup.DOFade(1f, 0.6f).SetEase(Ease.OutQuad);
//             targetCanvasGroup.transform
//                 .DOScale(1.1f, 0.4f)
//                 .From(0.8f)
//                 .SetEase(Ease.OutBack);
//         }

//     }

//     public void OnFruitClicked(FruitItem clicked)
//     {
//         if (clicked == null) return;

//         Debug.Log($"[FruitGridManager] OnFruitClicked clickedID={clicked.fruitID} targetID={targetFruitID}");

//         if (clicked.fruitID == targetFruitID && !clicked.IsSelected())
//         {
//             if (AudioManager.Instance != null && correctSounds.Count > 0)
//             {
//                 // int randomIndex = Random.Range(0, correctSounds.Count);
//                 // AudioManager.Instance.PlaySFX(correctSounds[randomIndex]);
//                 int randomIndex = GetUniqueRandomCorrectSoundIndex();
//                 AudioManager.Instance.PlaySFX(correctSounds[randomIndex]);
//             }

//             Sprite selected = selectedFruitSprites.Count > targetFruitID
//                 ? selectedFruitSprites[targetFruitID]
//                 : null;
//             clicked.MarkAsSelected(selected);

//             // ✨ Animate correct selection
//             clicked.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 8, 0.5f);
//             if (correctParticlePrefab != null)
//                 Instantiate(correctParticlePrefab, clicked.transform.position, Quaternion.identity, gridParent);

//             // ✅ Only trigger round end when all target fruits are found
//             if (!HasUnselectedTargetFruits())
//                 StartCoroutine(HandleRoundCompletion());
//         }
//         else
//         {
//             if (AudioManager.Instance != null)
//                 AudioManager.Instance.PlaySFX(wrongSound);

//             // 🚫 Shake + red tint flash
//             clicked.transform.DOShakePosition(0.3f, wrongShakeStrength, 10, 90f, false, true);
//             if (wrongParticlePrefab != null)
//                 Instantiate(wrongParticlePrefab, clicked.transform.position, Quaternion.identity, gridParent);
//         }
//     }


//     int GetUniqueRandomCorrectSoundIndex()
//     {
//         if (correctSounds == null || correctSounds.Count == 0)
//             return 0;

//         int index;
//         do
//         {
//             index = Random.Range(0, correctSounds.Count);
//         }
//         while (index == lastCorrectSoundIndex && correctSounds.Count > 1);

//         lastCorrectSoundIndex = index;
//         return index;
//     }



//     IEnumerator HandleRoundCompletion()
//     {
//         // 🕐 Wait a moment so correct sound finishes or at least starts to fade
//         float delay = 0.3f;
//         if (correctSounds != null && correctSounds.Count > 0 && correctSounds[0] != null)
//             delay = Mathf.Min(correctSounds[0].length, 0.5f); // don’t wait too long

//         yield return new WaitForSeconds(delay);

//         // 🎉 Play success clip (kids like instant response)
//         if (AudioManager.Instance != null && successClip != null)
//             AudioManager.Instance.PlaySFX(successClip);

//         // ✅ Instantly show loading panel right when success clip plays
//         if (loadingScreenPrefab != null)
//         {
//             if (loadingScreenInstance == null)
//                 loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);

//             CanvasGroup cg = loadingScreenInstance.GetComponent<CanvasGroup>();
//             loadingScreenInstance.SetActive(true);

//             if (cg != null)
//             {
//                 cg.alpha = 0f;
//                 cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
//             }
//         }

//         // 🕐 Keep it on screen for a short, satisfying moment
//         yield return new WaitForSeconds(successClip != null ? successClip.length + 0.4f : 1.2f);

//         // ✅ Hide loading and move to next round
//         if (loadingScreenInstance != null)
//         {
//             CanvasGroup cg = loadingScreenInstance.GetComponent<CanvasGroup>();
//             if (cg != null)
//                 cg.DOFade(0f, 0.4f).OnComplete(() => loadingScreenInstance.SetActive(false));
//             else
//                 loadingScreenInstance.SetActive(false);
//         }

//         // 🎵 Restart background music if needed
//         if (AudioManager.Instance != null && backgroundMusic != null && !AudioManager.Instance.IsMusicPlaying())
//         {
//             AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
//             AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
//         }

//         NextRound();
//     }




//     IEnumerator ShowLoadingAfterCorrectClick()
//     {
//         if (loadingScreenPrefab != null)
//         {
//             if (loadingScreenInstance == null)
//                 loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);

//             loadingScreenInstance.SetActive(true);
//             yield return new WaitForSeconds(2f);
//             loadingScreenInstance.SetActive(false);
//         }
//     }

//     void NextRound()
//     {
//         StartCoroutine(TransitionToNextRound());
//     }

//     IEnumerator TransitionToNextRound()
//     {
//         // Fade out target and fruits
//         if (targetCanvasGroup != null)
//             targetCanvasGroup.DOFade(0f, 0.4f);

//         foreach (var fruit in fruits)
//             fruit.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack);

//         yield return new WaitForSeconds(0.5f);

//         // Shuffle and new target
//         PickNewTarget();
//         ShuffleGrid();
//         ForceTargetFruitMultipleTimes();

//         // Pop-in new fruits
//         foreach (var fruit in fruits)
//         {
//             fruit.transform.localScale = Vector3.zero;
//             fruit.transform.DOScale(1f, 0.4f)
//                 .SetEase(Ease.OutBack)
//                 .SetDelay(Random.Range(0f, 0.3f));
//         }

//         if (targetCanvasGroup != null)
//             targetCanvasGroup.DOFade(1f, 0.4f);

//         if (correctParticlePrefab != null)
//         {
//             var confetti = Instantiate(correctParticlePrefab, targetFruitImage.transform.position, Quaternion.identity, transform);
//             Destroy(confetti, 2f);
//         }

//     }

//     bool HasUnselectedTargetFruits()
//     {
//         foreach (var f in fruits)
//             if (f.fruitID == targetFruitID && !f.IsSelected())
//                 return true;
//         return false;
//     }

//     void ShuffleGrid()
//     {
//         for (int i = 0; i < fruits.Count; i++)
//         {
//             int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
//             fruits[i].Init(this, fruitSprites[randomIndex], randomIndex);
//         }
//     }

//     public void SetMusicVolume(float value)
//     {
//         backgroundMusicVolume = Mathf.Clamp01(value);
//         if (AudioManager.Instance != null)
//             AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
//     }

//     // 🧩 NEW: Automatically resize grid cells to fit screen
//     void AdjustGridCellSize()
//     {
//         var gl = gridParent.GetComponent<GridLayoutGroup>();
//         if (gl == null) return;

//         // Use RectTransform dimensions for sizing
//         float parentWidth = gridParent.rect.width;
//         float parentHeight = gridParent.rect.height;

//         float totalSpacingX = (columns - 1) * gl.spacing.x;
//         float totalSpacingY = (rows - 1) * gl.spacing.y;

//         float cellWidth = (parentWidth - totalSpacingX - gl.padding.left - gl.padding.right) / columns;
//         float cellHeight = (parentHeight - totalSpacingY - gl.padding.top - gl.padding.bottom) / rows;

//         // Ensure cell size is positive
//         cellWidth = Mathf.Max(0, cellWidth);
//         cellHeight = Mathf.Max(0, cellHeight);
        
//         gl.cellSize = new Vector2(cellWidth, cellHeight);

//         Debug.Log($"[FruitGridManager] Adjusted cell size to {gl.cellSize}");
//     }

//     // ✅ Update layout automatically on resolution change (often called on RectTransform hierarchy changes too)
//     void OnRectTransformDimensionsChange()
//     {
//         AdjustGridCellSize();
//     }
// }


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
    public AudioClip wrongSound;
    public AudioClip successClip;

    [Tooltip("List of correct SFX clips — one will be chosen randomly each time.")]
    public List<AudioClip> correctSounds;

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


    private int lastCorrectSoundIndex = -1;

    [Header("Extra Visual Effects")]
    public GameObject glowRingPrefab;
    public GameObject starTrailPrefab;

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

        if (correctSounds == null || correctSounds.Count == 0)
            Debug.LogWarning("[FruitGridManager] No correct sounds assigned! Please add some clips in Inspector.");

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

        ForceTargetFruitMultipleTimes();
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

    // void EnsureTargetFruitExists()
    // {
    //     if (targetFruitID < 0 || targetFruitID >= totalFruits || fruits.Count == 0)
    //         return;

    //     bool hasTarget = false;
    //     foreach (var f in fruits)
    //         if (f.fruitID == targetFruitID) { hasTarget = true; break; }

    //     if (!hasTarget)
    //     {
    //         int replaceIndex = Random.Range(0, fruits.Count);
    //         fruits[replaceIndex].Init(this, fruitSprites[targetFruitID], targetFruitID);
    //         Debug.Log($"[FruitGridManager] Target fruit added at cell {replaceIndex}");
    //     }
    // }

    void ForceTargetFruitMultipleTimes()
{
    if (fruits == null || fruits.Count == 0 || targetFruitID < 0)
        return;

    int minTarget = 3;
    int maxTarget = 6;
    int targetCount = Random.Range(minTarget, maxTarget + 1); // inclusive upper bound

    // Ensure we don't exceed grid size
    targetCount = Mathf.Min(targetCount, fruits.Count);

    // Pick unique random indices
    List<int> indices = new List<int>();
    while (indices.Count < targetCount)
    {
        int randomIndex = Random.Range(0, fruits.Count);
        if (!indices.Contains(randomIndex))
            indices.Add(randomIndex);
    }

    // Replace selected cells with the target fruit
    foreach (int idx in indices)
    {
        fruits[idx].Init(this, fruitSprites[targetFruitID], targetFruitID);
    }

    Debug.Log($"[FruitGridManager] Target fruit forced {targetCount} times this round.");
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
            if (AudioManager.Instance != null && correctSounds.Count > 0)
            {
                // int randomIndex = Random.Range(0, correctSounds.Count);
                // AudioManager.Instance.PlaySFX(correctSounds[randomIndex]);
                int randomIndex = GetUniqueRandomCorrectSoundIndex();
                AudioManager.Instance.PlaySFX(correctSounds[randomIndex]);
            }

            Sprite selected = selectedFruitSprites.Count > targetFruitID
                ? selectedFruitSprites[targetFruitID]
                : null;
            clicked.MarkAsSelected(selected);

            // ✨ Animate correct selection
            // clicked.transform.DOPunchScale(Vector3.one * 0.3f, 0.4f, 8, 0.5f);
            // if (correctParticlePrefab != null)
            //     Instantiate(correctParticlePrefab, clicked.transform.position, Quaternion.identity, gridParent);

            // ✨ Animate correct selection — juicy version
            clicked.transform.DOKill();
            clicked.transform.DOPunchScale(Vector3.one * 0.25f, 0.4f, 10, 0.7f);
            clicked.transform.DOShakeRotation(0.3f, 10f, 8, 80f, false);

            if (correctParticlePrefab != null)
            {
                // 🌟 Main burst
                Instantiate(correctParticlePrefab, clicked.transform.position, Quaternion.identity, gridParent);
            }

            // 🌈 Add layered effects (if prefabs assigned)
            if (correctParticlePrefab != null)
            {
                Vector3 pos = clicked.transform.position;

                // Optional: glow ring
                if (glowRingPrefab != null)
                    Instantiate(glowRingPrefab, pos, Quaternion.identity, gridParent);

                // Optional: trail sparkles
                if (starTrailPrefab != null)
                    Instantiate(starTrailPrefab, pos, Quaternion.identity, gridParent);
            }

            // 🎶 Light bounce of entire grid (fun feedback)
            gridParent.DOShakePosition(0.25f, 15f, 8, 90, false, true);


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


    int GetUniqueRandomCorrectSoundIndex()
    {
        if (correctSounds == null || correctSounds.Count == 0)
            return 0;

        int index;
        do
        {
            index = Random.Range(0, correctSounds.Count);
        }
        while (index == lastCorrectSoundIndex && correctSounds.Count > 1);

        lastCorrectSoundIndex = index;
        return index;
    }



    IEnumerator HandleRoundCompletion()
    {
        // 🕐 Wait a moment so correct sound finishes or at least starts to fade
        float delay = 0.3f;
        if (correctSounds != null && correctSounds.Count > 0 && correctSounds[0] != null)
            delay = Mathf.Min(correctSounds[0].length, 0.5f); // don’t wait too long

        yield return new WaitForSeconds(delay);

        // 🎉 Play success clip (kids like instant response)
        if (AudioManager.Instance != null && successClip != null)
            AudioManager.Instance.PlaySFX(successClip);

        // ✅ Instantly show loading panel right when success clip plays
        if (loadingScreenPrefab != null)
        {
            if (loadingScreenInstance == null)
                loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);

            CanvasGroup cg = loadingScreenInstance.GetComponent<CanvasGroup>();
            loadingScreenInstance.SetActive(true);

            if (cg != null)
            {
                cg.alpha = 0f;
                cg.DOFade(1f, 0.4f).SetEase(Ease.OutQuad);
            }
        }

        // 🕐 Keep it on screen for a short, satisfying moment
        yield return new WaitForSeconds(successClip != null ? successClip.length + 0.4f : 1.2f);

        // ✅ Hide loading and move to next round
        if (loadingScreenInstance != null)
        {
            CanvasGroup cg = loadingScreenInstance.GetComponent<CanvasGroup>();
            if (cg != null)
                cg.DOFade(0f, 0.4f).OnComplete(() => loadingScreenInstance.SetActive(false));
            else
                loadingScreenInstance.SetActive(false);
        }

        // 🎵 Restart background music if needed
        if (AudioManager.Instance != null && backgroundMusic != null && !AudioManager.Instance.IsMusicPlaying())
        {
            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
            AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
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
        ForceTargetFruitMultipleTimes();

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