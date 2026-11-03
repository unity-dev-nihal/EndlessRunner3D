using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MainMenuSettings : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Volume Sliders")]
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;

    [Header("Vibration")]
    [SerializeField] private Toggle vibrationToggle;

    [Header("Default Settings")]
    [Range(0f, 1f)]
    [SerializeField] private float defaultMasterVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultMusicVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float defaultSFXVolume = 1f;

    [SerializeField] private bool defaultVibration = true;

    private const string MasterVolumeKey = "GottaGo_MasterVolume";
    private const string MusicVolumeKey = "GottaGo_MusicVolume";
    private const string SFXVolumeKey = "GottaGo_SFXVolume";
    private const string VibrationKey = "GottaGo_Vibration";

    private const string MasterMixerParameter = "MasterVolume";
    private const string MusicMixerParameter = "MusicVolume";
    private const string SFXMixerParameter = "SFXVolume";

    private void Awake()
    {
        LoadSettings();
    }

    private void Start()
    {
        RegisterListeners();
    }

    private void RegisterListeners()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.AddListener(SetSFXVolume);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.AddListener(SetVibration);
    }

    public void SetMasterVolume(float value)
    {
        value = Mathf.Clamp01(value);

        ApplyMixerVolume(MasterMixerParameter, value);

        PlayerPrefs.SetFloat(MasterVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetMusicVolume(float value)
    {
        value = Mathf.Clamp01(value);

        ApplyMixerVolume(MusicMixerParameter, value);

        PlayerPrefs.SetFloat(MusicVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp01(value);

        ApplyMixerVolume(SFXMixerParameter, value);

        PlayerPrefs.SetFloat(SFXVolumeKey, value);
        PlayerPrefs.Save();
    }

    public void SetVibration(bool enabled)
    {
        PlayerPrefs.SetInt(VibrationKey, enabled ? 1 : 0);
        PlayerPrefs.Save();

#if UNITY_ANDROID && !UNITY_EDITOR
        if (enabled)
            Handheld.Vibrate();
#endif
    }

    private void ApplyMixerVolume(string parameterName, float normalizedValue)
    {
        if (audioMixer == null)
        {
            Debug.LogWarning(
                "MainMenuSettings: Audio Mixer is not assigned."
            );

            return;
        }

        float decibels;

        if (normalizedValue <= 0.0001f)
        {
            decibels = -80f;
        }
        else
        {
            decibels = Mathf.Log10(normalizedValue) * 20f;
        }

        audioMixer.SetFloat(parameterName, decibels);
    }

    private void LoadSettings()
    {
        float masterVolume = PlayerPrefs.GetFloat(
            MasterVolumeKey,
            defaultMasterVolume
        );

        float musicVolume = PlayerPrefs.GetFloat(
            MusicVolumeKey,
            defaultMusicVolume
        );

        float sfxVolume = PlayerPrefs.GetFloat(
            SFXVolumeKey,
            defaultSFXVolume
        );

        bool vibrationEnabled = PlayerPrefs.GetInt(
            VibrationKey,
            defaultVibration ? 1 : 0
        ) == 1;

        masterVolume = Mathf.Clamp01(masterVolume);
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);

        if (masterVolumeSlider != null)
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.SetValueWithoutNotify(musicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);

        if (vibrationToggle != null)
            vibrationToggle.SetIsOnWithoutNotify(vibrationEnabled);

        ApplyMixerVolume(MasterMixerParameter, masterVolume);
        ApplyMixerVolume(MusicMixerParameter, musicVolume);
        ApplyMixerVolume(SFXMixerParameter, sfxVolume);
    }

    public bool IsVibrationEnabled()
    {
        return PlayerPrefs.GetInt(
            VibrationKey,
            defaultVibration ? 1 : 0
        ) == 1;
    }

    public float GetMasterVolume()
    {
        return PlayerPrefs.GetFloat(
            MasterVolumeKey,
            defaultMasterVolume
        );
    }

    public float GetMusicVolume()
    {
        return PlayerPrefs.GetFloat(
            MusicVolumeKey,
            defaultMusicVolume
        );
    }

    public float GetSFXVolume()
    {
        return PlayerPrefs.GetFloat(
            SFXVolumeKey,
            defaultSFXVolume
        );
    }

    private void OnDestroy()
    {
        if (masterVolumeSlider != null)
            masterVolumeSlider.onValueChanged.RemoveListener(SetMasterVolume);

        if (musicVolumeSlider != null)
            musicVolumeSlider.onValueChanged.RemoveListener(SetMusicVolume);

        if (sfxVolumeSlider != null)
            sfxVolumeSlider.onValueChanged.RemoveListener(SetSFXVolume);

        if (vibrationToggle != null)
            vibrationToggle.onValueChanged.RemoveListener(SetVibration);
    }
}