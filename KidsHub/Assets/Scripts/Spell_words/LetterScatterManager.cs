using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LetterScatterManager : MonoBehaviour
{
    [Header("Letter Prefab Settings")]
    public GameObject letterPrefab; // Prefab with Image + DraggableLetter
    public RectTransform scatterArea; // UI parent area (RectTransform)
    public float scatterRadius = 300f;

    private readonly List<GameObject> activeLetters = new List<GameObject>();

    public void SetupLetters(string word, Dictionary<char, Sprite> letterSpriteDict)
    {
        ClearLetters();

        if (letterPrefab == null)
        {
            Debug.LogError("❌ Letter prefab not assigned in Inspector!");
            return;
        }

        if (scatterArea == null)
        {
            Debug.LogError("❌ Scatter area (parent) not assigned in Inspector!");
            return;
        }

        if (letterSpriteDict == null || letterSpriteDict.Count == 0)
        {
            Debug.LogError("❌ Letter sprite dictionary is empty!");
            return;
        }

        // Get safe area radius (fit within scatter area)
        float safeRadius = Mathf.Min(scatterArea.rect.width, scatterArea.rect.height) / 2f - 100f;
        safeRadius = Mathf.Clamp(scatterRadius, 0f, safeRadius);

        List<char> letters = new List<char>(word.ToUpperInvariant().ToCharArray());

        // 🔀 Shuffle
        for (int i = 0; i < letters.Count; i++)
        {
            int randomIndex = Random.Range(i, letters.Count);
            (letters[i], letters[randomIndex]) = (letters[randomIndex], letters[i]);
        }

        // 🧩 Spawn each letter
        foreach (char c in letters)
        {
            GameObject go = Instantiate(letterPrefab, scatterArea);
            go.name = $"Letter_{c}";

            // ✅ Ensure RectTransform scale/anchor is correct
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            // ✅ Apply sprite to Image
            Image img = go.GetComponent<Image>();
            if (img != null && letterSpriteDict.ContainsKey(c))
                img.sprite = letterSpriteDict[c];
            else
                Debug.LogWarning($"❗ Missing sprite for letter: {c}");

            // ✅ Random UI position (center-based)
            Vector2 randomPos = Random.insideUnitCircle * safeRadius;
            rect.anchoredPosition = randomPos;

            // ✅ Initialize draggable
            DraggableLetter draggable = go.GetComponent<DraggableLetter>();
            if (draggable != null)
            {
                draggable.letter = c;
                draggable.SetStartPosition();
            }

            activeLetters.Add(go);

            Debug.Log($"✅ Spawned draggable letter: {c} at {rect.anchoredPosition}");
        }
    }

    public void ClearLetters()
    {
        foreach (var l in activeLetters)
            if (l != null)
                Destroy(l);

        activeLetters.Clear();
    }
}
