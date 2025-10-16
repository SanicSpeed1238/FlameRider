using UnityEngine;
using UnityEngine.UI;
using GogoGaga.TME;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [Header("Message Text")]
    public TextMeshProUGUI messageText;
    public TextMeshProUGUI countdownNumbers;

    [Header("Player Status")]
    public Slider fireEnergy;
    public TextMeshProUGUI speedValue;

    [Header("Track Progress")]
    public TextMeshProUGUI timerValue;
    public TextMeshProUGUI lapNumber;

    private void Awake()
    {
        Instance = this;
    }

    public void DisplayMessage(string message)
    {
        messageText.text = message;
        messageText.gameObject.GetComponent<LeantweenCustomAnimator>().PlayAnimation();
    }
    public void DisplayCountdown(int num)
    {
        string numText = num.ToString();
        if (num == 0) numText = string.Empty;

        countdownNumbers.text = numText;
        countdownNumbers.gameObject.GetComponent<LeantweenCustomAnimator>().PlayAnimation();

        switch(num)
        {
            case 3:
                countdownNumbers.color = Color.yellow; break;
            case 2:
                countdownNumbers.color = new Color(1f, 0.5f, 0f); break;
            case 1:
                countdownNumbers.color = Color.red; break;
            default:
                countdownNumbers.color = Color.white; break;
        }
    }

    public void UpdateFireEnergy(float value)
    {
        fireEnergy.value = value / 100f;
    }

    public void UpdateSpeedValue(float playerSpeed)
    {
        int intSpeed = Mathf.RoundToInt(playerSpeed);
        speedValue.text = intSpeed.ToString();
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
}