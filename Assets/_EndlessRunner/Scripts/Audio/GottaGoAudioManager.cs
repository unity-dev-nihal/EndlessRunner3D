using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GottaGoAudioManager : MonoBehaviour
{
    public static GottaGoAudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip gameplayMusic;

    [Header("SFX")]
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip objectHitSound;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Mixer Groups")]
    [SerializeField] private AudioMixerGroup musicGroup;
    [SerializeField] private AudioMixerGroup sfxGroup;

    [Header("SFX Volume")]
    [SerializeField, Range(0f, 1f)]
    private float sfxSourceVolume = 1f;

    [Header("Scene Names")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string gameplaySceneName = "RunnerScene";

    private AudioSource musicSource;
    private AudioSource sfxSource;

    private bool isInitialized;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        CreateAudioSources();
        SetupMixerGroups();

        isInitialized = true;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        PlayMusicForCurrentScene();
    }

    private void CreateAudioSources()
    {
        musicSource = gameObject.AddComponent<AudioSource>();

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = 1f;

        sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = Mathf.Clamp01(sfxSourceVolume);
        sfxSource.ignoreListenerPause = true;
    }

    private void SetupMixerGroups()
    {
        if (audioMixer == null)
        {
            Debug.LogWarning(
                "GottaGoAudioManager: Audio Mixer is not assigned."
            );
        }
        else
        {
            if (musicGroup == null)
            {
                AudioMixerGroup[] groups =
                    audioMixer.FindMatchingGroups("Music");

                if (groups.Length > 0)
                    musicGroup = groups[0];
            }

            if (sfxGroup == null)
            {
                AudioMixerGroup[] groups =
                    audioMixer.FindMatchingGroups("SFX");

                if (groups.Length > 0)
                    sfxGroup = groups[0];
            }
        }

        if (musicGroup != null)
        {
            musicSource.outputAudioMixerGroup = musicGroup;
        }
        else
        {
            Debug.LogWarning(
                "GottaGoAudioManager: Music mixer group could not be found."
            );
        }

        if (sfxGroup != null)
        {
            sfxSource.outputAudioMixerGroup = sfxGroup;
        }
        else
        {
            Debug.LogWarning(
                "GottaGoAudioManager: SFX mixer group could not be found."
            );
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!isInitialized)
            return;

        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        PlayMusicForScene(currentScene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip clipToPlay = null;

        if (sceneName == mainMenuSceneName)
        {
            clipToPlay = mainMenuMusic;
        }
        else if (sceneName == gameplaySceneName)
        {
            clipToPlay = gameplayMusic;
        }

        if (clipToPlay == null)
        {
            Debug.LogWarning(
                "GottaGoAudioManager: No music assigned for scene '" +
                sceneName +
                "'."
            );

            StopMusic();
            return;
        }

        if (musicSource.clip == clipToPlay && musicSource.isPlaying)
            return;

        musicSource.clip = clipToPlay;
        musicSource.Play();

        Debug.Log(
            "GottaGoAudioManager: Playing music for scene '" +
            sceneName +
            "'."
        );
    }

    // ---------------------------------------------------------
    // SFX
    // ---------------------------------------------------------

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickSound, "Button Click");
    }

    public void PlayCoinSound()
    {
        PlaySFX(coinSound, "Coin");
    }

    public void PlayObjectHitSound()
    {
        PlaySFX(objectHitSound, "Object Hit");
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, "Custom SFX");
    }

    private void PlaySFX(AudioClip clip, string soundName)
    {
        if (clip == null)
        {
            Debug.LogWarning(
                "GottaGoAudioManager: " +
                soundName +
                " SFX clip is not assigned."
            );

            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError(
                "GottaGoAudioManager: SFX AudioSource does not exist."
            );

            return;
        }

        if (sfxSource.outputAudioMixerGroup == null)
        {
            Debug.LogWarning(
                "GottaGoAudioManager: SFX AudioSource has no mixer group assigned."
            );
        }

        sfxSource.volume = Mathf.Clamp01(sfxSourceVolume);

        sfxSource.PlayOneShot(clip);

        Debug.Log(
            "GottaGoAudioManager: Playing SFX -> " +
            soundName +
            " | Clip: " +
            clip.name
        );
    }

    // ---------------------------------------------------------
    // Music Controls
    // ---------------------------------------------------------

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        musicSource.clip = null;
    }

    public void PauseMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Pause();
    }

    public void ResumeMusic()
    {
        if (musicSource == null)
            return;

        if (musicSource.clip != null)
            musicSource.UnPause();
    }

    public bool IsMusicPlaying()
    {
        return musicSource != null && musicSource.isPlaying;
    }

    // ---------------------------------------------------------
    // Getters
    // ---------------------------------------------------------

    public AudioClip GetMainMenuMusic()
    {
        return mainMenuMusic;
    }

    public AudioClip GetGameplayMusic()
    {
        return gameplayMusic;
    }

    // ---------------------------------------------------------
    // Cleanup
    // ---------------------------------------------------------

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
}