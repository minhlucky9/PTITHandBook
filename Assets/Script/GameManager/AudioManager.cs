using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header("Audio Clip")]
    public AudioClip hoverSound;
    public AudioClip chooseSound;
    public AudioClip correctSound;
    public AudioClip ambientSound;
    public AudioClip ambientWinSound;

    [Header("Vollume Settings")]
    [Range(0.0f, 1.0f)]
    public float backgroundVollume = 1f;
    [Range(0.0f, 1.0f)]
    public float triggerVollume = 1f;

    AudioSource ambientSource;
    AudioSource triggerSource;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            //
            ambientSource = gameObject.AddComponent<AudioSource>();
            triggerSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        PlayBackgroundAmbient();
    }

    public void UpdateVolume()
    {
        ambientSource.volume = backgroundVollume;
        triggerSource.volume = triggerVollume;
    }

    public void PlayHoverSound()
    {
        triggerSource.loop = false;
        triggerSource.volume = triggerVollume;
        if (!triggerSource.isPlaying)
        {
            triggerSource.clip = hoverSound;
            triggerSource.Play();
        }
    }

    public void PlayChooseSound()
    {
        triggerSource.loop = false;
        triggerSource.volume = triggerVollume;
        if (!triggerSource.isPlaying)
        {
            triggerSource.clip = chooseSound;
            triggerSource.Play();
        }
    }

    public void PlayCorrectSound()
    {
        triggerSource.loop = false;
        triggerSource.volume = triggerVollume;
        if (!triggerSource.isPlaying)
        {
            triggerSource.clip = correctSound;
            triggerSource.Play();
        }
    }

    public void PlayBackgroundAmbient()
    {
        if (ambientSource.clip == ambientSound) return;

        ambientSource.loop = true;
        StartCoroutine(ChangeAudioClip(ambientSource, ambientSound, backgroundVollume, 1f, 1f));
    }

    public void PlayVictoryBackgroundAmbient()
    {
        if (ambientSource.clip == ambientWinSound) return;

        ambientSource.loop = true;
        StartCoroutine(ChangeAudioClip(ambientSource, ambientWinSound, backgroundVollume, 1f, 1f));
    }

    IEnumerator ChangeAudioClip(AudioSource source, AudioClip targetClip, float maxVollume, float fadeOutTime, float fadeInTime)
    {
        fadeInTime += 0.01f;
        fadeOutTime += 0.01f;
        //
        float timer = 0;
        //fade out
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float percentage = Mathf.Clamp01(timer / fadeOutTime);
            source.volume = (1 - percentage) * maxVollume;
            yield return new WaitForEndOfFrame();
        }

        //change source
        source.Stop();
        source.clip = targetClip;
        source.Play();

        //fade in
        timer = 0;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float percentage = Mathf.Clamp01(timer / fadeInTime);
            source.volume = percentage * maxVollume;
            yield return new WaitForEndOfFrame();
        }
    }
}
