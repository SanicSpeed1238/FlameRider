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
        fireEnergy.value = value;
    }

    public void UpdateSpeedValue(float value)
    {
        // update speed text but as an int
    }

    public void UpdateTimerValue()
    {
        // update timer text in the format 0:00:000
    }
}