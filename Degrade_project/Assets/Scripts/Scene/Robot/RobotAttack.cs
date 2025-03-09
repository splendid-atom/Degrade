using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotAttack : MonoBehaviour
{
    private Animator animator;
    private CircleCollider2D secondCollider;
    private bool playerInside = false;
    public float attackDamage = 1f;
    public float attackInterval = 0.2f; // 每次攻击的时间间隔
    private float lastAttackTime = 0f;
    public bool isRotating = false;
    void Start()
    {
        animator = GetComponent<Animator>();

        // 获取对象上的所有 CircleCollider2D 组件
        CircleCollider2D[] colliders = GetComponents<CircleCollider2D>();

        // 确保至少有两个 CircleCollider2D
        if (colliders.Length > 1)
        {
            secondCollider = colliders[1]; // 获取第二个 CircleCollider2D
            secondCollider.isTrigger = true; // 确保它是触发器模式
            Debug.Log("成功获取第二个 CircleCollider2D：" + secondCollider);
        }
        else
        {
            Debug.LogWarning("该对象上没有足够的 CircleCollider2D 组件！");
        }
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
        if (other.CompareTag("Player") && other.IsTouching(secondCollider))
        {
            playerInside = true;
            Debug.Log("玩家进入第二个 CircleCollider2D");
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        // 持续检测玩家是否仍在碰撞器内，并定期造成伤害
        if (other.CompareTag("Player") && other.IsTouching(secondCollider))
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
            Debug.Log("玩家离开第二个 CircleCollider2D");
        }
    }

    void Attack()
    {
        PlayerController.Instance.PlayerHealth -= attackDamage;
        Debug.Log("对玩家造成 " + attackDamage + " 点伤害");
    }
}
