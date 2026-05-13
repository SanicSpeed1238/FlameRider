using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SplashScreen : MonoBehaviour
{
    [SerializeField] private bool limitFPS;
    [SerializeField] private float totalDuration = 2f;

    private IEnumerator Start()
    {
        if(limitFPS) Application.targetFrameRate = 120;

        yield return new WaitForSeconds(totalDuration);
        SceneManager.LoadScene("Main Menu");
    }
}