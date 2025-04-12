using System.Collections;
using UnityEngine;

public class TopArmTrashController : MonoBehaviour
{
    private Rigidbody2D rb; // 用于初始状态管理
    public float fallSpeed = 2f; // 初始摔落速度（可调整）
    public float gravityAcceleration = 9.81f; // 模拟重力加速度（可调整）
    public Transform TopArmContainer; // 目标停止位置的参考
    private float currentZVelocity = 0f; // 当前Z轴方向的速度
    private bool isFalling = false; // 标记是否正在摔落

    void Start()
    {
        if(TopArmContainer == null){
            TopArmContainer = GameObject.Find("TopArmContainer").transform;
        }
        // 获取 Rigidbody2D 组件（用于初始运动学状态）
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true; // 初始设置为运动学，避免在抓取前受默认重力影响
        }

        // 确保 TopArmContainer 已赋值
        if (TopArmContainer == null)
        {
            Debug.LogError("TopArmContainer is not assigned in " + gameObject.name);
        }
    }

    void Update()
    {
        // 如果正在摔落，模拟沿Z轴正向的重力
        if (isFalling && TopArmContainer != null)
        {
            // 增加Z轴速度（模拟重力加速度）
            currentZVelocity += gravityAcceleration * Time.deltaTime;

            // 更新位置（沿Z轴正向移动）
            Vector3 newPosition = transform.position;
            float targetZ = TopArmContainer.position.z;
            newPosition.z += currentZVelocity * Time.deltaTime;

            // 检查是否达到或超过 TopArmContainer 的Z轴位置
            if (newPosition.z >= targetZ)
            {
                newPosition.z = targetZ; // 固定到目标Z位置
                isFalling = false; // 停止摔落
                currentZVelocity = 0f; // 重置速度
                Debug.Log(gameObject.name + " stopped falling at TopArmContainer Z position");
            }

            transform.position = newPosition;
        }
    }

    // 摔落函数，在释放时被调用
    public void Fall()
    {
        if (rb != null)
        {
            rb.isKinematic = false; // 解除运动学状态
        }

        // 初始化摔落状态
        isFalling = true;
        currentZVelocity = fallSpeed; // 设置初始Z轴速度
        Debug.Log(gameObject.name + " started falling along Z+ direction");
    }

    // 可选：检测碰撞（保留以防需要）
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isFalling)
        {
            isFalling = false;
            currentZVelocity = 0f;
            Debug.Log(gameObject.name + " stopped falling due to collision");
        }
    }
}