// using UnityEngine;
// using System.Collections.Generic;

// public class ColorSpawner : MonoBehaviour
// {
//     [System.Serializable]
//     public class ColorData
//     {
//         public string colorName;
//         public Color colorValue;
//         public AudioClip colorClip; // each color’s unique pop sound
//     }

//     [Header("Spawn Settings")]
//     public GameObject colorPrefab;
//     public List<Transform> spawnPoints;

//     [Header("Colors Data")]
//     public List<ColorData> colorList = new List<ColorData>();

//     [Header("Spawn Timing")]
//     public float spawnDelay = 2f;
//     private float timer;

//     [Header("Shared Audio Clips")]
//     public AudioClip sparkleChimeClip; // ✨ used with every bubble pop
//     public AudioClip backgroundMusic;
//     [Range(0f, 1f)] public float musicVolume = 0.5f;

//     private AudioClip currentMusic = null;
//     private bool musicStarted = false;

//     private void Start()
//     {
//         // 🎵 Start background music via AudioManager
//         if (AudioManager.Instance != null && backgroundMusic != null)
//         {
//             currentMusic = backgroundMusic;
//             AudioManager.Instance.PlayBackgroundMusic(currentMusic);
//             AudioManager.Instance.SetMusicVolume(musicVolume);
//             musicStarted = true;
//         }
//     }

//     private void Update()
//     {
//         timer += Time.deltaTime;
//         if (timer >= spawnDelay)
//         {
//             timer = 0f;
//             SpawnColor();
//         }

//         // keep volume synced
//         if (musicStarted && AudioManager.Instance != null)
//         {
//             AudioManager.Instance.SetMusicVolume(musicVolume);
//         }
//     }

//     private void SpawnColor()
//     {
//         if (colorPrefab == null || colorList.Count == 0 || spawnPoints.Count == 0)
//             return;

//         // random color & position
//         ColorData randomColor = colorList[Random.Range(0, colorList.Count)];
//         Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

//         GameObject newColorObj = Instantiate(colorPrefab, randomPoint.position, Quaternion.identity);
//         ColorManager colorManager = newColorObj.GetComponent<ColorManager>();

//         if (colorManager != null)
//         {
//             // 🟣 sparkleChimeClip is shared, not from color data
//             colorManager.SetColorInfo(randomColor.colorName, randomColor.colorValue, randomColor.colorClip, sparkleChimeClip);
//         }
//     }

//     public void SetAndPlayBackgroundMusic(AudioClip newClip, float newVolume = -1f)
//     {
//         if (newClip == null || newClip == currentMusic)
//             return;

//         currentMusic = newClip;
//         musicStarted = true;

//         if (AudioManager.Instance != null)
//         {
//             AudioManager.Instance.PlayBackgroundMusic(currentMusic);
//             if (newVolume >= 0f)
//             {
//                 musicVolume = Mathf.Clamp01(newVolume);
//                 AudioManager.Instance.SetMusicVolume(musicVolume);
//             }
//         }
//     }
// }

using DG.Tweening;
using UnityEngine;
using System.Collections.Generic;

public class ColorSpawner : MonoBehaviour
{
    [System.Serializable]
    public class ColorData
    {
        public string colorName;
        public Color colorValue;
        public AudioClip colorClip; // each color’s unique pop sound
    }

    [Header("Spawn Settings")]
    public GameObject colorPrefab;
    public List<Transform> spawnPoints;

    [Header("Colors Data")]
    public List<ColorData> colorList = new List<ColorData>();

    [Header("Spawn Timing")]
    public float spawnDelay = 2f;
    private float timer;

    [Header("Shared Audio Clips")]
    public AudioClip sparkleChimeClip; // ✨ used with every bubble pop
    public AudioClip backgroundMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private AudioClip currentMusic = null;
    private bool musicStarted = false;

    private int currentColorIndex = 0;
    private int lastColorIndex = -1;

    private void Start()
    {
        // 🎵 Start background music via AudioManager
        if (AudioManager.Instance != null && backgroundMusic != null)
        {
            currentMusic = backgroundMusic;
            AudioManager.Instance.PlayBackgroundMusic(currentMusic);
            AudioManager.Instance.SetMusicVolume(musicVolume);
            musicStarted = true;
        }
    }

    private void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnDelay)
        {
            timer = 0f;
            SpawnColor();
        }

        // keep volume synced
        if (musicStarted && AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(musicVolume);
        }
    }

 
  private void SpawnColor()
{
    if (colorPrefab == null || colorList.Count == 0 || spawnPoints.Count == 0)
        return;

    // 🎨 pick a random color index — but not the same as last time
    int randomIndex;
    do
    {
        randomIndex = Random.Range(0, colorList.Count);
    } while (colorList.Count > 1 && randomIndex == lastColorIndex);

    lastColorIndex = randomIndex;
    ColorData randomColor = colorList[randomIndex];

    // 🎯 random spawn position
    Transform randomPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];

    // 🪄 instantiate and animate
    GameObject newColorObj = Instantiate(colorPrefab, randomPoint.position, Quaternion.identity);
    newColorObj.transform.localScale = Vector3.one * 0.3f;

    ColorManager colorManager = newColorObj.GetComponent<ColorManager>();
    if (colorManager != null)
    {
        colorManager.SetColorInfo(randomColor.colorName, randomColor.colorValue, randomColor.colorClip, sparkleChimeClip);
    }

    // 🌈 smooth grow-in animation
    newColorObj.transform.DOScale(Vector3.one, 1.2f)
        .SetEase(Ease.OutSine);

    // 💫 optional small rotation for life
    newColorObj.transform.DORotate(new Vector3(0, 0, Random.Range(-15f, 15f)), 1.5f)
        .SetEase(Ease.OutQuad);
}


    public void SetAndPlayBackgroundMusic(AudioClip newClip, float newVolume = -1f)
    {
        if (newClip == null || newClip == currentMusic)
            return;

        currentMusic = newClip;
        musicStarted = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayBackgroundMusic(currentMusic);
            if (newVolume >= 0f)
            {
                musicVolume = Mathf.Clamp01(newVolume);
                AudioManager.Instance.SetMusicVolume(musicVolume);
            }
        }
    }
}

