using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveparticleScript : MonoBehaviour
{
[SerializeField] float damageDelay = 0.1f;    // 玩家需要停留的时间（秒）才会受到伤害
    [SerializeField] float damageAmount = 10f;  // 每次造成的伤害量

    // 内部状态变量
    private bool isPlayerInArea = false;        // 玩家是否在陷阱区域内
    private bool isdamaging = false;    // 协程是否正在运行

    private float playerHealth;         // 玩家的健康组件
    // 初始化时获取玩家
    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = PlayerController.Instance.PlayerHealth;
        }
    }

    // 玩家进入触发区域
void OnTriggerEnter2D(Collider2D other)
{
    Debug.Log("触发区域进入");
    if (!isdamaging) 
    {
        StartCoroutine(DamageCoroutine(other)); // 修改这里
        //Debug.Log("开始伤害...");
    }
    else 
    {
        //Debug.Log("正在伤害中...");
    }
}

IEnumerator DamageCoroutine(Collider2D other)
{
    //Debug.Log("开始伤害协程...");
    yield return new WaitForSeconds(damageDelay);

    isdamaging = true;
    if (other.CompareTag("Player"))
    {   
        Debug.Log("玩家受到伤害10点");
        PlayerController.Instance.PlayerHealth -= (int)damageAmount;
        Destroy(gameObject);
    }
    
    yield return new WaitForSeconds(0.1f);
    isdamaging = false;
}
}
