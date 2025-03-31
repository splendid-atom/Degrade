using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRunArmAnimation : MonoBehaviour
{
    Animator animator;
    public LineRunArmController LineRunArmController;
    public bool isCatching = false;
    public bool isReleased = false;
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void ResetLineRunArm(){
        animator.SetBool("isCatched",false);
    }
    void isCatchingPlayer(){
        isCatching = true;
    }
    void isNotCatchingPlayer(){
        isCatching = false;
    }
    void isReleasedPlayer(){
        if(LineRunArmController.isPlayerCatchedBool()){
            isReleased = true;            
        }

    }
    public void isNotReleasedPlayer(){
        isReleased = false;
    }
    public bool GetIsCatching(){
        return isCatching;
    }
    public bool GetIsReleased(){
        return isReleased;
    }
}
