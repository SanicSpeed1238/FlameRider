using UnityEngine;
using EasyTransition;

public class MainMenu : MonoBehaviour
{
    public TransitionSettings playTransition;

    public void PlayGame()
    {
        TransitionManager.Instance().Transition("Fireball Stadium", playTransition, 1f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}