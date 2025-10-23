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

    // private void Awake()
    // {
    //     rectTransform = GetComponent<RectTransform>();
    //     canvasGroup = gameObject.AddComponent<CanvasGroup>();
    //     parentCanvas = GetComponentInParent<Canvas>();
    // }
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas == null)
        {
            Canvas found = FindObjectOfType<Canvas>();
            if (found != null)
                parentCanvas = found;
            else
                Debug.LogError("❌ No Canvas found for DraggableLetter!");
        }
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
