using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("SFX Clips")]
    public AudioClip ballSwishClip;
    public AudioClip rimBounceClip;
    public AudioClip comboFanfareClip;
    public AudioClip levelUpJingleClip;
    public AudioClip countdownBeepClip;
    public AudioClip buttonClickClip;
    public AudioClip gameOverStingClip;
    public AudioClip crowdCheerClip; // Fix: Reference CrowdCheer.wav to eliminate dead asset overhead

    [Header("Music Loops")]
    public AudioClip dayLoop;
    public AudioClip duskLoop;
    public AudioClip nightLoop;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = 0.5f;
        }
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = 0.8f;
        }
    }

    public void PlaySFX(AudioClip clip, float pitchRange = 0f)
    {
        if (clip == null || sfxSource == null) return;

        if (pitchRange > 0f)
        {
            sfxSource.pitch = Random.Range(1f - pitchRange, 1f + pitchRange);
        }
        else
        {
            sfxSource.pitch = 1f;
        }

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySwish()
    {
        PlaySFX(ballSwishClip, 0.15f); // Random pitch variation on swish
    }

    public void PlayRimBounce()
    {
        PlaySFX(rimBounceClip, 0.1f);
    }

    public void PlayComboFanfare()
    {
        PlaySFX(comboFanfareClip);
    }

    public void PlayLevelUp()
    {
        PlaySFX(levelUpJingleClip);
    }

    public void PlayCountdownBeep()
    {
        PlaySFX(countdownBeepClip);
    }

    public void PlayButtonClick()
    {
        PlaySFX(buttonClickClip);
    }

    public void PlayGameOver()
    {
        PlaySFX(gameOverStingClip);
    }

    public void PlayMusicForLevel(int level)
    {
        AudioClip targetClip = dayLoop;
        if (level >= 4)
        {
            targetClip = nightLoop;
        }
        else if (level >= 2)
        {
            targetClip = duskLoop;
        }

        if (musicSource != null && musicSource.clip != targetClip)
        {
            StartCoroutine(TransitionMusic(targetClip));
        }
    }

    private System.Collections.IEnumerator TransitionMusic(AudioClip newClip)
    {
        float fadeTime = 0.5f;
        float startVolume = musicSource.volume;

        // Fade out
        for (float t = 0; t < fadeTime; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }
        musicSource.volume = 0f;
        musicSource.clip = newClip;

        if (newClip != null)
        {
            musicSource.Play();
            // Fade in
            for (float t = 0; t < fadeTime; t += Time.deltaTime)
            {
                musicSource.volume = Mathf.Lerp(0f, startVolume, t / fadeTime);
                yield return null;
            }
            musicSource.volume = startVolume;
        }
    }
}
