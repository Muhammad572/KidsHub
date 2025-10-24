// using UnityEngine;
// using UnityEngine.EventSystems;
// using System.Collections;

// public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
// {
//     public char letter;

//     private RectTransform rectTransform;
//     private CanvasGroup canvasGroup;
//     private Canvas parentCanvas;
//     // private Vector2 startPos;
//     private Vector3 startPosition;
//     public Vector3 StartPosition => startPosition;
//     private bool isLocked;
//     private Coroutine moveBackCoroutine;

//     private void Awake()
//     {
//         rectTransform = GetComponent<RectTransform>();

//         canvasGroup = GetComponent<CanvasGroup>();
//         if (canvasGroup == null)
//             canvasGroup = gameObject.AddComponent<CanvasGroup>();

//         parentCanvas = GetComponentInParent<Canvas>();
//         if (parentCanvas == null)
//         {
//             Canvas found = FindObjectOfType<Canvas>();
//             if (found != null)
//                 parentCanvas = found;
//             else
//                 Debug.LogError("❌ No Canvas found for DraggableLetter!");
//         }
//     }

//     private void Start()
//     {
//         startPos = rectTransform.anchoredPosition;
//         startPosition = transform.position; // 👈 add this
//     }

//     public void SetStartPosition()
//     {
//         startPos = rectTransform.anchoredPosition;
//     }

//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         if (isLocked) return;

//         canvasGroup.blocksRaycasts = false;
//         rectTransform.SetAsLastSibling();
//     }

//     public void OnDrag(PointerEventData eventData)
//     {
//         if (isLocked) return;
//         rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
//     }


//     public void OnEndDrag(PointerEventData eventData)
//     {
//         if (isLocked) return;

//         canvasGroup.alpha = 1f;
//         canvasGroup.blocksRaycasts = true; // ✅ always re-enable after drag ends
//     }

//     public void ResetPosition()
//     {
//         if (isLocked) return; // don’t move locked letters

//         if (canvasGroup != null)
//             canvasGroup.blocksRaycasts = true; // ✅ re-enable input

//         if (moveBackCoroutine != null)
//             StopCoroutine(moveBackCoroutine);

//         moveBackCoroutine = StartCoroutine(SmoothMoveBack());
//     }

//     private IEnumerator SmoothMoveBack()
//     {
//         if (canvasGroup != null)
//             canvasGroup.blocksRaycasts = true; // ✅ keep it interactive

//         Vector2 start = rectTransform.anchoredPosition;
//         float t = 0f;
//         while (t < 1f)
//         {
//             t += Time.deltaTime * 5f;
//             rectTransform.anchoredPosition = Vector2.Lerp(start, startPos, t);
//             yield return null;
//         }
//         rectTransform.anchoredPosition = startPos;
//     }


//     public void LockLetter()
//     {
//         isLocked = true;
//         canvasGroup.blocksRaycasts = false;
//     }

//     public bool IsLocked() => isLocked;
// }


using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public char letter;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas parentCanvas;

    private Vector2 startPos;              // 👈 restored
    private Vector3 startPosition;         // 👈 used by LetterSlot animations
    public Vector3 StartPosition => startPosition;

    private bool isLocked;
    private Coroutine moveBackCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Canvas found = FindFirstObjectByType<Canvas>(); // ✅ replaces obsolete FindObjectOfType
            if (found != null)
                parentCanvas = found;
            else
                Debug.LogError("❌ No Canvas found for DraggableLetter!");
        }
    }

    private void Start()
    {
        startPos = rectTransform.anchoredPosition;
        startPosition = transform.position; // world position (for DOTween moves)
    }

    public void SetStartPosition()
    {
        startPos = rectTransform.anchoredPosition;
        startPosition = transform.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.blocksRaycasts = false;
        rectTransform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }

    public void ResetPosition()
    {
        if (isLocked) return;

        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;

        if (moveBackCoroutine != null)
            StopCoroutine(moveBackCoroutine);

        moveBackCoroutine = StartCoroutine(SmoothMoveBack());
    }

    private IEnumerator SmoothMoveBack()
    {
        Vector2 start = rectTransform.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 5f;
            rectTransform.anchoredPosition = Vector2.Lerp(start, startPos, t);
            yield return null;
        }

        rectTransform.anchoredPosition = startPos;
    }

    public void LockLetter()
    {
        isLocked = true;
        canvasGroup.blocksRaycasts = false;
    }

    public bool IsLocked() => isLocked;
}
