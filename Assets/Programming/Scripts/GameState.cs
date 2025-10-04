using UnityEngine;
using UnityEngine.SceneManagement;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    private float timeElapsed = 0f;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        timeElapsed += Time.deltaTime;
        PlayerHUD.Instance.UpdateTimerValue(timeElapsed);

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}