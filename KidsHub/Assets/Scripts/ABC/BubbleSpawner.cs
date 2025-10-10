using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BubbleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class CharacterPrefabPair
    {
        public string character;
        public GameObject prefab;
    }

    [Header("Dependencies")]
    public List<CharacterPrefabPair> bubblePrefabs = new List<CharacterPrefabPair>();
    public ObjectShaker shaker;
    public AlphabetMatchManager matchManager;

    [Header("Spawn Settings")]
    public Transform spawnPoint; // Only one transform required
    public float spawnInterval = 2f;
    [Range(0f, 1f)]
    public float matchChance = 0.7f;
    public float spawnOffsetY = -2.5f;
    public float randomXOffsetRange = 0.3f;

    private Dictionary<string, GameObject> prefabLookup = new Dictionary<string, GameObject>();
    private List<string> availableCharacters = new List<string>();

    void Start()
    {
        // --- Validation ---
        if (spawnPoint == null)
        {
            Debug.LogError("BubbleSpawner: Please assign a Spawn Point transform!");
            enabled = false;
            return;
        }

        if (shaker == null)
        {
            Debug.LogError("BubbleSpawner: Missing ObjectShaker reference!");
            enabled = false;
            return;
        }

        if (matchManager == null)
        {
            Debug.LogError("BubbleSpawner: Missing AlphabetMatchManager reference!");
            enabled = false;
            return;
        }

        // --- Build prefab lookup ---
        foreach (var pair in bubblePrefabs)
        {
            string charStr = pair.character.ToUpper();
            if (!prefabLookup.ContainsKey(charStr) && pair.prefab != null)
                prefabLookup.Add(charStr, pair.prefab);
        }

        for (char c = 'A'; c <= 'Z'; c++)
        {
            string charStr = c.ToString();
            if (prefabLookup.ContainsKey(charStr))
                availableCharacters.Add(charStr);
        }

        if (availableCharacters.Count == 0)
        {
            Debug.LogError("No valid alphabet prefabs found (A–Z) in bubblePrefabs list.");
            enabled = false;
            return;
        }

        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            List<char> currentTargets = matchManager.GetCurrentLetters();
            if (currentTargets == null || currentTargets.Count == 0)
                continue;

            string nextChar;

            // Choose letter
            if (Random.value < matchChance)
            {
                nextChar = currentTargets[Random.Range(0, currentTargets.Count)].ToString();
            }
            else
            {
                List<string> nonMatching = new List<string>(availableCharacters);
                foreach (char c in currentTargets)
                    nonMatching.Remove(c.ToString());
                nextChar = nonMatching[Random.Range(0, nonMatching.Count)];
            }

            yield return StartCoroutine(ShakeAndSpawn(nextChar));
        }
    }

    IEnumerator ShakeAndSpawn(string characterToSpawn)
    {
        bool shakeDone = false;
        shaker.TriggerShake(() => shakeDone = true);
        yield return new WaitUntil(() => shakeDone);

        if (!prefabLookup.ContainsKey(characterToSpawn))
            yield break;

        GameObject prefab = prefabLookup[characterToSpawn];
        if (prefab == null) yield break;

        Vector3 spawnPos = spawnPoint.position;
        spawnPos.y += spawnOffsetY;
        spawnPos.x += Random.Range(-randomXOffsetRange, randomXOffsetRange);

        GameObject newBubble = Instantiate(prefab, spawnPos, Quaternion.identity);
        Bubble bubble = newBubble.GetComponent<Bubble>();
        if (bubble != null)
            bubble.bubbleLetter = characterToSpawn[0];

        newBubble.name = $"Bubble - {characterToSpawn}";
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (spawnPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(spawnPoint.position, 0.1f);
        UnityEditor.Handles.Label(spawnPoint.position + Vector3.up * 0.2f, "Spawn Point");
    }
#endif
}
