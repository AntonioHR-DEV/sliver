using System;
using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Singleton that handles all game audio.
/// Routes music through the Music mixer group and SFX through the SFX mixer group.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SfxVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip jumpNormalClip;
    [SerializeField] private AudioClip jumpWallClip;
    [SerializeField] private AudioClip hurtClip;
    [SerializeField] private AudioClip checkpointClip;
    [SerializeField] private AudioClip achievementClip;
    [SerializeField] private AudioClip respawnClip;
    [SerializeField] private AudioClip startClip;

    [Header("Music Clip")]
    [SerializeField] private AudioClip musicClip;

    // Volume is stored as a 0-10 int step for the button-based UI
    private const int VOLUME_STEPS = 10;
    private const float VOLUME_STEP_SIZE = 1f / VOLUME_STEPS;

    private float musicVolume;
    private float sfxVolume;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        LoadVolumeSettings();
        PlayMusic(musicClip);
    }

    private void LoadVolumeSettings()
    {
        if (SaveSystem.Instance == null) return;

        SaveSystem.SettingsData settings = SaveSystem.Instance.GetSettings();
        musicVolume = settings.musicVolume;
        sfxVolume = settings.sfxVolume;

        ApplyMusicVolume();
        ApplySfxVolume();
    }

    /// <summary>Converts a 0-1 linear float to decibels and applies it to the mixer.</summary>
    private void ApplyMusicVolume()
    {
        audioMixer.SetFloat(MUSIC_VOLUME_PARAM, LinearToDecibel(musicVolume));
    }

    private void ApplySfxVolume()
    {
        audioMixer.SetFloat(SFX_VOLUME_PARAM, LinearToDecibel(sfxVolume));
    }

    private float LinearToDecibel(float linear)
    {
        // Clamp to avoid log(0) which is -infinity
        linear = Mathf.Clamp(linear, 0.0001f, 1f);
        return Mathf.Log10(linear) * 20f;
    }

    public void IncreaseMusicVolume()
    {
        musicVolume = Mathf.Clamp01(musicVolume + VOLUME_STEP_SIZE);
        ApplyMusicVolume();
        SaveSystem.Instance?.SetMusicVolume(musicVolume);
    }

    public void DecreaseMusicVolume()
    {
        musicVolume = Mathf.Clamp01(musicVolume - VOLUME_STEP_SIZE);
        ApplyMusicVolume();
        SaveSystem.Instance?.SetMusicVolume(musicVolume);
    }

    public void IncreaseSfxVolume()
    {
        sfxVolume = Mathf.Clamp01(sfxVolume + VOLUME_STEP_SIZE);
        ApplySfxVolume();
        SaveSystem.Instance?.SetSfxVolume(sfxVolume);
    }

    public void DecreaseSfxVolume()
    {
        sfxVolume = Mathf.Clamp01(sfxVolume - VOLUME_STEP_SIZE);
        ApplySfxVolume();
        SaveSystem.Instance?.SetSfxVolume(sfxVolume);
    }

    /// <summary>Returns current music volume as a 0-10 int for displaying in the UI.</summary>
    public int GetMusicVolumeStep() => Mathf.RoundToInt(musicVolume * VOLUME_STEPS);

    /// <summary>Returns current SFX volume as a 0-10 int for displaying in the UI.</summary>
    public int GetSfxVolumeStep() => Mathf.RoundToInt(sfxVolume * VOLUME_STEPS);

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlayJumpNormal() => PlaySFX(jumpNormalClip);
    public void PlayJumpWall() => PlaySFX(jumpWallClip);
    public void PlayHurt() => PlaySFX(hurtClip);
    public void PlayCheckpoint() => PlaySFX(checkpointClip);
    public void PlayRespawn() => PlaySFX(respawnClip);
    public void PlayAchievement() => PlaySFX(achievementClip);
    public void PlayStart() => PlaySFX(startClip);

    private void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}