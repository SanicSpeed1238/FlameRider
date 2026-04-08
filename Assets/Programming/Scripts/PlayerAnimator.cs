using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [SerializeField] private Animator motorcycleAnim;
    [SerializeField] private Animator playerAnim;

    void Start()
    {
        if (motorcycleAnim == null) Debug.LogWarning("Motorcycle Animator not set!");
        if (playerAnim == null) Debug.LogWarning("Player Animator not set!");
    }

    public void SetGrounded(bool grounded)
    {
        motorcycleAnim.SetBool("Grounded", grounded);
        playerAnim.SetBool("Grounded", grounded);
    }

    public void SetSpeed(float speed)
    {
        motorcycleAnim.SetFloat("Speed", speed, 0.2f, Time.deltaTime);
        playerAnim.SetFloat("Speed", speed, 0.2f, Time.deltaTime);
    }

    public void SteerAnimation(float tilt)
    {
        motorcycleAnim.SetFloat("Tilt", tilt, 0.2f, Time.deltaTime);
        playerAnim.SetFloat("Tilt", tilt, 0.2f, Time.deltaTime);
    }

    public void DriftAnimation(bool activate, float direction)
    {
        motorcycleAnim.SetBool("Drifting", activate);
        motorcycleAnim.SetFloat("Direction", direction);

        playerAnim.SetBool("Drifting", activate);
        playerAnim.SetFloat("Direction", direction);
    }
}