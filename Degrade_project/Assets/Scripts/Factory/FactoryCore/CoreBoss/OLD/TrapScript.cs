using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapScript : MonoBehaviour
{
// 在 Inspector 中设置的变量
    [SerializeField] float damageDelay = 0.5f;    // 玩家需要停留的时间（秒）才会受到伤害
    [SerializeField] float damageAmount = 10f;  // 每次造成的伤害量

    // 内部状态变量
    private bool isPlayerInArea = false;        // 玩家是否在陷阱区域内
    private bool isCoroutineRunning = false;    // 协程是否正在运行

    private float playerHealth;         // 玩家的健康组件
    // 初始化时获取玩家
    void Start()
    {
        // GameObject player = GameObject.FindGameObjectWithTag("Player");
        // if (player != null)
        // {
        //     playerHealth = PlayerController.Instance.PlayerHealth;
        // }
    }

    // 玩家进入触发区域
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = true;
            // 如果协程未运行，则启动协程
            if (!isCoroutineRunning)
            {
                StartCoroutine(DamageCoroutine());
                isCoroutineRunning = true;
            }
        }
    }

    // 玩家离开触发区域
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInArea = false;
        }
    }

    // 协程：处理伤害逻辑
    IEnumerator DamageCoroutine()
    {
        while (isPlayerInArea)
        {
            // 等待指定的延迟时间
            yield return new WaitForSeconds(damageDelay);
            // 如果玩家仍在区域内且玩家健康组件存在，则造成伤害
            if (isPlayerInArea && !PlayerController.Instance.isInvincible)
            {
                if (PlayerController.Instance.PlayerHealth>=0) PlayerController.Instance.PlayerHealth -= (int)damageAmount;
                
                //playerHealth.TakeDamage(damageAmount);
            }
        }
        // 玩家离开区域后，协程结束，重置运行状态
        isCoroutineRunning = false;
    }
}
