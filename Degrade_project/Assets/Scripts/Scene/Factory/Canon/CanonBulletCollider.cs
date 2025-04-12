using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanonBulletCollider : MonoBehaviour
{
    private CircleCollider2D circleCollider; // 存储 CircleCollider2D 组件
    private bool isPlayerInTrigger = false;  // 表示玩家是否在触发器内

    void Start()
    {
        // 获取挂载对象上的 CircleCollider2D
        circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider == null)
        {
            Debug.LogError($"{gameObject.name}: CircleCollider2D not found!", this);
        }
        else if (!circleCollider.isTrigger)
        {
            Debug.LogWarning($"{gameObject.name}: CircleCollider2D is not set as a trigger!", this);
        }
    }

    void Update()
    {
        // 可选：每帧更新状态（如果需要动态检查）
    }

    // 当有物体进入触发器时调用
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Debug.Log($"{gameObject.name}: Player is inside trigger.", this);
        }
    }

    // 当物体离开触发器时调用
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Debug.Log($"{gameObject.name}: Player has left trigger.", this);
        }
    }

    // Public 方法，返回玩家是否在触发器内的布尔值
    public bool IsPlayerInTrigger()
    {
        return isPlayerInTrigger;
    }
}