using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Music Clips")]
    public AudioClip backgroundMusic;

    [Header("Settings")]
    [Range(0f, 1f)] public float musicVolume = 0.8f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }

        PlayBackgroundMusic();
    }

    private void Start()
    {
        UpdateVolumes();
    }

    public void PlayBackgroundMusic(AudioClip clip = null)
    {
        if (clip != null)
            backgroundMusic = clip;

        if (backgroundMusic == null)
            return;

        if (musicSource.isPlaying && musicSource.clip == backgroundMusic)
            return; // already playing same clip

        musicSource.clip = backgroundMusic;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    public void StopMusic() => musicSource.Stop();

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        UpdateVolumes();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        UpdateVolumes();
    }

    private void UpdateVolumes()
    {
        musicSource.volume = musicVolume;
        sfxSource.volume = sfxVolume;
    }

    public void Mute(bool mute)
    {
        musicSource.mute = mute;
        sfxSource.mute = mute;
    }
}
