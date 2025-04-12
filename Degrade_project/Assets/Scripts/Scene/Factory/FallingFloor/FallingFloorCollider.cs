using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingFloorCollider : MonoBehaviour
{
    private BoxCollider2D boxCollider;
    public bool isPlayerInside = false;
    public bool isFloorFalling = false;
    void Start()
    {
        // 获取当前对象的 BoxCollider2D
        boxCollider = GetComponent<BoxCollider2D>();

        if (boxCollider == null)
        {
            Debug.LogWarning("BoxCollider2D 未找到，请确保该对象上有 BoxCollider2D 组件！");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 检测玩家是否进入
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            Transform floor = transform.parent; // 获取当前 collider 所在对象的父对象
            if (floor != null)
            {
                // Debug.Log("玩家进入了 FallingFloor：" + floor.name);
            }
            else
            {
                Debug.LogWarning("FallingFloorCollider 没有父对象！");
            }
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            Transform floor = transform.parent; // 获取当前 collider 所在对象的父对象
            if (floor != null)
            {
                // Debug.Log("玩家离开了 FallingFloor：" + floor.name);
            }
            else
            {
                Debug.LogWarning("FallingFloorCollider 没有父对象！");
            }
        }
    }
}
