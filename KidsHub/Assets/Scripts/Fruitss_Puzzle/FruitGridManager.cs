//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;

//public class FruitGridManager : MonoBehaviour
//{
//    [Header("Grid Settings")]
//    public int rows = 5;
//    public int columns = 4;
//    public float spacing = 100f;
//    public RectTransform gridParent;
//    public GameObject fruitPrefab;

//    [Header("Fruits")]
//    public List<Sprite> fruitSprites;
//    public List<Sprite> selectedFruitSprites;
//    public List<Sprite> targetFruitNameSprites;

//    [Header("UI")]
//    public Image targetFruitImage;
//    [Range(0f, 1f)] public float backgroundMusicVolume = 0.6f; // 👈 Volume control (0–1)

//    [Header("Audio")]
//    public AudioClip backgroundMusic;
//    public AudioClip correctSound;
//    public AudioClip wrongSound;

//    private List<FruitItem> fruits = new List<FruitItem>();
//    private int targetFruitID = -1;
//    private int lastTargetFruitID = -1;
//    private int totalFruits => fruitSprites?.Count ?? 0;

//    void Start()
//    {
//        EnsureEventSystemAndRaycaster();
//        EnsureGridLayoutGroup();

//        if (AudioManager.Instance != null && backgroundMusic != null)
//        {
//            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
//            AudioManager.Instance.SetMusicVolume(backgroundMusicVolume); // ✅ fixed
//        }

//        PickNewTarget();
//        CreateGrid();
//    }

//    void EnsureEventSystemAndRaycaster()
//    {
//        if (FindFirstObjectByType<EventSystem>() == null)
//        {
//            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
//            Debug.Log("[FruitGridManager] Created EventSystem automatically.");
//        }

//        Canvas parentCanvas = gridParent != null ? gridParent.GetComponentInParent<Canvas>() : null;
//        if (parentCanvas != null)
//        {
//            var gr = parentCanvas.GetComponent<GraphicRaycaster>();
//            if (gr == null)
//            {
//                parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
//                Debug.Log("[FruitGridManager] Added GraphicRaycaster to Canvas.");
//            }
//        }
//        else
//        {
//            Debug.LogWarning("[FruitGridManager] gridParent is not under a Canvas.");
//        }
//    }

//    void EnsureGridLayoutGroup()
//    {
//        if (gridParent == null)
//        {
//            Debug.LogError("[FruitGridManager] gridParent is not assigned.");
//            return;
//        }

//        var gl = gridParent.GetComponent<GridLayoutGroup>();
//        if (gl == null)
//        {
//            gl = gridParent.gameObject.AddComponent<GridLayoutGroup>();
//            Debug.Log("[FruitGridManager] Added GridLayoutGroup automatically.");
//        }

//        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//        gl.constraintCount = columns;
//        gl.startAxis = GridLayoutGroup.Axis.Horizontal;
//        gl.childAlignment = TextAnchor.MiddleCenter;
//    }

//    void CreateGrid()
//    {
//        if (fruitPrefab == null || gridParent == null)
//        {
//            Debug.LogError("[FruitGridManager] Assign fruitPrefab and gridParent.");
//            return;
//        }

//        // Remove old children
//        for (int i = gridParent.childCount - 1; i >= 0; i--)
//            DestroyImmediate(gridParent.GetChild(i).gameObject);
//        fruits.Clear();

//        int totalCells = rows * columns;

//        for (int i = 0; i < totalCells; i++)
//        {
//            GameObject obj = Instantiate(fruitPrefab, gridParent, false);
//            var fruit = obj.GetComponent<FruitItem>();
//            if (fruit == null)
//            {
//                Debug.LogError("[FruitGridManager] fruitPrefab must have a FruitItem component.");
//                Destroy(obj);
//                continue;
//            }

//            if (fruit.fruitImage == null)
//                fruit.fruitImage = obj.GetComponentInChildren<Image>();

//            int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
//            fruit.Init(this, fruitSprites[randomIndex], randomIndex);
//            fruits.Add(fruit);
//        }

//        EnsureTargetFruitExists();
//        Debug.Log($"[FruitGridManager] Created {fruits.Count} fruit cells.");
//    }

//    void EnsureTargetFruitExists()
//    {
//        if (targetFruitID < 0 || targetFruitID >= totalFruits || fruits.Count == 0)
//            return;

//        bool hasTarget = false;
//        foreach (var f in fruits)
//            if (f.fruitID == targetFruitID) { hasTarget = true; break; }

//        if (!hasTarget)
//        {
//            int replaceIndex = Random.Range(0, fruits.Count);
//            fruits[replaceIndex].Init(this, fruitSprites[targetFruitID], targetFruitID);
//            Debug.Log($"[FruitGridManager] Target fruit added at cell {replaceIndex}");
//        }
//    }

//    void PickNewTarget()
//    {
//        if (totalFruits == 0)
//        {
//            Debug.LogError("[FruitGridManager] No fruit sprites assigned.");
//            return;
//        }

//        int newTarget;
//        do
//        {
//            newTarget = Random.Range(0, totalFruits);
//        }
//        while (newTarget == lastTargetFruitID && totalFruits > 1);

//        lastTargetFruitID = newTarget;
//        targetFruitID = newTarget;

//        if (targetFruitImage != null && targetFruitNameSprites.Count > targetFruitID)
//            targetFruitImage.sprite = targetFruitNameSprites[targetFruitID];

//        Debug.Log($"[FruitGridManager] New target ID = {targetFruitID}");
//    }

//    public void OnFruitClicked(FruitItem clicked)
//    {
//        if (clicked == null) return;
//        Debug.Log($"[FruitGridManager] OnFruitClicked clickedID={clicked.fruitID} targetID={targetFruitID}");

//        if (clicked.fruitID == targetFruitID && !clicked.IsSelected())
//        {
//            if (AudioManager.Instance != null)
//                AudioManager.Instance.PlaySFX(correctSound);

//            Sprite selected = selectedFruitSprites.Count > targetFruitID
//                ? selectedFruitSprites[targetFruitID]
//                : null;
//            clicked.MarkAsSelected(selected);

//            if (!HasUnselectedTargetFruits())
//                Invoke(nameof(NextRound), 0.6f);
//        }
//        else
//        {
//            if (AudioManager.Instance != null)
//                AudioManager.Instance.PlaySFX(wrongSound);
//        }
//    }

//    void NextRound()
//    {
//        PickNewTarget();
//        ShuffleGrid();
//        EnsureTargetFruitExists();
//    }

//    bool HasUnselectedTargetFruits()
//    {
//        foreach (var f in fruits)
//            if (f.fruitID == targetFruitID && !f.IsSelected())
//                return true;
//        return false;
//    }

//    void ShuffleGrid()
//    {
//        for (int i = 0; i < fruits.Count; i++)
//        {
//            int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
//            fruits[i].Init(this, fruitSprites[randomIndex], randomIndex);
//        }
//    }

//    // 🔊 Optional live volume adjustment
//    public void SetMusicVolume(float value)
//    {
//        backgroundMusicVolume = Mathf.Clamp01(value);
//        if (AudioManager.Instance != null)
//            AudioManager.Instance.SetMusicVolume(backgroundMusicVolume); // ✅ fixed
//    }
//}













//using UnityEngine;
//using System.Collections.Generic;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections;

//public class FruitGridManager : MonoBehaviour
//{
//    [Header("Grid Settings")]
//    public int rows = 5;
//    public int columns = 4;
//    public float spacing = 100f;
//    public RectTransform gridParent;
//    public GameObject fruitPrefab;

//    [Header("Fruits")]
//    public List<Sprite> fruitSprites;
//    public List<Sprite> selectedFruitSprites;
//    public List<Sprite> targetFruitNameSprites;

//    [Header("UI")]
//    public Image targetFruitImage;
//    [Range(0f, 1f)] public float backgroundMusicVolume = 0.6f;

//    [Header("Audio")]
//    public AudioClip backgroundMusic;
//    public AudioClip correctSound;
//    public AudioClip wrongSound;

//    [Header("Loading Screen Settings")]
//    [Tooltip("Assign your loading screen prefab here.")]
//    public GameObject loadingScreenPrefab;
//    [Tooltip("Duration for the loading screen at game start.")]
//    public float initialLoadingDuration = 3f;

//    private GameObject loadingScreenInstance;
//    private List<FruitItem> fruits = new List<FruitItem>();
//    private int targetFruitID = -1;
//    private int lastTargetFruitID = -1;
//    private int totalFruits => fruitSprites?.Count ?? 0;

//    void Start()
//    {
//        StartCoroutine(ShowInitialLoadingScreen());
//    }

//    IEnumerator ShowInitialLoadingScreen()
//    {
//        if (loadingScreenPrefab != null)
//        {
//            loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);
//            loadingScreenInstance.SetActive(true);
//        }

//        yield return new WaitForSeconds(initialLoadingDuration);

//        if (loadingScreenInstance != null)
//            loadingScreenInstance.SetActive(false);

//        InitializeGame();
//    }

//    void InitializeGame()
//    {
//        EnsureEventSystemAndRaycaster();
//        EnsureGridLayoutGroup();

//        if (AudioManager.Instance != null && backgroundMusic != null)
//        {
//            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
//            AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
//        }

//        PickNewTarget();
//        CreateGrid();
//    }

//    void EnsureEventSystemAndRaycaster()
//    {
//        if (FindFirstObjectByType<EventSystem>() == null)
//        {
//            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
//            Debug.Log("[FruitGridManager] Created EventSystem automatically.");
//        }

//        Canvas parentCanvas = gridParent != null ? gridParent.GetComponentInParent<Canvas>() : null;
//        if (parentCanvas != null)
//        {
//            var gr = parentCanvas.GetComponent<GraphicRaycaster>();
//            if (gr == null)
//            {
//                parentCanvas.gameObject.AddComponent<GraphicRaycaster>();
//                Debug.Log("[FruitGridManager] Added GraphicRaycaster to Canvas.");
//            }
//        }
//        else
//        {
//            Debug.LogWarning("[FruitGridManager] gridParent is not under a Canvas.");
//        }
//    }

//    void EnsureGridLayoutGroup()
//    {
//        if (gridParent == null)
//        {
//            Debug.LogError("[FruitGridManager] gridParent is not assigned.");
//            return;
//        }

//        var gl = gridParent.GetComponent<GridLayoutGroup>();
//        if (gl == null)
//        {
//            gl = gridParent.gameObject.AddComponent<GridLayoutGroup>();
//            Debug.Log("[FruitGridManager] Added GridLayoutGroup automatically.");
//        }

//        gl.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
//        gl.constraintCount = columns;
//        gl.startAxis = GridLayoutGroup.Axis.Horizontal;
//        gl.childAlignment = TextAnchor.MiddleCenter;
//    }

//    void CreateGrid()
//    {
//        if (fruitPrefab == null || gridParent == null)
//        {
//            Debug.LogError("[FruitGridManager] Assign fruitPrefab and gridParent.");
//            return;
//        }

//        for (int i = gridParent.childCount - 1; i >= 0; i--)
//            DestroyImmediate(gridParent.GetChild(i).gameObject);
//        fruits.Clear();

//        int totalCells = rows * columns;

//        for (int i = 0; i < totalCells; i++)
//        {
//            GameObject obj = Instantiate(fruitPrefab, gridParent, false);
//            var fruit = obj.GetComponent<FruitItem>();
//            if (fruit == null)
//            {
//                Debug.LogError("[FruitGridManager] fruitPrefab must have a FruitItem component.");
//                Destroy(obj);
//                continue;
//            }

//            if (fruit.fruitImage == null)
//                fruit.fruitImage = obj.GetComponentInChildren<Image>();

//            int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
//            fruit.Init(this, fruitSprites[randomIndex], randomIndex);
//            fruits.Add(fruit);
//        }

//        EnsureTargetFruitExists();
//        Debug.Log($"[FruitGridManager] Created {fruits.Count} fruit cells.");
//    }

//    void EnsureTargetFruitExists()
//    {
//        if (targetFruitID < 0 || targetFruitID >= totalFruits || fruits.Count == 0)
//            return;

//        bool hasTarget = false;
//        foreach (var f in fruits)
//            if (f.fruitID == targetFruitID) { hasTarget = true; break; }

//        if (!hasTarget)
//        {
//            int replaceIndex = Random.Range(0, fruits.Count);
//            fruits[replaceIndex].Init(this, fruitSprites[targetFruitID], targetFruitID);
//            Debug.Log($"[FruitGridManager] Target fruit added at cell {replaceIndex}");
//        }
//    }

//    void PickNewTarget()
//    {
//        if (totalFruits == 0)
//        {
//            Debug.LogError("[FruitGridManager] No fruit sprites assigned.");
//            return;
//        }

//        int newTarget;
//        do
//        {
//            newTarget = Random.Range(0, totalFruits);
//        }
//        while (newTarget == lastTargetFruitID && totalFruits > 1);

//        lastTargetFruitID = newTarget;
//        targetFruitID = newTarget;

//        if (targetFruitImage != null && targetFruitNameSprites.Count > targetFruitID)
//            targetFruitImage.sprite = targetFruitNameSprites[targetFruitID];

//        Debug.Log($"[FruitGridManager] New target ID = {targetFruitID}");
//    }

//    public void OnFruitClicked(FruitItem clicked)
//    {
//        if (clicked == null) return;
//        Debug.Log($"[FruitGridManager] OnFruitClicked clickedID={clicked.fruitID} targetID={targetFruitID}");

//        if (clicked.fruitID == targetFruitID && !clicked.IsSelected())
//        {
//            if (AudioManager.Instance != null)
//                AudioManager.Instance.PlaySFX(correctSound);

//            Sprite selected = selectedFruitSprites.Count > targetFruitID
//                ? selectedFruitSprites[targetFruitID]
//                : null;
//            clicked.MarkAsSelected(selected);

//            // ✅ Show loading screen after correct click
//            StartCoroutine(ShowLoadingAfterCorrectClick());

//            if (!HasUnselectedTargetFruits())
//                Invoke(nameof(NextRound), 0.6f);
//        }
//        else
//        {
//            if (AudioManager.Instance != null)
//                AudioManager.Instance.PlaySFX(wrongSound);
//        }
//    }

//    IEnumerator ShowLoadingAfterCorrectClick()
//    {
//        if (loadingScreenPrefab != null)
//        {
//            if (loadingScreenInstance == null)
//                loadingScreenInstance = Instantiate(loadingScreenPrefab, transform);

//            loadingScreenInstance.SetActive(true);
//            yield return new WaitForSeconds(2f);
//            loadingScreenInstance.SetActive(false);
//        }
//    }

//    void NextRound()
//    {
//        PickNewTarget();
//        ShuffleGrid();
//        EnsureTargetFruitExists();
//    }

//    bool HasUnselectedTargetFruits()
//    {
//        foreach (var f in fruits)
//            if (f.fruitID == targetFruitID && !f.IsSelected())
//                return true;
//        return false;
//    }

//    void ShuffleGrid()
//    {
//        for (int i = 0; i < fruits.Count; i++)
//        {
//            int randomIndex = totalFruits > 0 ? Random.Range(0, totalFruits) : 0;
//            fruits[i].Init(this, fruitSprites[randomIndex], randomIndex);
//        }
//    }

//    public void SetMusicVolume(float value)
//    {
//        backgroundMusicVolume = Mathf.Clamp01(value);
//        if (AudioManager.Instance != null)
//            AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
//    }
//}








using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class FruitGridManager : MonoBehaviour
{
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

        InitializeGame();
    }

    void InitializeGame()
    {
        EnsureEventSystemAndRaycaster();
        EnsureGridLayoutGroup();

        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(backgroundMusic);
            AudioManager.Instance.SetMusicVolume(backgroundMusicVolume);
        }

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

            StartCoroutine(ShowLoadingAfterCorrectClick());

            if (!HasUnselectedTargetFruits())
                Invoke(nameof(NextRound), 0.6f);
        }
        else
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(wrongSound);
        }
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
        PickNewTarget();
        ShuffleGrid();
        EnsureTargetFruitExists();
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

        float parentWidth = gridParent.rect.width;
        float parentHeight = gridParent.rect.height;

        float totalSpacingX = (columns - 1) * gl.spacing.x;
        float totalSpacingY = (rows - 1) * gl.spacing.y;

        float cellWidth = (parentWidth - totalSpacingX) / columns;
        float cellHeight = (parentHeight - totalSpacingY) / rows;

        gl.cellSize = new Vector2(cellWidth, cellHeight);

        Debug.Log($"[FruitGridManager] Adjusted cell size to {gl.cellSize}");
    }

    // ✅ Update layout automatically on resolution change
    void OnRectTransformDimensionsChange()
    {
        AdjustGridCellSize();
    }
}
