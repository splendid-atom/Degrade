    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldGeneratorController : MonoBehaviour
{
    public float Health = 100f;
    private ShieldGeneratorAnimation ShieldGeneratorAnimation;
    void Start()
    {
        ShieldGeneratorAnimation = GetComponent<ShieldGeneratorAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("shield health:"+Health);
        if(Input.GetKeyDown(KeyCode.K)){
            Health -= 10f;
        }
        if(Health <= 0){
            ShieldGeneratorAnimation.OnAnimationBroken();
        }
    }
}
