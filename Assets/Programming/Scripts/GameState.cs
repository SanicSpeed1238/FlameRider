using GogoGaga.TME;
using System.Collections;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;
    [HideInInspector] public bool isPlaying;

    [Header("Important References")]
    public GameObject playerHUD;
    public GameObject winScreen;
    public GameObject pauseScreen;

    [Header("Audio Sources")]
    public AudioSource countdownSound;
    public AudioSource lapSound;
    public AudioSource winSound;

    private float timeElapsed = 0f;

    private void Awake()
    {
        Instance = this;
        isPlaying = false;
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
        countdownSound.volume = 1f;
        countdownSound.PlayOneShot(countdownSound.clip);
        PlayerHUD.Instance.DisplayMessage("GO !!!");
        isPlaying = true;
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
        winSound.Play();
        yield return new WaitForSeconds(3f);

        PlayerHUD.Instance.DisplayMessage(string.Empty);
        playerHUD.GetComponent<LeantweenCustomAnimator>().PlayAnimation();
        winScreen.SetActive(true);
    }
}