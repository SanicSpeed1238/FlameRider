using GogoGaga.TME;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    [HideInInspector] public bool isPlaying;

    [Header("Important References")]
    public GameObject playerHUD;
    public GameObject winScreen;
    public GameObject pauseScreen;

    [Header("Audio Sources")]
    public AudioMixer audioMixer;
    public AudioSource gameMusic;
    public AudioSource resultsMusic;
    public AudioSource countdownSound;
    public AudioSource startSound;
    public AudioSource lapSound;

    [Header("Debugging Tools")]
    public bool disableCountdown;

    // Variables Needed
    private float timeElapsed = 0f;

    private void Awake()
    {
        Instance = this;
        isPlaying = false;
        audioMixer.SetFloat("sfxVolume", 0f);
    }
    private void Start()
    {
        StartCoroutine(StartCountdown());
    }

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (isPlaying) pauseScreen.SetActive(true);
        }

        UpdateTimer();
    }

    IEnumerator StartCountdown()
    {
        if (!disableCountdown)
        {
            yield return new WaitForSeconds(2f);

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
            PlayerHUD.Instance.DisplayMessage("GO !!!");
            startSound.PlayOneShot(startSound.clip);
            gameMusic.Play();
            isPlaying = true;          
        }
        else
        {
            isPlaying = true;
        }      
    }

    private void UpdateTimer()
    {
        if (!isPlaying) return;

        timeElapsed += Time.deltaTime;
        PlayerHUD.Instance.UpdateTimerValue(timeElapsed);
    }
    public float GetFinalTime()
    {
        return timeElapsed;
    }

    public void WinGame()
    {
        StartCoroutine(FinishSequence());
    }
    IEnumerator FinishSequence()
    {
        isPlaying = false;

        PlayerHUD.Instance.DisplayMessage("FINISH!");
        gameMusic.Stop();
        resultsMusic.Play();
        startSound.PlayOneShot(startSound.clip);
        yield return new WaitForSeconds(3f);

        PlayerHUD.Instance.DisplayMessage(string.Empty);
        PlayerHUD.Instance.SetSelectedButton(playerHUD.GetComponent<PlayerHUD>().replayButton);
        playerHUD.GetComponent<LeantweenCustomAnimator>().PlayAnimation();
        winScreen.SetActive(true);
    }

    public void LowerGameVolume(bool paused)
    {
        if (paused)
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
}