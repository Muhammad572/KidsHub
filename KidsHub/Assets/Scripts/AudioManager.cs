// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// /// <summary>
// /// Singleton class to manage background music and sound effects (SFX).
// /// </summary>
// public class AudioManager : MonoBehaviour
// {
//     public static AudioManager Instance { get; private set; }

//     [Header("Audio Sources")]
//     public AudioSource musicSource;
//     public AudioSource sfxSource;

//     private void Awake()
//     {
//         // Singleton setup
//         if (Instance == null)
//         {
//             Instance = this;
//             DontDestroyOnLoad(gameObject);
//         }
//         else
//         {
//             Destroy(gameObject);
//             return;
//         }

//         // Ensure audio sources exist
//         if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
//         if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

//         musicSource.loop = true;
//         sfxSource.playOnAwake = false;
//     }

//     /// <summary>
//     /// Starts playing the background music.
//     /// </summary>
//     // public void PlayBackgroundMusic(AudioClip clip)
//     // {
//     //     if (musicSource != null && clip != null)
//     //     {
//     //         musicSource.clip = clip;
//     //         if (!musicSource.isPlaying)
//     //         {
//     //             musicSource.Play();
//     //         }
//     //     }
//     // }

//     public void PlayBackgroundMusic(AudioClip clip)
//     {
//         if (musicSource == null) return;

//         // 🧠 Stop any currently playing background music
//         if (musicSource.isPlaying)
//             musicSource.Stop();

//         if (clip == null)
//         {
//             Debug.LogWarning("AudioManager: No background music clip provided.");
//             return;
//         }

//         musicSource.clip = clip;
//         musicSource.loop = true;
//         musicSource.Play();
//     }


//     /// <summary>
//     /// Plays a one-shot sound effect using the main SFX source.
//     /// </summary>
//     public void PlaySFX(AudioClip clip)
//     {
//         // Calls the overloaded version with default volume 1.0f
//         PlaySFX(clip, 1f);
//     }

//     /// <summary>
//     /// Plays a one-shot sound effect using the main SFX source with a specified volume.
//     /// This fixes the "No overload for method 'PlaySFX' takes 2 arguments" error.
//     /// </summary>
//     public void PlaySFX(AudioClip clip, float volume)
//     {
//         if (sfxSource != null && clip != null)
//         {
//             // PlayOneShot accepts volume as a second argument
//             sfxSource.PlayOneShot(clip, volume);
//         }
//     }
    
//     /// <summary>
//     /// Plays a sound effect using a temporary, dedicated AudioSource.
//     /// This fixes the missing 'PlaySFXSeparate' definition error (CS1061).
//     /// Useful for ensuring sounds don't interrupt each other.
//     /// </summary>
//     public void PlaySFXSeparate(AudioClip clip, float volume = 1f)
//     {
//         if (clip == null) return;

//         // Create a temporary GameObject with an AudioSource
//         GameObject tempAudio = new GameObject($"TempSFX_{clip.name}");
//         AudioSource tempSource = tempAudio.AddComponent<AudioSource>();

//         tempSource.clip = clip;
//         tempSource.volume = volume;
//         tempSource.spatialBlend = 0; // 2D sound

//         // Optionally inherit the mixer group from the main SFX source
//         if (sfxSource != null)
//         {
//             tempSource.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
//         }

//         tempSource.Play();

//         // Destroy the GameObject after the clip finishes
//         Destroy(tempAudio, clip.length);
//     }

//     /// <summary>
//     /// Sets the volume for the background music.
//     /// </summary>
//     public void SetMusicVolume(float volume)
//     {
//         if (musicSource != null)
//         {
//             musicSource.volume = volume;
//         }
//     }

//     public bool IsMusicPlaying() => musicSource != null && musicSource.isPlaying;

//     public void StopAllMusic()
//     {
//         if (musicSource != null && musicSource.isPlaying)
//             musicSource.Stop();
//     }

//     public void FadeOutAndStopMusic(float duration = 1f)
//     {
//         StartCoroutine(FadeOutMusicRoutine(duration));
//     }

//     private IEnumerator FadeOutMusicRoutine(float duration)
//     {
//         if (musicSource == null || !musicSource.isPlaying) yield break;

//         float startVolume = musicSource.volume;
//         float time = 0;

//         while (time < duration)
//         {
//             time += Time.deltaTime;
//             musicSource.volume = Mathf.Lerp(startVolume, 0, time / duration);
//             yield return null;
//         }

//         musicSource.Stop();
//         musicSource.volume = startVolume;
//     }


// }



using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

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
            return;
        }

        if (musicSource == null) musicSource = gameObject.AddComponent<AudioSource>();
        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();

        musicSource.loop = true;
        musicSource.spatialBlend = 0; // 2D
        sfxSource.spatialBlend = 0;   // 2D
        sfxSource.playOnAwake = false;
    }

    // ✅ BACKGROUND MUSIC with Fade In
    public void PlayBackgroundMusic(AudioClip clip)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;

        StartCoroutine(FadeInNewMusic(clip, 1f));
    }

    private IEnumerator FadeInNewMusic(AudioClip newClip, float fadeTime)
    {
        if (musicSource.isPlaying)
        {
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(musicSource.volume, 0f, t / fadeTime);
                yield return null;
            }
            musicSource.Stop();
        }

        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.Play();

        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(0f, 0.5f, t / fadeTime);
            yield return null;
        }
    }

    // ✅ PLAY SOUND EFFECT (Randomized Pitch)
    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.pitch = Random.Range(0.95f, 1.05f);
            sfxSource.PlayOneShot(clip, volume);
            sfxSource.pitch = 1f;
        }
    }

    public void PlaySFXSeparate(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;
        GameObject temp = new GameObject("TempSFX_" + clip.name);
        AudioSource tempSource = temp.AddComponent<AudioSource>();
        tempSource.clip = clip;
        tempSource.volume = volume;
        tempSource.spatialBlend = 0;
        tempSource.Play();
        Destroy(temp, clip.length);
    }

    // ✅ LAYERED POP + SPARKLE
    public void PlayLayeredSFX(AudioClip clip1, AudioClip clip2, float volume = 1f)
    {
        if (clip1 != null) PlaySFXSeparate(clip1, volume);
        if (clip2 != null) PlaySFXSeparate(clip2, volume * 0.8f);
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = Mathf.Clamp01(volume);
    }

    // ✅ Check if music is currently playing
    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    // ✅ Smooth Fade-Out and Stop Music
    public void StopAllMusic(float fadeTime = 0.8f)
    {
        if (musicSource == null || !musicSource.isPlaying) return;
        StartCoroutine(FadeOutAndStop(fadeTime));
    }

    private IEnumerator FadeOutAndStop(float fadeTime)
    {
        float startVol = musicSource.volume;
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVol, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.Stop();
        musicSource.clip = null;
        musicSource.volume = startVol; // reset for next play
    }
}
