using UnityEngine;
using EasyTransition;

public class PauseScreen : MonoBehaviour
{
    public TransitionSettings exitTransition;

    private void OnEnable()
    {
        PauseGame();
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        GameState.Instance.isPlaying = false;
        GameState.Instance.LowerGameVolume(true);
        PlayerHUD.Instance.SetSelectedButton(PlayerHUD.Instance.resumeButton);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        GameState.Instance.isPlaying = true;
        GameState.Instance.LowerGameVolume(false);
        this.gameObject.SetActive(false);
    }

    public void ExitGame()
    {
        Time.timeScale = 1;
        TransitionManager.Instance().Transition("Main Menu", exitTransition, 0.2f);
    }
}