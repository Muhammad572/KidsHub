using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

[System.Serializable]
public class AnimalData
{
    public string animalName; // e.g. "ELEPHANT"
    public Sprite animalSprite;
}

public class AnimalManager : MonoBehaviour
{
    [Header("Animal Data")]
    public List<AnimalData> animals;
    public Image animalImage;

    [Header("Letter Sprites (A–Z)")]
    [Tooltip("Assign 26 sprites in correct order (A–Z).")]
    public Sprite[] letterSprites;

    [Header("Managers")]
    public LetterScatterManager letterScatterManager;
    public WordManager wordManager;

    private int currentAnimalIndex = -1;
    private int lastAnimalIndex = -1;
    private Dictionary<char, Sprite> letterSpriteDict = new Dictionary<char, Sprite>();

    void Awake()
    {
        LoadLetterSprites();
    }

    void Start()
    {
        if (animalImage == null) Debug.LogError("❌ Animal Image not assigned!");
        if (wordManager == null) Debug.LogError("❌ WordManager not assigned!");
        if (letterScatterManager == null) Debug.LogError("❌ LetterScatterManager not assigned!");

        // Subscribe to word complete event from WordManager
        wordManager.OnWordCompleted += HandleWordCompleted;

        LoadNextAnimal();
    }

    void LoadLetterSprites()
    {
        letterSpriteDict.Clear();

        if (letterSprites == null || letterSprites.Length < 26)
        {
            Debug.LogError("❌ Please assign all 26 letter sprites (A–Z) in the Inspector.");
            return;
        }

        for (int i = 0; i < 26; i++)
        {
            char c = (char)('A' + i);
            letterSpriteDict[c] = letterSprites[i];
        }

        Debug.Log($"✅ Loaded {letterSpriteDict.Count} letter sprites from Inspector.");
    }

    public void LoadAnimal(int index)
    {
        if (animals == null || animals.Count == 0)
        {
            Debug.LogError("❌ No animals assigned!");
            return;
        }

        if (index < 0 || index >= animals.Count)
        {
            Debug.LogError($"❌ Invalid animal index: {index}");
            return;
        }

        currentAnimalIndex = index;
        lastAnimalIndex = index;

        AnimalData animal = animals[index];

        if (animal == null)
        {
            Debug.LogError($"❌ Animal at index {index} is null!");
            return;
        }

        if (animal.animalSprite == null)
        {
            Debug.LogError($"❌ Missing sprite for animal: {animal.animalName}");
            return;
        }

        // ✅ Set animal image
        animalImage.sprite = animal.animalSprite;

        // ✅ Create word slots and letters
        string word = animal.animalName.Trim().ToUpper();
        wordManager.CreateSlots(word);
        letterScatterManager.SetupLetters(word, letterSpriteDict);

        Debug.Log($"✅ Loaded animal: {word}");
    }

    public void LoadNextAnimal()
    {
        if (animals == null || animals.Count == 0)
        {
            Debug.LogError("❌ No animals available!");
            return;
        }

        int nextIndex = Random.Range(0, animals.Count);

        // 🔄 Prevent same animal twice in a row
        if (animals.Count > 1)
        {
            while (nextIndex == lastAnimalIndex)
                nextIndex = Random.Range(0, animals.Count);
        }

        LoadAnimal(nextIndex);
    }

    private void HandleWordCompleted()
    {
        Debug.Log("🎉 Word completed! Loading next animal in 2 seconds...");
        Invoke(nameof(LoadNextAnimal), 2f); // Delay for celebration animation
    }

    private void OnDestroy()
    {
        // Cleanly unsubscribe
        if (wordManager != null)
            wordManager.OnWordCompleted -= HandleWordCompleted;
    }
}
