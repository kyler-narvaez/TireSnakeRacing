using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAnimations : MonoBehaviour
{
    private Animator animator;
    private bool oldDirection = false;

    public void SetAnimator()
    {
        animator = GetComponentInChildren<Animator>();
    }

    public void GoingBackwards(bool isBackwards)
    {
        if (isBackwards != oldDirection)
        {
            transform.Rotate(0f, 180f, 0f, Space.Self);
            oldDirection = isBackwards;
        }
    }

    public void SetSpeed(float speed)
    {
        if (animator == null) SetAnimator();
        animator.SetFloat("Speed", speed);
    }

    public void SetGrounded(bool grounded)
    {
        if (animator == null) SetAnimator();
        animator.SetBool("IsGrounded", grounded);
    }

    public void TriggerJump()
    {
        if (animator == null) SetAnimator();
        animator.SetTrigger("Jump");
    }

    public void SetTrauma(bool isTrauma)
    {
        if (animator == null) SetAnimator();
        animator.SetBool("IsTumble", isTrauma);
    }

    public void SetBumped(bool isBumped)
    {
        if (animator == null) SetAnimator();
        animator.SetBool("IsBumped", isBumped);
    }
}
