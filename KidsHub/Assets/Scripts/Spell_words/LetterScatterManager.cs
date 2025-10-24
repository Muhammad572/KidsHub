using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

public class LetterScatterManager : MonoBehaviour
{
    [Header("Letter Prefab Settings")]
    public GameObject letterPrefab; // Prefab with Image + DraggableLetter
    public RectTransform scatterArea; // UI parent area (RectTransform)
    public float scatterRadius = 300f;
    [Tooltip("Minimum allowed distance between letters (no overlap)")]
    public float minLetterSpacing = 120f;

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

        float safeRadius = Mathf.Min(scatterArea.rect.width, scatterArea.rect.height) / 2f - 100f;
        safeRadius = Mathf.Clamp(scatterRadius, 0f, safeRadius);

        List<char> letters = new List<char>(word.ToUpperInvariant().ToCharArray());

        // 🔀 Shuffle
        for (int i = 0; i < letters.Count; i++)
        {
            int randomIndex = Random.Range(i, letters.Count);
            (letters[i], letters[randomIndex]) = (letters[randomIndex], letters[i]);
        }

        // 🧩 Spawn each letter safely
        foreach (char c in letters)
        {
            GameObject go = Instantiate(letterPrefab, scatterArea);
            go.name = $"Letter_{c}";

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.localScale = Vector3.one;
            rect.SetAsLastSibling();

            // ✅ Assign letter image
            Image img = go.GetComponent<Image>();
            if (img != null && letterSpriteDict.ContainsKey(c))
                img.sprite = letterSpriteDict[c];
            else
                Debug.LogWarning($"❗ Missing sprite for letter: {c}");

            // // 🧭 Find non-overlapping position
            // Vector2 pos = GetNonOverlappingPosition(safeRadius);
            // rect.anchoredPosition = pos;

            // // ✅ Initialize draggable
            // DraggableLetter draggable = go.GetComponent<DraggableLetter>();
            // if (draggable != null)
            // {
            //     draggable.letter = c;
            //     draggable.SetStartPosition();
            // }

            // activeLetters.Add(go);

            // Debug.Log($"✅ Spawned draggable letter: {c} at {rect.anchoredPosition}");

            // 🧭 Find non-overlapping position
            Vector2 pos = GetNonOverlappingPosition(safeRadius);
            rect.anchoredPosition = pos;

            // 🔥 Animate appearance
            rect.localScale = Vector3.zero;
            rect.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

            Sequence appearSeq = DOTween.Sequence();
            appearSeq.Append(rect.DOScale(1f, 0.5f).SetEase(Ease.OutBack));
            appearSeq.Join(rect.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutSine));
            appearSeq.Play();

            // ✅ Initialize draggable
            DraggableLetter draggable = go.GetComponent<DraggableLetter>();
            if (draggable != null)
            {
                draggable.letter = c;
                draggable.SetStartPosition();
            }

            activeLetters.Add(go);
        }
    }

    // 🧩 Generate a random position that doesn't overlap existing letters
    private Vector2 GetNonOverlappingPosition(float safeRadius)
    {
        const int maxAttempts = 100;
        Vector2 newPos = Vector2.zero;
        bool valid = false;

        for (int attempt = 0; attempt < maxAttempts && !valid; attempt++)
        {
            newPos = Random.insideUnitCircle * safeRadius;
            valid = true;

            foreach (GameObject letter in activeLetters)
            {
                if (letter == null) continue;
                // RectTransform rt = letter.GetComponent<RectTransform>();
                // After instantiating newLetter (your draggable letter object)
                RectTransform rt = letter.GetComponent<RectTransform>();

                // Start tiny and rotate slightly
                rt.localScale = Vector3.zero;
                rt.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

                // Animate in
                rt.DOScale(1f, 0.5f).SetEase(Ease.OutBack);
                rt.DOLocalRotate(Vector3.zero, 0.5f).SetEase(Ease.OutSine);
                if (rt == null) continue;

                float dist = Vector2.Distance(rt.anchoredPosition, newPos);
                if (dist < minLetterSpacing)
                {
                    valid = false;
                    break;
                }
            }
        }

        if (!valid)
            Debug.LogWarning("⚠️ Couldn’t find perfect spacing; placing letter anyway.");

        return newPos;
    }

    public void ClearLetters()
    {
        foreach (var l in activeLetters)
            if (l != null)
                Destroy(l);

        activeLetters.Clear();
    }
}
