using UnityEngine;

public class PauseScreen : MonoBehaviour
{
    public GameObject pauseScreenUI;

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
        pauseScreenUI.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseScreenUI.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}