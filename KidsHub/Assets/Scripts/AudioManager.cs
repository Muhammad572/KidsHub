// using UnityEngine;
// using UnityEngine.Audio;

// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance;

//     [Header("Audio Sources")]
//     public AudioSource musicSource;
//     public AudioSource sfxSource;

//     [Header("Music Clips")]
//     public AudioClip backgroundMusic;

//     [Header("Settings")]
//     [Range(0f, 1f)] public float musicVolume = 0.8f;
//     [Range(0f, 1f)] public float sfxVolume = 1f;

//     private void Awake()
//     {
//         if (Instance != null && Instance != this)
//         {
//             Destroy(gameObject);
//             return;
//         }

//         Instance = this;
//         DontDestroyOnLoad(gameObject);

//         if (musicSource == null)
//         {
//             musicSource = gameObject.AddComponent<AudioSource>();
//             musicSource.loop = true;
//         }

//         if (sfxSource == null)
//         {
//             sfxSource = gameObject.AddComponent<AudioSource>();
//         }

//         PlayBackgroundMusic();
//     }

//     private void Start()
//     {
//         UpdateVolumes();
//     }

//     public void PlayBackgroundMusic(AudioClip clip = null)
//     {
//         if (clip != null)
//             backgroundMusic = clip;

//         if (backgroundMusic == null)
//             return;

//         if (musicSource.isPlaying && musicSource.clip == backgroundMusic)
//             return; // already playing same clip

//         musicSource.clip = backgroundMusic;
//         musicSource.loop = true;
//         musicSource.Play();
//     }

//     public void PlaySFX(AudioClip clip)
//     {
//         if (clip == null) return;
//         sfxSource.PlayOneShot(clip, sfxVolume);
//     }

//     public void StopMusic() => musicSource.Stop();

//     public void SetMusicVolume(float value)
//     {
//         musicVolume = Mathf.Clamp01(value);
//         UpdateVolumes();
//     }

//     public void SetSFXVolume(float value)
//     {
//         sfxVolume = Mathf.Clamp01(value);
//         UpdateVolumes();
//     }

//     private void UpdateVolumes()
//     {
//         musicSource.volume = musicVolume;
//         sfxSource.volume = sfxVolume;
//     }

//     public void Mute(bool mute)
//     {
//         musicSource.mute = mute;
//         sfxSource.mute = mute;
//     }
// }


using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volumes")]
    [Range(0f, 1f)] public float musicVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
        }
    }

    // 🎵 Background music control
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopBackgroundMusic()
    {
        musicSource.Stop();
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        musicSource.volume = musicVolume;
    }

    // 🔊 Play SFX (simple)
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, sfxVolume);
    }

    // 🔊 Play SFX with custom volume
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    // 🔊 Play SFX on a temporary AudioSource (isolated channel)
    public void PlaySFXSeparate(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        GameObject tempGO = new GameObject("TempSFX_" + clip.name);
        AudioSource tempSource = tempGO.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = Mathf.Clamp01(volume * sfxVolume);
        tempSource.Play();

        Destroy(tempGO, clip.length);
    }

    // 🔊 Change overall SFX volume live
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
}
