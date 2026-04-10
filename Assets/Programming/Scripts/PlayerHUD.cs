using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [Header("Message Text")]
    public TextMeshProUGUI countdownNumbers;
    public TextMeshProUGUI messageText;

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
    private int speedValueStep;
    private float speedValueRecord;
    private Color speedValueColor;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        eventSystem = EventSystem.current;
        speedValueStep = 0;
        speedValueColor = speedValue.color;
    }

    public void DisplayMessage(string message)
    {
        messageText.text = message;

        RectTransform rect = messageText.rectTransform;
        rect.DOKill();
        float screenWidth = Screen.width;
        rect.anchoredPosition = new Vector2(-screenWidth, rect.anchoredPosition.y);
        rect.localScale = Vector3.one;
        Sequence seq = DOTween.Sequence();

        // 🔹 Slide in from left
        seq.Append(rect.DOAnchorPosX(0, 0.5f).SetEase(Ease.OutCubic));

        // 🔹 Wait in center
        seq.AppendInterval(1.2f);

        // 🔹 Zoom + fly out to the right
        seq.Append(rect.DOAnchorPosX(screenWidth, 0.5f).SetEase(Ease.InBack));
        seq.Join(rect.DOScale(1.5f, 0.5f));
    }
    public void DisplayCountdown(int num)
    {
        string numText = num.ToString();
        if (num == 0) numText = string.Empty;

        countdownNumbers.text = numText;
        countdownNumbers.transform.DOPunchScale(Vector3.one * 0.5f, 0.4f, 10, 1);

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

    public void UpdateSpeedValue(float playerSpeed, float baseSpeed)
    {
        int intSpeed = Mathf.RoundToInt(playerSpeed);
        speedValue.text = intSpeed.ToString();

        int newSpeedValueIncrement = intSpeed / 50;

        bool passedRecord =
            playerSpeed > baseSpeed &&
            newSpeedValueIncrement != speedValueStep &&
            Mathf.Abs(playerSpeed - speedValueRecord) > 5f;

        if (passedRecord)
        {
            speedValueStep = newSpeedValueIncrement;
            speedValueRecord = playerSpeed;

            Transform t = speedValue.transform;

            DOTween.Kill("speedPunch");
            t.localScale = Vector3.one;

            Sequence seq = DOTween.Sequence();
            seq.SetId("speedPunch");

            // Punch Animation
            float punchRelative = Mathf.Clamp01(intSpeed / 1000f);
            float punchStrength = Mathf.Lerp(0.2f, 2f, punchRelative);

            seq.Append(
                t.DOPunchScale(Vector3.one * punchStrength, 0.3f, 8, 0.8f)
            );

            // Color flash (optional - keeping your commented logic)
            // seq.Join(speedValue.DOColor(Color.white, 0.1f));
            // seq.Append(speedValue.DOColor(speedValueColor, 0.2f));
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

    public void SetSelectedButton(GameObject menuButton)
    {
        eventSystem.SetSelectedGameObject(menuButton);
    }
}