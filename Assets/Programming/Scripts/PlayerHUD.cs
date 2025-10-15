using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public static PlayerHUD Instance;

    [Header("Fire Energy UI")]
    public Slider fireEnergy;

    [Header("Speed UI")]
    public TextMeshProUGUI speedValue;

    [Header("Timer UI")]
    public TextMeshProUGUI timerValue;

    private void Awake()
    {
        Instance = this;
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
}