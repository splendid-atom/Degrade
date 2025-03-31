using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BambooGroupMovement : MonoBehaviour
{
    public static BambooGroupMovement instance;
    public Vector2 initialPosition;  // 期望的本地坐标
    public Vector2 targetPosition;   // 目标位置
    public float moveDuration = 3f; // 每次移动的时间（秒）
    public float movePeriod = 10f;
    private bool isMovingToTarget = false;  // 用于判断是否正在向目标位置移动
    public bool isInitial = true;//标志是否为初始位置，对应bamboohint中的hint1和hint2
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        // 如果 initialPosition 的 x 和 y 不为零，则设置该对象的本地坐标
        if (initialPosition.x != 0 && initialPosition.y != 0)
        {
            transform.localPosition = initialPosition;  // 设置初始位置
            if(targetPosition.x != 0 && targetPosition.y != 0){
                // 启动循环的协程
                StartCoroutine(MoveBackAndForth());                
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 你可以在此添加额外的逻辑（如暂停、停止移动等）
    }

    // 移动到目标位置并返回的协程
    private IEnumerator MoveBackAndForth()
    {
        while (true)
        {
            Vector2 startPos = transform.localPosition;
            Vector2 endPos = isMovingToTarget ? targetPosition : initialPosition;

            float timeElapsed = 0f;

            // 逐渐移动到目标位置
            while (timeElapsed < moveDuration)
            {
                transform.localPosition = Vector2.Lerp(startPos, endPos, timeElapsed / moveDuration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            // 确保最终位置精确
            transform.localPosition = endPos;

            // 等待10秒
            yield return new WaitForSeconds(movePeriod);

            // 切换方向
            isMovingToTarget = !isMovingToTarget;
            isInitial = !isInitial;
        }
    }
}
