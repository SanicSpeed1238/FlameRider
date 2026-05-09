using UnityEngine;
using System.Collections;

public class AudioSequencer : MonoBehaviour
{
    private AudioSource audioSource;
    private float baseVolume;

    [Header("Fade In")]
    public bool enableFadeIn;
    public float startFadeIn;
    public float fadeInDuration;

    [Header("Fade Out")]
    public bool enableFadeOut;
    public float startFadeOut;
    public float fadeOutDuration;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        baseVolume = audioSource.volume;
    }
    void Start()
    {
        if(enableFadeIn) StartCoroutine(FadeIn(startFadeIn, fadeInDuration));
        if(enableFadeOut) StartCoroutine(FadeOut(startFadeOut, fadeOutDuration));
    }

    IEnumerator FadeIn(float startTime, float fadeDuration)
    {
        yield return new WaitForSeconds(startTime);
        float time = 0f;
        audioSource.volume = 0f;
        audioSource.Play();

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, baseVolume, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = baseVolume;
    }

    IEnumerator FadeOut(float startTime, float fadeDuration)
    {
        yield return new WaitForSeconds(startTime);
        float startVolume = audioSource.volume;
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVolume, 0f, time / fadeDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}