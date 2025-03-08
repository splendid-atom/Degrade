using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAnimation : MonoBehaviour
{
    private Animator animator;
    private GameObject Robot;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        //调试动画的代码
        if(Input.GetKeyDown(KeyCode.Z)){
            SetAnimationMove();
        }
        if(Input.GetKeyDown(KeyCode.X)){
            SetAnimationReadyAttack();
        }
        if(Input.GetKeyDown(KeyCode.C)){
            SetAnimationAttack();
        }
        if(Input.GetKeyDown(KeyCode.V)){
            SetAnimationReturn();
        }
    }
    public void SetAnimationMove(){
        animator.SetBool("isMoving", true);
        animator.SetBool("isReadyAttacking", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", false);
    }
    public void SetAnimationReadyAttack(){
        animator.SetBool("isMoving", false);
        animator.SetBool("isReadyAttacking", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", false);

    }
    public void SetAnimationAttack()
    {
        animator.SetBool("isMoving", false);
        animator.SetBool("isReadyAttacking", false);
        animator.SetBool("isAttacking", true);
        animator.SetBool("isReturning", false);

    }
    public void SetAnimationReturn()
    {
        animator.SetBool("isMoving", false);
        animator.SetBool("isReadyAttacking", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", true);

    }
    public void OnAnimationReadyAndAttack()
    {
        animator.SetBool("isMoving", false);
        animator.SetBool("isReadyAttacking", false);
        animator.SetBool("isAttacking", true);
        animator.SetBool("isReturning", false);

    }
    public void OnAnimationReturnToMoving()
    {
        animator.SetBool("isMoving", true);
        animator.SetBool("isReadyAttacking", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", false);

    }
}
