using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInsideTrigger : MonoBehaviour
{
    public bool isPlayerInside { get; private set; } = false; // 记录玩家是否在触发器内
    private GameObject player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            isPlayerInside = true;
            // Debug.Log("玩家进入触发器: " + gameObject.name);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == player)
        {
            isPlayerInside = false;
            // Debug.Log("玩家离开触发器: " + gameObject.name);
        }
    }
}
