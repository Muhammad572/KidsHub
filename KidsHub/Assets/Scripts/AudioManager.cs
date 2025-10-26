// using UnityEngine;

// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance;

//     [Header("Audio Sources")]
//     public AudioSource musicSource;
//     public AudioSource sfxSource;

//     [Header("Volumes")]
//     [Range(0f, 1f)] public float musicVolume = 0.6f;
//     [Range(0f, 1f)] public float sfxVolume = 1f;

//     private void Awake()
//     {
//         // if (Instance == null)
//         // {
//         //     Instance = this;
//         //     DontDestroyOnLoad(gameObject);
//         // }
//         // else
//         // {
//         //     Destroy(gameObject);
//         // }

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
//     }

//     // 🎵 Background music control
//     // public void PlayBackgroundMusic(AudioClip clip)
//     // {
//     //     if (clip == null) return;
//     //     musicSource.clip = clip;
//     //     musicSource.volume = musicVolume;
//     //     musicSource.Play();
//     // }

//     public void PlayBackgroundMusic(AudioClip music)
//     {
//         if (musicSource == null)
//         {
//             musicSource = gameObject.AddComponent<AudioSource>();
//             musicSource.loop = true;
//             musicSource.playOnAwake = false;
//         }

//         musicSource.Stop();
//         musicSource.clip = music;
//         musicSource.volume = musicVolume;
//         musicSource.pitch = 1f; // ✅ Reset pitch
//         musicSource.Play();
//     }


//     public void StopBackgroundMusic()
//     {
//         musicSource.Stop();
//     }

//     public void SetMusicVolume(float volume)
//     {
//         musicVolume = Mathf.Clamp01(volume);
//         musicSource.volume = musicVolume;
//     }

//     // 🔊 Play SFX (simple)
//     public void PlaySFX(AudioClip clip)
//     {
//         if (clip == null) return;
//         sfxSource.PlayOneShot(clip, sfxVolume);
//     }

//     // 🔊 Play SFX with custom volume
//     // public void PlaySFX(AudioClip clip, float volume)
//     // {
//     //     if (clip == null) return;
//     //     sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
//     // }

//     public void PlaySFX(AudioClip clip, float volume = 1f)
//     {
//         Debug.Log($"🎵 Playing {clip.name} at pitch {sfxSource.pitch} and volume {sfxSource.volume}");

//         if (clip == null) return;

//         if (sfxSource == null)
//         {
//             sfxSource = gameObject.AddComponent<AudioSource>();
//             sfxSource.playOnAwake = false;
//             sfxSource.loop = false;
//         }

//         sfxSource.Stop();
//         sfxSource.clip = clip;
//         sfxSource.volume = sfxVolume * volume;
//         sfxSource.pitch = 1f; // ✅ VERY IMPORTANT — prevent fast playback
//         sfxSource.Play();
//     }


//     // 🔊 Play SFX on a temporary AudioSource (isolated channel)
//     public void PlaySFXSeparate(AudioClip clip, float volume = 1f)
//     {
//         if (clip == null) return;

//         GameObject tempGO = new GameObject("TempSFX_" + clip.name);
//         AudioSource tempSource = tempGO.AddComponent<AudioSource>();
//         tempSource.clip = clip;
//         tempSource.volume = Mathf.Clamp01(volume * sfxVolume);
//         tempSource.Play();

//         Destroy(tempGO, clip.length);
//     }

//     // 🔊 Change overall SFX volume live
//     public void SetSFXVolume(float volume)
//     {
//         sfxVolume = Mathf.Clamp01(volume);
//     }
// }

using UnityEngine;

/// <summary>
/// Singleton class to manage background music and sound effects (SFX).
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure audio sources exist
        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        sfxSource.playOnAwake = false;
    }

    /// <summary>
    /// Starts playing the background music.
    /// </summary>
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            musicSource.clip = clip;
            if (!musicSource.isPlaying)
            {
                musicSource.Play();
            }
        }
    }

    /// <summary>
    /// Plays a one-shot sound effect using the main SFX source.
    /// </summary>
    public void PlaySFX(AudioClip clip)
    {
        // Calls the overloaded version with default volume 1.0f
        PlaySFX(clip, 1f);
    }

    /// <summary>
    /// Plays a one-shot sound effect using the main SFX source with a specified volume.
    /// This fixes the "No overload for method 'PlaySFX' takes 2 arguments" error.
    /// </summary>
    public void PlaySFX(AudioClip clip, float volume)
    {
        if (sfxSource != null && clip != null)
        {
            // PlayOneShot accepts volume as a second argument
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    
    /// <summary>
    /// Plays a sound effect using a temporary, dedicated AudioSource.
    /// This fixes the missing 'PlaySFXSeparate' definition error (CS1061).
    /// Useful for ensuring sounds don't interrupt each other.
    /// </summary>
    public void PlaySFXSeparate(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // Create a temporary GameObject with an AudioSource
        GameObject tempAudio = new GameObject($"TempSFX_{clip.name}");
        AudioSource tempSource = tempAudio.AddComponent<AudioSource>();

        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.spatialBlend = 0; // 2D sound

        // Optionally inherit the mixer group from the main SFX source
        if (sfxSource != null)
        {
            tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }

        tempSource.Play();

        // Destroy the GameObject after the clip finishes
        Destroy(tempAudio, clip.length);
    }

    /// <summary>
    /// Sets the volume for the background music.
    /// </summary>
    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = volume;
        }
    }

    public bool IsMusicPlaying() => musicSource != null && musicSource.isPlaying;
}

