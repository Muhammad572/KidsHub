using UnityEngine;

public class BubbleSpawner_AnimalC : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("Drag your Bubble Prefab here")]
    public GameObject bubblePrefab;
    [Tooltip("Time between each bubble spawn")]
    public float spawnRate = 0.5f;
    [Tooltip("The range (left to right) where bubbles can spawn")]
    public float spawnWidth = 5f;

    private float nextSpawnTime;

    void Update()
    {
        if (Time.time > nextSpawnTime)
        {
            SpawnBubble();
            // Schedule the next spawn
            nextSpawnTime = Time.time + spawnRate;
        }
    }

    void SpawnBubble()
    {
        // 1. Calculate a random X position within the specified width
        float randomX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        
        // 2. Determine the spawn position (Spawner's Y, random X)
        Vector3 spawnPosition = new Vector3(transform.position.x + randomX, 
                                            transform.position.y, 
                                            0);

        // 3. Create the bubble instance
        GameObject newBubble = Instantiate(bubblePrefab, spawnPosition, Quaternion.identity);

        // Optional: Get the movement script and slightly vary the speed for realism
        BubbleMovement_AnimalC movement = newBubble.GetComponent<BubbleMovement_AnimalC>();
        if (movement != null)
        {
            // Vary the speed slightly for a more natural look
            movement.upwardSpeed += Random.Range(-0.5f, 0.5f);
        }
    }
}