    using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldGeneratorController : MonoBehaviour
{
    public float Health = 100f;
    private ShieldGeneratorAnimation ShieldGeneratorAnimation;
    private Enemy3 Enemy3;
    void Start()
    {
        Enemy3 = GetComponent<Enemy3>();
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
        if(Health<=0){
            gameObject.SetActive(false);
        }
        Health = Enemy3.currentHealth;
    }
}
