// using UnityEngine;
// using UnityEngine.UI;

// public class FruitItem : MonoBehaviour
// {
//     [Header("References")]
//     public Image fruitImage;                 // assign in prefab (child Image)
//     public Button button;                    // optional: assign in prefab. If null, script will try to find one.
//     [HideInInspector] public int fruitID;
//     private FruitGridManager gridManager;
//     private bool isSelected = false;

//     void Awake()
//     {
//         // auto-find button if not assigned
//         if (button == null)
//         {
//             button = GetComponent<Button>() ?? GetComponentInChildren<Button>();
//         }

//         if (button != null)
//         {
//             // ensure we don't double-add listeners
//             button.onClick.RemoveListener(OnClick);
//             button.onClick.AddListener(OnClick);
//             button.interactable = true;
//         }
//         else
//         {
//             Debug.LogWarning($"[FruitItem] No Button found on prefab '{gameObject.name}'. Clicks won't work.");
//         }

//         if (fruitImage != null)
//         {
//             fruitImage.raycastTarget = true; // make sure it receives UI clicks
//         }
//     }

//     public void Init(FruitGridManager manager, Sprite sprite, int id)
//     {
//         gridManager = manager;
//         fruitID = id;
//         isSelected = false;

//         if (fruitImage != null)
//             fruitImage.sprite = sprite;

//         // re-enable button (in case it was disabled previously)
//         if (button != null)
//             button.interactable = true;
//     }

//     public void OnClick()
//     {
//         Debug.Log($"[FruitItem] Clicked id={fruitID} selected={isSelected}");
//         if (!isSelected && gridManager != null)
//         {
//             gridManager.OnFruitClicked(this);
//         }
//     }

//     public void MarkAsSelected(Sprite selectedSprite)
//     {
//         isSelected = true;
//         if (fruitImage != null && selectedSprite != null)
//             fruitImage.sprite = selectedSprite;

//         // disable button so it can't be clicked again
//         if (button != null)
//             button.interactable = false;
//     }

//     public bool IsSelected()
//     {
//         return isSelected;
//     }
// }

using UnityEngine;
using UnityEngine.UI;

public class FruitItem : MonoBehaviour
{
    [Header("References (optional - auto-found if empty)")]
    public Image fruitImage;                 // assign in prefab (child Image) or leave empty
    public Button button;                    // optional: assign in prefab. If null, script will try to find or add one.
    [HideInInspector] public int fruitID;
    private FruitGridManager gridManager;
    private bool isSelected = false;

    void Awake()
    {
        // try to find the button
        if (button == null)
            button = GetComponent<Button>() ?? GetComponentInChildren<Button>();

        // if still null, add a Button component so clicks can be handled (safe fallback)
        if (button == null)
        {
            button = gameObject.AddComponent<Button>();
            Debug.LogWarning($"[FruitItem] No Button found on '{name}', added Button component automatically.");
        }

        // try to find image if not assigned
        if (fruitImage == null)
        {
            // prefer a child Image that is not the background of the button itself
            fruitImage = GetComponentInChildren<Image>();
            if (fruitImage == null)
                Debug.LogWarning($"[FruitItem] No Image found on '{name}'. Please assign fruitImage in prefab.");
        }

        // wire click
        if (button != null)
        {
            // remove before add to avoid duplicates during domain reloads
            button.onClick.RemoveListener(OnClick);
            button.onClick.AddListener(OnClick);
            button.interactable = true;
        }

        // ensure this image will block raycasts so Button receives clicks
        if (fruitImage != null)
            fruitImage.raycastTarget = true;
    }

    public void Init(FruitGridManager manager, Sprite sprite, int id)
    {
        gridManager = manager;
        fruitID = id;
        isSelected = false;

        if (fruitImage != null)
            fruitImage.sprite = sprite;

        if (button != null)
            button.interactable = true;
    }

    public void OnClick()
    {
        Debug.Log($"[FruitItem] Clicked id={fruitID} selected={isSelected} name={name}");
        if (!isSelected && gridManager != null)
        {
            gridManager.OnFruitClicked(this);
        }
    }

    public void MarkAsSelected(Sprite selectedSprite)
    {
        isSelected = true;
        if (fruitImage != null && selectedSprite != null)
            fruitImage.sprite = selectedSprite;

        if (button != null)
            button.interactable = false;
    }

    public bool IsSelected() => isSelected;
}
