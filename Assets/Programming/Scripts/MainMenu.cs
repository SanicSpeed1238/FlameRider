using UnityEngine;
using EasyTransition;

public class MainMenu : MonoBehaviour
{
    public TransitionSettings playTransition;

    private void Start()
    {
        Application.targetFrameRate = 120;
    }

    public void PlayGame()
    {
        TransitionManager.Instance().Transition("Test Scene", playTransition, 0.2f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}