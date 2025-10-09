using UnityEngine;
using EasyTransition;

public class PauseScreen : MonoBehaviour
{
    public GameObject pauseScreenUI;
    public TransitionSettings exitTransition;

    bool paused;

    private void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (!paused) PauseGame();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0;
        paused = true;
        pauseScreenUI.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        paused = false;
        pauseScreenUI.SetActive(false);
    }

    public void ExitGame()
    {
        Time.timeScale = 1;
        TransitionManager.Instance().Transition("Main Menu", exitTransition, 0.2f);
    }
}