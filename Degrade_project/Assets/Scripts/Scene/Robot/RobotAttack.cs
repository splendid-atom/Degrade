using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAttack : MonoBehaviour
{
    private Animator animator;
    private bool playerInside = false;
    public float attackDamage = 1f;
    public float attackInterval = 0.2f; // 每次攻击的时间间隔
    private float lastAttackTime = 0f;
    public bool isRotating = false;
    void Start()
    {
        animator = GetComponent<Animator>();
    }
    void Update()
    {
        if(animator.GetBool("isAttacking")==true){
            isRotating = true;
        }
        else{
            isRotating = false;
        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        // 检查是否是玩家进入
        if (other.CompareTag("Player"))
        {
            playerInside = true;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // 持续检测玩家是否仍在碰撞器内，并定期造成伤害
        if (other.CompareTag("Player"))
        {
            if (Time.time - lastAttackTime >= attackInterval) // 检测攻击间隔
            {
                lastAttackTime = Time.time; // 记录上次攻击时间
                if(isRotating){
                    Attack(); // 对玩家造成伤害                    
                }
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // 检测玩家离开
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
        if(playerInside){}
    }

    void Attack()
    {
        PlayerController.Instance.PlayerHealth -= attackDamage;
    }
}
