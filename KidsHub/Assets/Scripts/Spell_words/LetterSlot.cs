// using UnityEngine;
// using UnityEngine.EventSystems;
// using System.Collections;
// using DG.Tweening; 

// public class LetterSlot : MonoBehaviour, IDropHandler
// {
//     public char correctLetter;
//     private bool filled = false;
//     public int slotIndex;
//     private Vector3 originalScale;
//     private Coroutine shakeCoroutine;

//     public float shakeDuration = 0.4f;
//     public float shakeScaleAmount = 0.05f;

//     private void Awake()
//     {
//         originalScale = transform.localScale;
//     }

//     // public void OnDrop(PointerEventData eventData)
//     // {
//     //     if (filled) return;

//     //     DraggableLetter letter = eventData.pointerDrag?.GetComponent<DraggableLetter>();
//     //     if (letter == null) return;

//     //     // 🔹 Ensure this letter is the next one required
//     //     if (!WordManager.instance.CanPlaceLetter(letter.letter))
//     //     {
//     //         WordManager.instance.PlayWrongSound();
//     //         ShakeAndReturn(letter);
//     //         return;
//     //     }

//     //     // ✅ Correct letter for this slot
//     //     if (letter.letter == correctLetter)
//     //     {
//     //         PlaceLetter(letter);
//     //         HighlightSlot(Color.green);
//     //     }
//     //     else
//     //     {
//     //         // ❌ Wrong slot, wrong order, or mismatch
//     //         WordManager.instance.PlayWrongSound();
//     //         ShakeAndReturn(letter);
//     //     }
//     // }

//     public void OnDrop(PointerEventData eventData)
//     {
//         if (!eventData.pointerDrag) return;

//         DraggableLetter letter = eventData.pointerDrag.GetComponent<DraggableLetter>();
//         if (letter == null) return;

//         // Prevent double-drop if already filled
//         if (filled) return;

//         // Make sure WordManager exposes currentWord
//         // string currentWord = WordManager.instance.currentWord;
//         string currentWord = WordManager.instance.CurrentWord;

//         // Compare with the correct character for this slot
//         bool isCorrect = slotIndex < currentWord.Length &&
//                         char.ToUpper(letter.letter) == char.ToUpper(currentWord[slotIndex]);

//         if (isCorrect)
//         {
//             PlaceLetter(letter);
//         }
//         else
//         {
//             Debug.Log($"❌ Wrong letter {letter.letter}, expected {currentWord[slotIndex]}");
//             WrongLetterFeedback(letter);
//         }
//     }

//     private void WrongLetterFeedback(DraggableLetter letter)
//     {
//         // Disable interaction briefly
//         letter.GetComponent<CanvasGroup>().blocksRaycasts = false;

//         // Smooth shake
//         transform.DOKill();
//         transform.DOShakeScale(0.35f, 0.15f, 8, 90f, true)
//             .OnComplete(() =>
//             {
//                 // Return letter smoothly
//                 // letter.transform.DOMove(letter.startPosition, 0.4f)
//                 letter.transform.DOMove(letter.StartPosition, 0.4f)
//                     .SetEase(Ease.InOutSine)
//                     .OnComplete(() =>
//                     {
//                         // Re-enable interaction
//                         letter.GetComponent<CanvasGroup>().blocksRaycasts = true;
//                         letter.ResetPosition();
//                     });
//             });
//     }



//     private void PlaceLetter(DraggableLetter letter)
//     {
//         letter.transform.SetParent(transform);
//         RectTransform rect = letter.GetComponent<RectTransform>();
//         rect.anchoredPosition = Vector2.zero;

//         letter.LockLetter();
//         filled = true;

//         float clipLength = WordManager.instance.PlayLetterSound(correctLetter);
//         // 🎯 Tween feedback on success
//         rect.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
//         transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 3, 0.5f);

//         WordManager.instance.OnCorrectLetterPlaced(clipLength);
//     }
//     // private void ShakeAndReturn(DraggableLetter letter)
//     // {
//     //     if (shakeCoroutine != null)
//     //         StopCoroutine(shakeCoroutine);
//     //     shakeCoroutine = StartCoroutine(ShakeSlot());

//     //     letter.ResetPosition();

//     //     // ✅ Safety delay to re-enable after physics/raycast glitch
//     //     StartCoroutine(ReenableRaycast(letter));
//     // }

//     private void ShakeAndReturn(DraggableLetter letter)
//     {
//         transform.DOKill();
//         transform.DOShakeScale(0.4f, 0.15f, 10, 90f, true);
//         letter.ResetPosition();

//         StartCoroutine(ReenableRaycast(letter));
//     }


//     private IEnumerator ReenableRaycast(DraggableLetter letter)
//     {
//         yield return new WaitForSeconds(0.1f);
//         var cg = letter.GetComponent<CanvasGroup>();
//         if (cg != null && !letter.IsLocked())
//             cg.blocksRaycasts = true;
//     }



//     private IEnumerator ShakeSlot()
//     {
//         float elapsed = 0f;
//         while (elapsed < shakeDuration)
//         {
//             elapsed += Time.deltaTime;
//             float scale = 1f + Mathf.Sin(Time.time * 40f) * shakeScaleAmount;
//             transform.localScale = originalScale * scale;
//             yield return null;
//         }
//         transform.localScale = originalScale;
//         shakeCoroutine = null;
//     }

//     public bool IsFilled() => filled;

//     private void HighlightSlot(Color color)
//     {
//         var img = GetComponent<UnityEngine.UI.Image>();
//         if (img != null)
//         {
//             img.DOColor(color, 0.2f).OnComplete(() => img.DOColor(Color.white, 0.2f));
//         }
//     }

// }


using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public class LetterSlot : MonoBehaviour, IDropHandler
{
    public int slotIndex;                  // 👈 assign in inspector (0 for first, 1 for second, etc.)
    public char correctLetter;

    private bool filled = false;
    private Vector3 originalScale;

    public float shakeDuration = 0.3f;
    public float shakeScaleAmount = 0.1f;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!eventData.pointerDrag || filled) return;

        DraggableLetter letter = eventData.pointerDrag.GetComponent<DraggableLetter>();
        if (letter == null || letter.IsLocked()) return;

        // 1️⃣ Check if this slot is the NEXT one expected
        int nextIndex = WordManager.instance.CurrentFilledIndex; // new helper we'll add below
        if (slotIndex != nextIndex)
        {
            Debug.Log($"❌ Can't place yet! Need to fill slot {nextIndex} first.");
            WordManager.instance.PlayWrongSound();
            WrongLetterFeedback(letter);
            return;
        }

        // 2️⃣ Check correct letter
        char expected = WordManager.instance.CurrentWord[slotIndex];
        bool isCorrect = char.ToUpper(letter.letter) == char.ToUpper(expected);

        if (isCorrect)
        {
            PlaceLetter(letter);
            WordManager.instance.AdvanceProgress(); // ✅ move to next letter
        }
        else
        {
            Debug.Log($"❌ Wrong letter {letter.letter}, expected {expected}");
            WordManager.instance.PlayWrongSound();
            WrongLetterFeedback(letter);
        }
    }

    private void PlaceLetter(DraggableLetter letter)
    {
        filled = true;

        letter.transform.SetParent(transform);
        RectTransform rect = letter.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;

        letter.LockLetter();

        float clipLength = WordManager.instance.PlayLetterSound(correctLetter);

        // ✅ Fancy DOTween feedback
        rect.DOScale(1.2f, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.OutQuad);
        transform.DOPunchScale(Vector3.one * 0.1f, 0.3f, 3, 0.5f);

        WordManager.instance.OnCorrectLetterPlaced(clipLength);
    }

    private void WrongLetterFeedback(DraggableLetter letter)
    {
        transform.DOKill();
        transform.DOShakeScale(shakeDuration, shakeScaleAmount, 10, 90f, true)
            .OnComplete(() =>
            {
                letter.ResetPosition();
            });
    }

    public bool IsFilled() => filled;
}
