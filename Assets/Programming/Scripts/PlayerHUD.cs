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

    [Header("Fire Energy")]
    public Slider fireEnergy;   

    [Header("Current Speed")]
    public TextMeshProUGUI speedValue;
    public Animator speedAnimator;
    public float speedAnimationScale;
    public float speedFontSizeMax;
    private float speedFontSizeBase;

    [Header("Track Progress")]
    public TextMeshProUGUI timerValue;
    public TextMeshProUGUI lapNumber;

    [Header("Button References")]
    public GameObject resumeButton;
    public GameObject replayButton;

    // Other Variables Needed
    private EventSystem eventSystem;

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
        float normalizedSpeed = Mathf.Clamp01(playerSpeed / (baseSpeed * 2f));        
        string characterSpacing = "<mspace=115>";
        speedValue.text = string.Format(characterSpacing + intSpeed.ToString());

        speedAnimator.speed = Mathf.Clamp(normalizedSpeed * speedAnimationScale, 0.5f, speedAnimationScale);
        if (speedAnimator.speed <= 0.51f) speedAnimator.speed = 0f;

        if (speedFontSizeBase == 0) speedFontSizeBase = speedValue.fontSize;
        float targetSize = Mathf.Lerp(speedFontSizeBase, speedFontSizeMax, normalizedSpeed);
        speedValue.fontSize = targetSize;

        Color targetColor;
        if (normalizedSpeed < 0.5f)
        {
            float t = normalizedSpeed / 0.5f;
            targetColor = Color.Lerp(Color.white, Color.yellow, t);
        }
        else
        {
            float t = (normalizedSpeed - 0.5f) / 0.5f;
            targetColor = Color.Lerp(Color.yellow, Color.red, t);
        }
        speedValue.DOColor(targetColor, 0.1f);
    }

    public void UpdateTimerValue(float timeElapsed)
    {
        int minutes = Mathf.FloorToInt(timeElapsed / 60f);
        int seconds = Mathf.FloorToInt(timeElapsed % 60f);
        int milliseconds = Mathf.FloorToInt((timeElapsed * 1000f) % 1000f);

        string characterSpacing = "<mspace=45>";
        timerValue.text = string.Format(characterSpacing + "{0}:{1:00}:{2:000}", minutes, seconds, milliseconds);
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