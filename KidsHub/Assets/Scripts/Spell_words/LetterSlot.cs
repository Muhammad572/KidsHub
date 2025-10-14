// using UnityEngine;
// using UnityEngine.EventSystems;
// using UnityEngine.UI;
// using System.Collections;

// public class LetterSlot : MonoBehaviour, IDropHandler
// {
//     public char correctLetter;
//     private char currentLetter = '\0';
//     private bool filled = false;
//     private DraggableLetter lockedLetter;

//     [Header("Shake Settings")]
//     public float shakeDuration = 0.4f;
//     public float shakeMagnitude = 10f;
//     public float shakeScaleAmount = 0.05f;

//     private Vector3 originalScale;
//     private Coroutine shakeCoroutine;

//     private void Awake()
//     {
//         originalScale = transform.localScale;
//     }

//     // // 🧲 Triggered when a letter is dropped onto this slot
//     // public void OnDrop(PointerEventData eventData)
//     // {
//     //     if (filled) return;

//     //     DraggableLetter letter = eventData.pointerDrag?.GetComponent<DraggableLetter>();
//     //     if (letter == null) return;

//     //     int myIndex = WordManager.instance.GetSlotIndex(this);
//     //     if (!WordManager.instance.CanFillSlot(myIndex))
//     //     {
//     //         letter.ResetPosition();
//     //         return;
//     //     }

//     //     if (letter.letter == correctLetter)
//     //     {
//     //         letter.transform.SetParent(transform);
//     //         RectTransform letterRect = letter.GetComponent<RectTransform>();
//     //         letterRect.anchoredPosition = Vector2.zero;

//     //         letter.LockLetter();
//     //         lockedLetter = letter;
//     //         currentLetter = letter.letter;
//     //         filled = true;

//     //         WordManager.instance.PlayLetterSound(correctLetter);
//     //         WordManager.instance.CheckCompletion();
//     //     }
//     //     else
//     //     {
//     //         // ❌ Wrong letter feedback
//     //         WordManager.instance.PlayWrongSound();

//     //         if (shakeCoroutine != null)
//     //             StopCoroutine(shakeCoroutine);
//     //         shakeCoroutine = StartCoroutine(ShakeSlot());

//     //         letter.ResetPosition();
//     //     }
//     // }

//     public void OnDrop(PointerEventData eventData)
//     {
//         if (filled) return;

//         DraggableLetter letter = eventData.pointerDrag.GetComponent<DraggableLetter>();
//         if (letter != null && letter.letter == correctLetter)
//         {
//             letter.transform.SetParent(transform); // parent to slot
//             RectTransform letterRect = letter.GetComponent<RectTransform>();
//             letterRect.anchoredPosition = Vector2.zero; // snap to center

//             letter.LockLetter();
//             lockedLetter = letter;
//             currentLetter = letter.letter;
//             filled = true;

//             WordManager.instance.PlayLetterSound(correctLetter);
//             WordManager.instance.CheckCompletion();
//         }
//     }


//     private IEnumerator ShakeSlot()
//     {
//         float elapsed = 0f;
//         Vector3 baseScale = originalScale;

//         while (elapsed < shakeDuration)
//         {
//             elapsed += Time.deltaTime;
//             float scale = 1f + Mathf.Sin(Time.time * 40f) * shakeScaleAmount;
//             transform.localScale = baseScale * scale;
//             yield return null;
//         }

//         transform.localScale = baseScale;
//         shakeCoroutine = null;
//     }

//     public bool IsFilled() => filled;
//     public char CurrentLetter() => currentLetter;
// }


using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class LetterSlot : MonoBehaviour, IDropHandler
{
    public char correctLetter;
    private bool filled = false;
    private Vector3 originalScale;
    private Coroutine shakeCoroutine;

    public float shakeDuration = 0.4f;
    public float shakeScaleAmount = 0.05f;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (filled) return;

        DraggableLetter letter = eventData.pointerDrag?.GetComponent<DraggableLetter>();
        if (letter == null) return;

        // 🔹 Ensure this letter is the next one required
        if (!WordManager.instance.CanPlaceLetter(letter.letter))
        {
            WordManager.instance.PlayWrongSound();
            ShakeAndReturn(letter);
            return;
        }

        // ✅ Correct letter for this slot
        if (letter.letter == correctLetter)
        {
            PlaceLetter(letter);
        }
        else
        {
            // ❌ Wrong slot, wrong order, or mismatch
            WordManager.instance.PlayWrongSound();
            ShakeAndReturn(letter);
        }
    }

    private void PlaceLetter(DraggableLetter letter)
    {
        letter.transform.SetParent(transform);
        RectTransform rect = letter.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        letter.LockLetter();
        filled = true;

        float clipLength = WordManager.instance.PlayLetterSound(correctLetter);
        WordManager.instance.OnCorrectLetterPlaced(clipLength);
    }


    private void ShakeAndReturn(DraggableLetter letter)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeSlot());
        letter.ResetPosition();
    }

    private IEnumerator ShakeSlot()
    {
        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float scale = 1f + Mathf.Sin(Time.time * 40f) * shakeScaleAmount;
            transform.localScale = originalScale * scale;
            yield return null;
        }
        transform.localScale = originalScale;
        shakeCoroutine = null;
    }

    public bool IsFilled() => filled;
}
