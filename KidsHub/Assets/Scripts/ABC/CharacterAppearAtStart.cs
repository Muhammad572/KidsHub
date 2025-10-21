using UnityEngine;
using System.Collections;

public class CharacterAppearAtStart : MonoBehaviour
{
    [Header("Character Settings")]
    public GameObject character;        // The character GameObject to show/hide

    [Header("Timing Settings")]
    public float firstAppearTime = 0f;  // When to first show the character (0 = at start)
    public float secondAppearDelay = 4f; // Time after first appear before showing again
    public float appearDuration = 2f;   // How long the character stays visible each time

    void Start()
    {
        if (character != null)
            character.SetActive(false);

        // Start the appearance sequence
        StartCoroutine(AppearSequence());
    }

    IEnumerator AppearSequence()
    {
        // Wait for initial delay (optional)
        yield return new WaitForSeconds(firstAppearTime);

        // 1️⃣ First appearance
        character.SetActive(true);
        yield return new WaitForSeconds(appearDuration);
        character.SetActive(false);

        // Wait before second appearance
        yield return new WaitForSeconds(secondAppearDelay);

        // 2️⃣ Second appearance
        character.SetActive(true);
        yield return new WaitForSeconds(appearDuration);
        character.SetActive(false);
    }
}
