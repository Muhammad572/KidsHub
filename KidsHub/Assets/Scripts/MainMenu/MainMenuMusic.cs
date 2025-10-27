using UnityEngine;

public class MainMenuMusic : MonoBehaviour
{
    [Header("Main Menu Background Music")]
    public AudioClip mainMenuMusic;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    private void Start()
    {
        // ✅ Only play if AudioManager exists and clip is assigned
        if (AudioManager.Instance != null && mainMenuMusic != null)
        {
            // Prevent double playback
            if (!AudioManager.Instance.IsMusicPlaying())
            {
                AudioManager.Instance.PlayBackgroundMusic(mainMenuMusic);
                AudioManager.Instance.SetMusicVolume(musicVolume);
            }
            else if (AudioManager.Instance.IsMusicPlaying() &&
                     AudioManager.Instance.musicSource.clip != mainMenuMusic)
            {
                // If another scene's music is playing, smoothly switch to main menu music
                AudioManager.Instance.PlayBackgroundMusic(mainMenuMusic);
                AudioManager.Instance.SetMusicVolume(musicVolume);
            }
        }
    }
}
