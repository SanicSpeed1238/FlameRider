using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Playables;

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    [HideInInspector] public bool isPlaying;

    [Header("Important References")]
    public GameObject playerHUD;
    public GameObject introSequence;
    public GameObject resultsSequence;
    public GameObject resultsScreen;
    public GameObject pauseScreen;

    [Header("Audio Sources")]
    public AudioMixer audioMixer;
    public AudioSource gameMusic;
    public AudioSource resultsMusic;
    public AudioSource countdownSound;
    public AudioSource startSound;
    public AudioSource lapSound;

    [Header("Debugging Tools")]
    public bool disableIntro;

    // Variables Needed
    private float timeElapsed;
    private Coroutine introCoroutine;

    private void Awake()
    {
        Instance = this;
        isPlaying = false;

        introSequence.SetActive(false);
        resultsSequence.SetActive(false);

        timeElapsed = 0f;
        audioMixer.SetFloat("sfxVolume", 0f);
    }

    private void Start()
    {
        PlayIntroSequence();
    }

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (isPlaying) pauseScreen.SetActive(true);

            else if (introCoroutine != null) SkipIntro();
        }

        UpdateTimer();
    }   
    private void UpdateTimer()
    {
        if (!isPlaying) return;

        timeElapsed += Time.deltaTime;
        PlayerHUD.Instance.UpdateTimerValue(timeElapsed);
    }

    #region Public Functions

    public float GetFinalTime()
    {
        return timeElapsed;
    }

    public void LowerGameMusic(bool lower)
    {
        if (lower)
        {
            gameMusic.volume = 0.1f;
            audioMixer.SetFloat("sfxVolume", -45f);
        }
        else
        {
            gameMusic.volume = 0.45f;
            audioMixer.SetFloat("sfxVolume", 0f);
        }
    }

    #endregion

    #region Start Game
    public void StartGame()
    {
        StartCoroutine(CountdownSequence());
    }
    IEnumerator CountdownSequence()
    {
        if (disableIntro)
        {
            isPlaying = true;
            yield break;
        }
        else
        {
            yield return new WaitForSeconds(2f);
            playerHUD.SetActive(true);
            yield return new WaitForSeconds(1f);
        }         

        PlayerHUD.Instance.DisplayCountdown(3);
        countdownSound.volume = 0.2f;
        countdownSound.PlayOneShot(countdownSound.clip);
        yield return new WaitForSeconds(1f);

        PlayerHUD.Instance.DisplayCountdown(2);
        countdownSound.volume = 0.4f;
        countdownSound.PlayOneShot(countdownSound.clip);
        yield return new WaitForSeconds(1f);

        PlayerHUD.Instance.DisplayCountdown(1);
        countdownSound.volume = 0.6f;
        countdownSound.PlayOneShot(countdownSound.clip);
        yield return new WaitForSeconds(1f);

        PlayerHUD.Instance.DisplayCountdown(0);
        PlayerHUD.Instance.DisplayMessage("Go !!!");
        startSound.PlayOneShot(startSound.clip);
        gameMusic.Play();
        isPlaying = true;
    }
    #endregion

    #region Intro Sequence
    public void PlayIntroSequence()
    {
        if (!disableIntro) introCoroutine = StartCoroutine(IntroSequence());
        else StartGame();
    }
    IEnumerator IntroSequence()
    {
        introSequence.SetActive(true);
        playerHUD.SetActive(false);

        float cutsceneLength = (float)introSequence.GetComponent<PlayableDirector>().playableAsset.duration;
        yield return new WaitForSeconds(cutsceneLength);

        introSequence.SetActive(false);
        StartGame();
    }
    private void SkipIntro()
    {
        introSequence.SetActive(false);
        StartGame();
    }
    #endregion

    #region Results Sequence
    public void PlayResultsSequence()
    {
        StartCoroutine(ResultsSequence());
    }
    IEnumerator ResultsSequence()
    {
        isPlaying = false;

        PlayerHUD.Instance.DisplayMessage("Finish!");
        gameMusic.Stop();
        resultsMusic.Play();
        startSound.PlayOneShot(startSound.clip);
        yield return new WaitForSeconds(3f);

        PlayerHUD.Instance.DisplayMessage(string.Empty);
        PlayerHUD.Instance.SetSelectedButton(playerHUD.GetComponent<PlayerHUD>().replayButton);
        resultsScreen.SetActive(true);
    }
    #endregion  
}