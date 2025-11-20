using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using GogoGaga.TME;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [Header("Message Text")]
    public TextMeshProUGUI countdownNumbers;
    public TextMeshProUGUI messageText;
    public LeantweenCustomAnimator messageAnimation;

    [Header("Player Status")]
    public Slider fireEnergy;
    public TextMeshProUGUI speedValue;

    [Header("Track Progress")]
    public TextMeshProUGUI timerValue;
    public TextMeshProUGUI lapNumber;

    [Header("Button References")]
    public GameObject resumeButton;
    public GameObject replayButton;

    // Other Variables Needed
    private EventSystem eventSystem;
    private bool tweenPlaying;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        eventSystem = EventSystem.current;
    }

    public void DisplayMessage(string message)
    {
        messageText.text = message;
        messageAnimation.PlayAnimation();
    }
    public void DisplayCountdown(int num)
    {
        string numText = num.ToString();
        if (num == 0) numText = string.Empty;

        countdownNumbers.text = numText;
        countdownNumbers.gameObject.GetComponent<LeantweenCustomAnimator>().PlayAnimation();

        countdownNumbers.color = num switch
        {
            3 => Color.yellow,
            2 => new Color(1f, 0.5f, 0f),
            1 => Color.red,
            _ => Color.white,
        };
    }

    public void UpdateFireEnergy(float value)
    {
        fireEnergy.value = value / 100f;
    }

    public void UpdateSpeedValue(float playerSpeed, bool increaseAnimation)
    {
        int intSpeed = Mathf.RoundToInt(playerSpeed);
        speedValue.text = intSpeed.ToString();

        if (increaseAnimation && !tweenPlaying)
        {
            speedValue.GetComponent<LeantweenCustomAnimator>().PlayAnimation();
            SetTweenPlaying(true);
        }
    }

    public void UpdateTimerValue(float timeElapsed)
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        int milliseconds = Mathf.FloorToInt((timeElapsed * 1000f) % 1000f);

        timerValue.text = string.Format("{0}:{1:00}:{2:000}", minutes, seconds, milliseconds);
    }

    public void UpdateLapNumber(int lap)
    {
        string lapText = lap.ToString();
        lapNumber.text = "LAP " + lapText + "/3";
    }

    public void SetTweenPlaying(bool playing)
    {
        tweenPlaying = playing;
    }

    public void SetSelectedButton(GameObject menuButton)
    {
        eventSystem.SetSelectedGameObject(menuButton);
    }
}