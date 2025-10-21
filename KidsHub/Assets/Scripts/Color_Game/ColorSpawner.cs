using UnityEngine;
using System.Collections.Generic;

public class ColorSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ColorData
    {
        public string colorName;
        public Color colorValue;
        public AudioClip colorClip;
    }

    [Header("Spawn Settings")]
    public GameObject colorPrefab;
    public List<Transform> spawnPoints;

    [Header("Colors Data")]
    public List<ColorData> colorList = new List<ColorData>();

    [Header("Spawn Timing")]
    public float spawnDelay = 2f;
    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnDelay)
        {
            timer = 0f;
            SpawnColor();
        }
    }

    private void SpawnColor()
    {
        if (colorPrefab == null || colorList.Count == 0 || spawnPoints.Count == 0)
            return;

        // Pick random color data & spawn point
        ColorData randomColor = colorList[Random.Range(0, colorList.Count)];
        Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

        GameObject newColorObj = Instantiate(colorPrefab, randomPoint.position, Quaternion.identity);
        ColorManager colorManager = newColorObj.GetComponent<ColorManager>();

        if (colorManager != null)
        {
            colorManager.SetColorInfo(randomColor.colorName, randomColor.colorValue, randomColor.colorClip);
        }
    }
}




