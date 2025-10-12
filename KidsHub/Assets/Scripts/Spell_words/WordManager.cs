using UnityEngine;
using System.Collections.Generic;

public class WordManager : MonoBehaviour
{
    public static WordManager instance;

    [Header("Slot Setup")]
    public GameObject slotPrefab;
    public Transform slotsParent;

    [Header("Letter Sprite List (A–Z in order)")]
    public List<Sprite> letterSprites;

    [Header("Layout Settings")]
    public float spacing = 1.2f;
    public float slotScale = 1.0f;
    public System.Action OnWordCompleted;

    private List<LetterSlot> slots = new List<LetterSlot>();

    private void Awake()
    {
        instance = this;
    }

    public void CreateSlots(string word)
    {
        foreach (Transform child in slotsParent)
            Destroy(child.gameObject);

        slots.Clear();

        float totalWidth = (word.Length - 1) * spacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < word.Length; i++)
        {
            char c = char.ToUpper(word[i]);
            int spriteIndex = c - 'A';

            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            slotObj.name = "Slot_" + c + "_" + i;
            slotObj.transform.localPosition = new Vector3(startX + i * spacing, 0, 0);
            slotObj.transform.localScale = Vector3.one * slotScale;

            SpriteRenderer sr = slotObj.GetComponent<SpriteRenderer>();
            if (sr != null && spriteIndex >= 0 && spriteIndex < letterSprites.Count)
                sr.sprite = letterSprites[spriteIndex];

            LetterSlot slotScript = slotObj.GetComponent<LetterSlot>();
            if (slotScript != null)
                slotScript.correctLetter = c;

            slots.Add(slotScript);
        }

        Debug.Log($"🧩 Created slots for word: {word}");
    }

    public void CheckCompletion()
    {
        string formedWord = "";
        string targetWord = "";

        foreach (LetterSlot slot in slots)
        {
            if (slot.IsFilled())
                formedWord += slot.CurrentLetter();
            else
                formedWord += "_";

            targetWord += slot.correctLetter;
        }

        Debug.Log($"🔠 Current Progress: {formedWord}");

        if (formedWord == targetWord)
        {
            Debug.Log($"🎉 Word complete: {targetWord}");
            OnWordCompleted?.Invoke(); // 🔥 Notify AnimalManager
        }
    }
}
