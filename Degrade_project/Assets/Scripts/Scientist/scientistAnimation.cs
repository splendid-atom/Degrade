using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scientistAnimation : MonoBehaviour
{
    Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.U)){
            OnAnimatorFloatUp();
        }
        if(Input.GetKeyDown(KeyCode.I)){
            OnAnimatorFloatDown();
        }
        if(Input.GetKeyDown(KeyCode.O)){
            OnAnimatorSummonUp();
        }
        if(Input.GetKeyDown(KeyCode.P)){
            OnAnimatorIdling();
        }
    }
    public void OnAnimatorFloatUp(){
        animator.SetBool("isFloatUp", true);
        animator.SetBool("isFloatDown", false);
        animator.SetBool("isFloating", false);
        animator.SetBool("isIdling", false);
        animator.SetBool("isSummoning", false);
        animator.SetBool("isSummonUp", false);

    }
    public void OnAnimatorFloatDown(){
        animator.SetBool("isFloatUp", false);
        animator.SetBool("isFloatDown", true);
        animator.SetBool("isFloating", false);
        animator.SetBool("isIdling", false);
        animator.SetBool("isSummoning", false);
        animator.SetBool("isSummonUp", false);

    }
    public void OnAnimatorFloating(){
        animator.SetBool("isFloatUp", false);
        animator.SetBool("isFloatDown", false);
        animator.SetBool("isFloating", true);
        animator.SetBool("isIdling", false);
        animator.SetBool("isSummoning", false);
        animator.SetBool("isSummonUp", false);
        
    }
    public void OnAnimatorSummonUp(){
        animator.SetBool("isFloatUp", false);
        animator.SetBool("isFloatDown", false);
        animator.SetBool("isFloating", false);
        animator.SetBool("isIdling", false);
        animator.SetBool("isSummoning", false);
        animator.SetBool("isSummonUp", true);
    }
    public void OnAnimatorSummoning(){
        animator.SetBool("isFloatUp", false);
        animator.SetBool("isFloatDown", false);
        animator.SetBool("isFloating", false);
        animator.SetBool("isIdling", false);
        animator.SetBool("isSummoning", true);
        animator.SetBool("isSummonUp", false);

    }
    public void OnAnimatorIdling(){
        animator.SetBool("isFloatUp", false);
        animator.SetBool("isFloatDown", false);
        animator.SetBool("isFloating", false);
        animator.SetBool("isIdling", true);
        animator.SetBool("isSummoning", false);
        animator.SetBool("isSummonUp", false);

    }
}
