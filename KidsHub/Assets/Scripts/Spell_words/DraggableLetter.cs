// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;

// public class DraggableLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
// {
//     public char letter;

//     private RectTransform rectTransform;
//     private CanvasGroup canvasGroup;
//     private Canvas parentCanvas;
//     private Vector2 startPos;
//     private bool isLocked;

//     private void Awake()
//     {
//         rectTransform = GetComponent<RectTransform>();
//         canvasGroup = gameObject.AddComponent<CanvasGroup>();
//         parentCanvas = GetComponentInParent<Canvas>();
//     }

//     // ✅ Added for backward compatibility
//     public void SetStartPosition()
//     {
//         startPos = rectTransform.anchoredPosition;
//     }

//     public void OnBeginDrag(PointerEventData eventData)
//     {
//         if (isLocked) return;

//         startPos = rectTransform.anchoredPosition;
//         canvasGroup.blocksRaycasts = false;
//         rectTransform.SetAsLastSibling(); // bring to front
//     }

//     // public void OnDrag(PointerEventData eventData)
//     // {
//     //     if (isLocked) return;
//     //     rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;

//     //     Vector2 pos;
//     //     RectTransformUtility.ScreenPointToLocalPointInRectangle(
//     //         parentCanvas.transform as RectTransform,
//     //         eventData.position,
//     //         parentCanvas.worldCamera,
//     //         out pos);
//     //     rectTransform.anchoredPosition = pos;
//     // }

//     public void OnDrag(PointerEventData eventData)
//     {
//         if (isLocked) return;

//         // Move relative to pointer delta in UI space
//         rectTransform.anchoredPosition += eventData.delta / parentCanvas.scaleFactor;
//     }



//     public void OnEndDrag(PointerEventData eventData)
//     {
//         if (isLocked) return;
//         canvasGroup.blocksRaycasts = true;
//     }

//     public void ResetPosition()
//     {
//         rectTransform.anchoredPosition = startPos;
//     }

//     public void LockLetter()
//     {
//         isLocked = true;
//         canvasGroup.blocksRaycasts = false;
//     }
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
    private Vector2 startPos;
    private bool isLocked;
    private Coroutine moveBackCoroutine;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = gameObject.AddComponent<CanvasGroup>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void SetStartPosition()
    {
        startPos = rectTransform.anchoredPosition;
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
        canvasGroup.blocksRaycasts = true;
    }

    public void ResetPosition()
    {
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
}
