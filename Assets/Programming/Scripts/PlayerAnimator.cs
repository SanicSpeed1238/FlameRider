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

    public void SteerAnimation(float direction)
    {
        animator.SetFloat("Direction", direction, 0.2f, Time.deltaTime);
    }

    public void DriftAnimation(bool activate, float direction)
    {
        animator.SetBool("Drifting", activate);
        animator.SetFloat("Direction", direction);
    }
}