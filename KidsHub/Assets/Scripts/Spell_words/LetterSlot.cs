using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LetterSlot : MonoBehaviour
{
    public char correctLetter;
    private char currentLetter = '\0';
    private bool filled = false;
    private DraggableLetter lockedLetter;

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (filled) return; // already filled

        DraggableLetter letter = other.GetComponent<DraggableLetter>();
        if (letter == null) return;

        // ✅ Check correct letter match
        if (letter.letter == correctLetter)
        {
            // Snap letter to slot center
            letter.transform.position = transform.position;

            // Lock it in place
            letter.LockLetter();
            lockedLetter = letter;

            currentLetter = letter.letter;
            filled = true;

            Debug.Log($"✅ Correct letter '{letter.letter}' for slot '{correctLetter}'");

            // Notify word manager
            WordManager.instance.CheckCompletion();
        }
        else
        {
            // ❌ Wrong letter — bounce back
            Debug.Log($"❌ Wrong letter '{letter.letter}' for slot '{correctLetter}'");
            letter.ResetPosition();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // Prevent another letter from sitting on top of a filled slot
        if (!filled) return;

        var letter = other.GetComponent<DraggableLetter>();
        if (letter != null && letter != lockedLetter)
        {
            // Immediately push back wrong one
            letter.ResetPosition();
        }
    }

    public bool IsFilled() => filled;
    public char CurrentLetter() => currentLetter;
}
