using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void BoostAnimation(bool activate)
    {
        animator.SetBool("Boosting", activate);
    }

    public void JumpAnimation(bool activate)
    {
        animator.SetBool("Jumping", activate);
    }

    public void SteerAnimation(float steer)
    {
        animator.SetFloat("Steer", steer);
    }

    public void DriftAnimation(bool activate, float drift)
    {
        animator.SetBool("Drifting", activate);
        animator.SetFloat("Drift", drift);
    }
}