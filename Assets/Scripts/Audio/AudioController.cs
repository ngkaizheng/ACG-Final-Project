using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    [Header("Audio Sources")]
    private List<AudioSource> musicSources;          // Pool for music
    private List<AudioSource> soundEffectSources;    // Pool for SFX

    [Header("Audio Mappings")]
    [SerializeField] private List<SoundEffectMapping> soundEffectMappings = new List<SoundEffectMapping>();
    [SerializeField] private List<MusicTrackMapping> musicTrackMappings = new List<MusicTrackMapping>();

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float musicVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    private Dictionary<SoundEffect, AudioClip> soundEffectDictionary;
    private Dictionary<MusicTrack, AudioClip> musicDictionary;

    // Keys for PlayerPrefs
    public const string MusicVolumeKey = "MusicVolume";
    public const string SFXVolumeKey = "SFXVolume";

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

        // Initialize collections
        musicSources = new List<AudioSource>();
        soundEffectSources = new List<AudioSource>();

        soundEffectDictionary = new Dictionary<SoundEffect, AudioClip>();
        musicDictionary = new Dictionary<MusicTrack, AudioClip>();

        // Populate sound effect dictionary from mappings
        foreach (var mapping in soundEffectMappings)
        {
            if (mapping.clip != null)
            {
                soundEffectDictionary[mapping.effect] = mapping.clip;
            }
            else
            {
                Debug.LogWarning($"AudioClip for SoundEffect '{mapping.effect}' is not assigned!");
            }
        }

        // Populate music dictionary from mappings
        foreach (var mapping in musicTrackMappings)
        {
            if (mapping.clip != null)
            {
                musicDictionary[mapping.track] = mapping.clip;
            }
            else
            {
                Debug.LogWarning($"AudioClip for MusicTrack '{mapping.track}' is not assigned!");
            }
        }

        // Create initial music source pool
        for (int i = 0; i < musicTrackMappings.Count; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            musicSources.Add(source);
        }

        // Create sound effect pool
        for (int i = 0; i < soundEffectMappings.Count; i++)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            soundEffectSources.Add(source);
        }

        // Load saved volume settings or use defaults
        float savedMusicVolume = PlayerPrefs.GetFloat(MusicVolumeKey, musicVolume);
        float savedSFXVolume = PlayerPrefs.GetFloat(SFXVolumeKey, sfxVolume);
        SetMusicVolume(savedMusicVolume);
        SetSFXVolume(savedSFXVolume);
    }

    private void Start()
    {
    }

    #region PlayMusic
    // Play background music by enum
    public void PlayMusic(MusicTrack track)
    {
        if (musicDictionary.TryGetValue(track, out AudioClip clip))
        {
            AudioSource availableSource = GetAvailableMusicSource();
            if (availableSource != null)
            {
                availableSource.clip = clip;
                availableSource.volume = musicVolume;
                availableSource.Play();
            }
        }
        else
        {
            Debug.LogWarning($"MusicTrack '{track}' not found in dictionary!");
        }
    }

    // Play background music by AudioClip
    public void PlayMusic(AudioClip musicClip)
    {
        AudioSource availableSource = GetAvailableMusicSource();
        if (availableSource != null)
        {
            availableSource.clip = musicClip;
            availableSource.volume = musicVolume;
            availableSource.Play();
        }
    }
    #endregion

    #region PlaySoundEffect
    // Play a sound effect by name
    public void PlaySoundEffect(SoundEffect effect)
    {
        if (soundEffectDictionary.TryGetValue(effect, out AudioClip clip))
        {
            AudioSource availableSource = GetAvailableSource();
            if (availableSource != null)
            {
                availableSource.volume = sfxVolume;
                availableSource.PlayOneShot(clip);
            }
        }
        else
        {
            Debug.LogWarning($"SoundEffect '{effect}' not found in dictionary!");
        }
    }

    // Play a sound effect by enum using a specified AudioSource (for spatial audio)
    public void PlaySoundEffect(SoundEffect effect, AudioSource audioSource, bool stopCurrent = false, bool is3D = true)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("Provided AudioSource is null!");
            return;
        }

        if (soundEffectDictionary.TryGetValue(effect, out AudioClip clip))
        {
            if (stopCurrent && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            audioSource.volume = sfxVolume;
            if (is3D)
                audioSource.spatialBlend = 1f; // Ensure 3D audio
            audioSource.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning($"SoundEffect '{effect}' not found in dictionary!");
        }
    }

    // Play a sound effect at a specific position (for short-lived objects)
    public void PlaySoundEffectAtPosition(SoundEffect effect, Vector3 position, bool is3D = true)
    {
        if (soundEffectDictionary.TryGetValue(effect, out AudioClip clip))
        {
            GameObject tempAudioObject = new GameObject($"TempAudio_{effect}");
            tempAudioObject.transform.position = position;
            AudioSource tempSource = tempAudioObject.AddComponent<AudioSource>();
            tempSource.volume = sfxVolume;
            if (is3D)
                tempSource.spatialBlend = 1f; // 3D audio
            tempSource.maxDistance = 10f; // Adjust as needed
            tempSource.rolloffMode = AudioRolloffMode.Logarithmic;
            tempSource.PlayOneShot(clip);
            Destroy(tempAudioObject, clip.length + 0.1f); // Clean up after clip finishes
        }
        else
        {
            Debug.LogWarning($"SoundEffect '{effect}' not found in dictionary!");
        }
    }

    // Play a sound effect by AudioClip
    public void PlaySoundEffect(AudioClip clip)
    {
        AudioSource availableSource = GetAvailableSource();
        if (availableSource != null)
        {
            availableSource.volume = sfxVolume;
            availableSource.PlayOneShot(clip);
        }
    }
    #endregion

    // Get an available AudioSource from the music pool
    private AudioSource GetAvailableMusicSource()
    {
        foreach (AudioSource source in musicSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If all music sources are busy, create a new one
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.loop = true;
        newSource.playOnAwake = false;
        musicSources.Add(newSource);
        return newSource;
    }

    // Get an available AudioSource from the sound effect pool
    private AudioSource GetAvailableSource()
    {
        foreach (AudioSource source in soundEffectSources)
        {
            if (!source.isPlaying)
            {
                return source;
            }
        }

        // If all sources are busy, create a new one
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.playOnAwake = false;
        soundEffectSources.Add(newSource);
        return newSource;
    }

    // Volume control methods
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        foreach (AudioSource source in musicSources)
        {
            if (source.isPlaying)
            {
                source.volume = musicVolume;
            }
        }
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        foreach (AudioSource source in soundEffectSources)
        {
            if (source.isPlaying)
            {
                source.volume = sfxVolume;
            }
        }
    }

    // Stop all audio
    public void StopAllAudio()
    {
        foreach (AudioSource source in musicSources)
        {
            source.Stop();
        }
        foreach (AudioSource source in soundEffectSources)
        {
            source.Stop();
        }
    }

    // // Get list of available music tracks
    // public List<string> GetMusicNames()
    // {
    //     return new List<string>(musicDictionary.Keys);
    // }

    // // Get list of available sound effects
    // public List<string> GetSoundEffectNames()
    // {
    //     return new List<string>(soundEffectDictionary.Keys);
    // }

    #region FadeInOutMusic
    public void FadeOutMusic(float duration)
    {
        foreach (AudioSource source in musicSources)
        {
            if (source.isPlaying)
            {
                StartCoroutine(FadeOut(source, duration));
            }
        }
    }

    public void FadeInMusic(MusicTrack track, float duration)
    {
        PlayMusic(track);
        AudioSource source = musicSources.Find(s => s.isPlaying && s.clip == musicDictionary[track]);
        if (source != null)
        {
            StartCoroutine(FadeIn(source, duration));
        }
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        source.Stop();
        source.volume = musicVolume;
    }

    private IEnumerator FadeIn(AudioSource source, float duration)
    {
        source.volume = 0;
        float targetVolume = musicVolume;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            source.volume = Mathf.Lerp(0, targetVolume, t / duration);
            yield return null;
        }
        source.volume = targetVolume;
    }
    #endregion
}

// Enum for sound effects
public enum SoundEffect
{
    Click,
    HurtPlayer1,
    HurtPlayer2,
    HurtPlayer3,
    HurtPlayer4,
    HurtPlayer5,
    DiePlayer,
    Footstep_Player1,
    Footstep_Player2,
    Footstep_Player3,
    Footstep_Player4,
    Footstep_Player5,
    Fire,
}

// Enum for music tracks
public enum MusicTrack
{
    LoginBGM,
    MainMenuBGM,
    LobbyTheme,
    BattleTheme,
    AmbientLoop,
    GameBGM,
    // Add more as needed
}

[System.Serializable]
public struct SoundEffectMapping
{
    public SoundEffect effect;
    public AudioClip clip;
}

[System.Serializable]
public struct MusicTrackMapping
{
    public MusicTrack track;
    public AudioClip clip;
}
