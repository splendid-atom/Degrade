using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldGeneratorAnimation : MonoBehaviour
{
    private Animator animator;
    private GameObject ShieldGenerator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnAnimationBroken()
    {
        if(animator.GetBool("isBroken") == false){
            animator.SetBool("isBroken", true);            
        }

    }
}
