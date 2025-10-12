using UnityEngine;
using System.Collections.Generic;

public class LetterScatterManager : MonoBehaviour
{
    [Header("Letter Prefab Settings")]
    public GameObject letterPrefab; // Prefab with SpriteRenderer + DraggableLetter + Collider2D (isTrigger)
    public Transform scatterArea;   // Parent area where letters appear
    public float scatterRadius = 5f;

    private List<GameObject> activeLetters = new List<GameObject>();

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

        List<char> letters = new List<char>(word.ToUpperInvariant().ToCharArray());

        // 🔀 Shuffle letters
        for (int i = 0; i < letters.Count; i++)
        {
            int randomIndex = Random.Range(i, letters.Count);
            (letters[i], letters[randomIndex]) = (letters[randomIndex], letters[i]);
        }

        // 🧩 Spawn letters
        foreach (char c in letters)
        {
            GameObject go = Instantiate(letterPrefab, scatterArea);
            go.name = $"Letter_{c}";

            // ✅ Set sprite
            SpriteRenderer sr = go.GetComponent<SpriteRenderer>();
            if (sr != null && letterSpriteDict.ContainsKey(c))
                sr.sprite = letterSpriteDict[c];
            else
                Debug.LogWarning($"❗ Missing sprite for letter: {c}");

            // ✅ Random scatter position
            Vector2 randomPos = Random.insideUnitCircle * scatterRadius;
            go.transform.localPosition = randomPos;

            // ✅ Record correct spawn position
            DraggableLetter draggable = go.GetComponent<DraggableLetter>();
            if (draggable != null)
            {
                draggable.letter = c;
                draggable.SetStartPosition(); // 👈 Correct placement
                Debug.Log($"✅ Spawned draggable letter: {c}");
            }
            else
            {
                Debug.LogError($"❌ No DraggableLetter script found on prefab {go.name}");
            }

            // ✅ Ensure collider is set correctly
            Collider2D col = go.GetComponent<Collider2D>();
            if (col != null)
                col.isTrigger = true;

            activeLetters.Add(go);
        }
    }

    void ClearLetters()
    {
        foreach (var l in activeLetters)
            if (l != null) Destroy(l);
        activeLetters.Clear();
    }
}
