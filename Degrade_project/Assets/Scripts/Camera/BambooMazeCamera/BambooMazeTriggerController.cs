using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BambooMazeTriggerController : MonoBehaviour
{
    public static BambooMazeTriggerController instance;
    private Collider2D BambooMazeTrigger; // 修正大小写
    public bool isInMaze = false;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        BambooMazeTrigger = GetComponent<Collider2D>();
        
        if (BambooMazeTrigger == null)
        {
            Debug.LogError("没有找到Collider2D组件！");
        }
    }

    void Update()
    {
        // 可以在这里放置其他逻辑，当前没有任何操作
    }

    // 玩家进入触发器
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInMaze = true;
            // Debug.Log("玩家进入触发器: " + gameObject.name);
            // BigMapController.instance.DisableMap();
            BambooMazeCameraController.instance.isInMaze = true;
        }
    }

    // 玩家离开触发器
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isInMaze = false;
            // Debug.Log("玩家离开触发器: " + gameObject.name);
            // BigMapController.instance.EnableMap();
            BambooMazeCameraController.instance.isInMaze = false;
        }
    }
}
