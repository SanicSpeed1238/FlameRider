using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void SetGrounded(bool grounded)
    {
        animator.SetBool("Grounded", grounded);
    }

    public void SetSpeed(float speed)
    {
        animator.SetFloat("Speed", speed, 0.2f, Time.deltaTime);
    }

    public void SteerAnimation(float tilt)
    {
        animator.SetFloat("Tilt", tilt, 0.2f, Time.deltaTime);
    }

    public void DriftAnimation(bool activate, float direction)
    {
        animator.SetBool("Drifting", activate);
        animator.SetFloat("Direction", direction);
    }
}