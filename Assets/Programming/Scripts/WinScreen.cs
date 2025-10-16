using UnityEngine;
using UnityEngine.SceneManagement;
using EasyTransition;
using TMPro;
using System.Collections;

public class WinScreen : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI finalTimeValueText;

    [Header("Scene Transitions")]
    public TransitionSettings replayTransition;
    public TransitionSettings exitTransition;

    public void ReplayGame()
    {
        TransitionManager.Instance().Transition(SceneManager.GetActiveScene().name, replayTransition, 0f);
    }

    public void ExitGame()
    {
        TransitionManager.Instance().Transition("Main Menu", exitTransition, 0.2f);
    }

    public void ShowFinalTime()
    {
        StartCoroutine(FinalTimeReveal());
    }
    IEnumerator FinalTimeReveal()
    {
        float finalTime = GameState.Instance.GetFinalTime();

        float startTime = 600f;
        float duration = 1f;
        float elapsed = 0f;

        yield return new WaitForSeconds(0.25f);
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float displayTime = Mathf.Lerp(startTime, finalTime, t);

            int minutes = Mathf.FloorToInt(displayTime / 60f);
            int seconds = Mathf.FloorToInt(displayTime % 60f);
            int milliseconds = Mathf.FloorToInt((displayTime * 1000f) % 1000f);

            finalTimeValueText.text = $"{minutes:0}:{seconds:00}:{milliseconds:000}";
            yield return null;
        }

        int finalMinutes = Mathf.FloorToInt(finalTime / 60f);
        int finalSeconds = Mathf.FloorToInt(finalTime % 60f);
        int finalMilliseconds = Mathf.FloorToInt((finalTime * 1000f) % 1000f);
        finalTimeValueText.text = $"{finalMinutes:0}:{finalSeconds:00}:{finalMilliseconds:000}";
    }
}