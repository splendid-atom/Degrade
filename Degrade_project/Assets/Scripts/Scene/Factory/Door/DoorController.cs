using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    public Transform door; // 用于存储门的 Transform
    public Collider2D doorCollider; // 用于存储门的 Collider2D
    public float slideDistance = 4f; // 门滑动的距离（Z 轴方向）
    public float slideSpeed = 2.5f; // 门滑动的速度
    private Vector3 closedPosition; // 门的关闭位置
    private Vector3 openPosition; // 门的开启位置
    private Coroutine doorCoroutine; // 用于存储当前运行的协程
    public bool isExit = false; // 是否是出口门   
    void Start()
    {
        if (door == null)
        {
            Debug.LogError("Door Transform is not assigned!");
            return;
        }
        if (doorCollider == null)
        {
            Debug.LogError("Door Collider2D is not assigned!");
            return;
        }

        // 初始化门的关闭和开启位置
        closedPosition = door.position;
        openPosition = closedPosition + Vector3.forward * -slideDistance; // Z 轴减少为开门方向
    }

    void Update()
    {
        // Update 中不需要额外逻辑，门的移动由协程控制
    }

    // 当玩家进入触发器时调用
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 如果有正在运行的协程，先停止它
            if (doorCoroutine != null)
            {
                StopCoroutine(doorCoroutine);
            }
            if(isExit&&!Factory2Controller.Instance.isCanonInRageMode()){//是出口且炮塔未进入狂暴模式
                return;
            }
            // 启动开门协程
            doorCoroutine = StartCoroutine(SlideDoor(openPosition, true));
        }
    }

    // 当玩家离开触发器时调用
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 如果有正在运行的协程，先停止它
            if (doorCoroutine != null)
            {
                StopCoroutine(doorCoroutine);
            }
            // 启动关门协程
            doorCoroutine = StartCoroutine(SlideDoor(closedPosition, false));
        }
    }

    // 协程函数，用于平滑滑动门到目标位置，并控制 Collider 的启用状态
    private IEnumerator SlideDoor(Vector3 targetPosition, bool isOpening)
    {
        // 根据开门还是关门设置 Collider 状态
        if (isOpening)
        {
            doorCollider.enabled = false; // 开门时禁用 Collider
        }
        else
        {
            doorCollider.enabled = true; // 关门时启用 Collider
        }

        while (Vector3.Distance(door.position, targetPosition) > 0.01f)
        {
            door.position = Vector3.MoveTowards(door.position, targetPosition, slideSpeed * Time.deltaTime);
            yield return null; // 等待下一帧
        }
        // 确保门精确到达目标位置
        door.position = targetPosition;
        doorCoroutine = null; // 重置协程引用
    }
}