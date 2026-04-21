using UnityEngine;
using EasyTransition;

public class MainMenu : MonoBehaviour
{
    public TransitionSettings playTransition;

    public void PlayGame()
    {
        TransitionManager.Instance().Transition("Fireball Stadium", playTransition, 2.5f);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}