using UnityEngine;
using System.Collections;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Pause Settings")]
    public AudioLowPassFilter lowPassFilter;
    public float pausedCutoff = 800f;
    public float normalCutoff = 22000f;

    [Header("Ambient Settings")]
    public AudioSource ambientSource;
    public float ambientFadeDuration = 1.5f;

    [Header("Settings")]
    public float fadeDuration = 1.0f;

    [Header("Footsteps Settings")]
    public float stepPitchMin = 0.9f;
    public float stepPitchMax = 1.1f;
    public float stepVolumeMin = 0.8f;
    public float stepVolumeMax = 1.0f;

    [Header("Optimization")]
    public float coinSoundInterval = 0.05f;
    private float _nextCoinSoundTime;



    [Header("Coin Combo Settings")]
    public float pitchStep = 0.05f;
    public float maxPitch = 1.5f;
    public float comboResetTime = 0.5f;
    private float _currentCoinPitch = 1.0f;
    private float _lastCoinTime;

    [SerializeField][Range(0f, 1f)] private float _fullMusicVolume = 0.1f;
    private Coroutine _musicCoroutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        bool musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        bool sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        musicSource.mute = !musicEnabled;
        if (ambientSource != null) ambientSource.mute = !musicEnabled;
        sfxSource.mute = !sfxEnabled;

        if (ambientSource != null)
        {
            ambientSource.loop = true;
            ambientSource.Play();
        }
    }

    public void PlayCoinSound(AudioClip clip, float volume = 1f)
    {
        if (clip == null || sfxSource == null) return;

        if (Time.time < _nextCoinSoundTime) return;

        if (Time.time - _lastCoinTime > comboResetTime)
        {
            _currentCoinPitch = 1.0f;
        }
        else
        {
            _currentCoinPitch = Mathf.Min(_currentCoinPitch + pitchStep, maxPitch);
        }

        _lastCoinTime = Time.time;
        _nextCoinSoundTime = Time.time + coinSoundInterval;

        sfxSource.pitch = _currentCoinPitch;
        sfxSource.PlayOneShot(clip, volume);

        StartCoroutine(ResetPitchRoutine());
    }

    private IEnumerator ResetPitchRoutine()
    {
        yield return null;
        if (Time.time - _lastCoinTime > 0.01f)
        {
            sfxSource.pitch = 1.0f;
        }
    }

    public void PauseGameSound()
    {
        if (lowPassFilter != null) lowPassFilter.cutoffFrequency = pausedCutoff;
    }

    public void ResumeGameSound()
    {
        if (lowPassFilter != null) lowPassFilter.cutoffFrequency = normalCutoff;
    }

    public void PlayMusicWithFade(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.Play();
        StartCoroutine(FadeIn(musicSource, fadeDuration));
    }

    public void SwitchFromAmbientToMusic(AudioClip intro, AudioClip loop)
    {
        if (_musicCoroutine != null) StopCoroutine(_musicCoroutine);
        _musicCoroutine = StartCoroutine(CrossFadeAmbientToMusic(intro, loop));
    }

    public void StartAmbientWithFade()
    {
        if (ambientSource != null)
        {
            ambientSource.Play();
            StartCoroutine(FadeIn(ambientSource, ambientFadeDuration));
        }
    }

    IEnumerator CrossFadeAmbientToMusic(AudioClip intro, AudioClip loop)
    {
        float t = 0;
        float startAmbientVol = ambientSource != null ? ambientSource.volume : 0;

        AudioClip firstClip = (intro != null) ? intro : loop;
        if (firstClip == null) yield break;

        musicSource.clip = firstClip;
        musicSource.volume = 0;
        musicSource.loop = (intro == null);
        musicSource.Play();

        while (t < ambientFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float norm = t / ambientFadeDuration;

            if (ambientSource != null) ambientSource.volume = Mathf.Lerp(startAmbientVol, 0, norm);
            musicSource.volume = Mathf.Lerp(0, _fullMusicVolume, norm);
            yield return null;
        }

        if (ambientSource != null) ambientSource.Stop();
        musicSource.volume = _fullMusicVolume;

        if (intro != null && loop != null)
        {
            while (musicSource.isPlaying && musicSource.time < (intro.length - 0.1f))
            {
                if (musicSource.clip != intro) yield break;
                yield return null;
            }

            if (musicSource.clip == intro)
            {
                musicSource.clip = loop;
                musicSource.loop = true;
                musicSource.Play();
                Debug.Log("Music switched to Loop");
            }
        }
    }

    public void StopMusicWithFade()
    {
        if (_musicCoroutine != null) StopCoroutine(_musicCoroutine);
        _musicCoroutine = StartCoroutine(FadeOut(musicSource, fadeDuration));
    }

    public void PlaySFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.pitch = 1.0f;
            sfxSource.PlayOneShot(clip, volume);
        }
    }
    public void PlayVariableSFX(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
        {
            float pitchRange = clip.name.Contains("step") ? 0.15f : 0.05f;
            sfxSource.pitch = UnityEngine.Random.Range(1f - pitchRange, 1f + pitchRange);
            sfxSource.PlayOneShot(clip, volume);

            StartCoroutine(ResetPitchAfterFrame());
        }
    }

    private IEnumerator ResetPitchAfterFrame()
    {
        yield return null;
        if (Time.time - _lastCoinTime > 0.01f)
        {
            sfxSource.pitch = 1.0f;
        }
    }

    private IEnumerator FadeIn(AudioSource source, float duration)
    {
        float t = 0;
        source.volume = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(0, _fullMusicVolume, t / duration);
            yield return null;
        }
    }

    private IEnumerator FadeOut(AudioSource source, float duration)
    {
        float t = 0;
        float startVolume = source.volume;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0, t / duration);
            yield return null;
        }
        source.Stop();
        source.volume = startVolume;
    }

    public void ToggleMusic(bool isOn)
    {
        musicSource.mute = !isOn;
        if (ambientSource != null) ambientSource.mute = !isOn;
        PlayerPrefs.SetInt("MusicEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void ToggleSFX(bool isOn)
    {
        sfxSource.mute = !isOn;
        PlayerPrefs.SetInt("SFXEnabled", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void StartAmbientQuietly(float targetVolume)
    {
        if (ambientSource != null)
        {
            ambientSource.volume = 0;
            ambientSource.Play();
            StartCoroutine(FadeVolume(ambientSource, targetVolume, 2.0f));
        }
    }

    private IEnumerator FadeVolume(AudioSource source, float targetVol, float duration)
    {
        float t = 0;
        float startVol = source.volume;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(startVol, targetVol, t / duration);
            yield return null;
        }
        source.volume = targetVol;
    }
    public void ResumeMusicAfterRevive(AudioClip intro, AudioClip loop)
    {
        if (_musicCoroutine != null) StopCoroutine(_musicCoroutine);

        _musicCoroutine = StartCoroutine(CrossFadeAmbientToMusic(intro, loop));
    }
}