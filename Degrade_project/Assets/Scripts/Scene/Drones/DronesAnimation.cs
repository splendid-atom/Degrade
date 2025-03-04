using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DronesAnimation : MonoBehaviour
{
    private Animator animator;
    private GameObject Drones;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //调试动画的代码
        if(Input.GetKeyDown(KeyCode.Z)){
            SetAnimationIdling();
        }
        if(Input.GetKeyDown(KeyCode.X)){
            SetAnimationAimming();
        }
        if(Input.GetKeyDown(KeyCode.C)){
            SetAnimationShoting();
        }
    }
    public void SetAnimationIdling(){
        animator.SetBool("isIdling", true);
        animator.SetBool("isAimming", false);
        animator.SetBool("isShoting", false);
    }
    public void SetAnimationAimming(){
        animator.SetBool("isIdling", false);
        animator.SetBool("isAimming", true);
        animator.SetBool("isShoting", false);
    }
    public void SetAnimationShoting()
    {
        animator.SetBool("isIdling", false);
        animator.SetBool("isAimming", false);
        animator.SetBool("isShoting", true);
    }
    public void OnAnimationShotingReturnAimming()
    {
        animator.SetBool("isIdling", false);
        animator.SetBool("isAimming", true);
        animator.SetBool("isShoting", false);
    }
}
