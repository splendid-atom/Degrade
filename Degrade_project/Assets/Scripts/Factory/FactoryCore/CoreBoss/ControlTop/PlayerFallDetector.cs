using UnityEngine;
using System.Collections;

public class PlayerFallDetector : MonoBehaviour
{
[Header("形状投射设置")]
    [Tooltip("地板块使用的标签")]
    [SerializeField] private string floorTag = "Floor";

    [Tooltip("【重要】检测框中心相对于玩家中心点的【世界坐标】偏移量 (用于对准视觉脚底)")]
    [SerializeField] private Vector3 boxCenterWorldOffset = new Vector3(0f, -0.5f, 0f); // 需要仔细调整 Y 和 Z!

    [Tooltip("【重要】检测框的半边长 (X:左右, Y:厚度, Z:前后) (用于匹配视觉脚底范围)")]
    [SerializeField] private Vector3 boxHalfExtents = new Vector3(0.2f, 0.1f, 0.2f); // 需要仔细调整 X 和 Z!

    [Tooltip("向下投射的最大距离 (从 Box 中心开始算)")]
    [SerializeField] private float maxDistance = 0.2f; // 保持较小值，增加灵敏度

    [Tooltip("只检测特定层级的物体 (推荐!)")]
    [SerializeField] private LayerMask floorLayerMask; // 在Inspector中选择 Floor 层

    [Tooltip("是否在编辑器中绘制检测形状 (用于调试)")]
    [SerializeField] private bool drawDebugBox = true;

    [Header("坠落效果设置")]
    [SerializeField] private float fallDepth = 10f;
    [SerializeField] private float fallSpeed = 15f;

    // 内部状态
    private bool isFalling = false;
    private PlayerController playerController;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        if (playerController == null) { Debug.LogWarning("PlayerController not found."); }

        if (floorLayerMask == 0)
        {
             floorLayerMask = LayerMask.GetMask("Floor");
             if (floorLayerMask == 0) { Debug.LogError("Layer 'Floor' not found!"); }
             else { Debug.LogWarning("Floor Layer Mask auto-set to 'Floor'."); }
        }
         Debug.Log($"PlayerFallDetector Start: Player Layer={LayerMask.LayerToName(gameObject.layer)}, Checking Floor Layer={Mathf.Log(floorLayerMask.value, 2)}");
    }

    void Update()
    {
        if (isFalling) return;
        CheckForGroundWithBox();
    }

    // 使用 BoxCast 检查玩家脚下是否有地面
    void CheckForGroundWithBox()
    {
        // 1. 计算 Box 的【世界坐标】中心点: 玩家世界位置 + 世界坐标偏移量
        Vector3 boxCenter = transform.position + boxCenterWorldOffset;

        // 2. 确定投射方向: 世界空间的 Z 轴正方向 (物理下方)
        Vector3 direction = Vector3.forward;

        RaycastHit hit;
        bool groundDetected = false;

        // 3. 执行 BoxCast
        if (Physics.BoxCast(boxCenter, boxHalfExtents, direction, out hit, transform.rotation, maxDistance, floorLayerMask))
        {
            groundDetected = true;
        }
        else
        {
            groundDetected = false;
        }

        // 4. 绘制调试 Box (这个将在 OnDrawGizmos 中处理)

        // 5. 打印日志 (简化版，方便快速查看结果)
        string hitInfo = groundDetected ? $"Hit Floor Layer: {hit.collider?.name}" : "Hit Nothing"; // 添加 null 检查
        Debug.Log($"PlayerFall BoxCast Check: DetectedFloor={groundDetected}, Result='{hitInfo}'");

        // 6. 如果没有检测到地面，触发坠落 (需要解除注释)
        if (!groundDetected)
        {
             Debug.LogWarning("Ground not detected by BoxCast! Triggering Fall.");
              StartFalling(); // <--- 调整好参数后解除注释
        }
    }

    // --- 用于绘制调试 Box 的方法 ---
    void OnDrawGizmos()
    {
        if (!drawDebugBox || !Application.isPlaying) return; // 只在运行时且勾选了选项时绘制

        // 使用与 BoxCast 完全相同的参数来计算位置和方向
        Vector3 boxCenter = transform.position + boxCenterWorldOffset;
        Vector3 direction = Vector3.forward;
        Quaternion orientation = transform.rotation; // 使用玩家当前的旋转

        // 模拟 BoxCast 的检测来决定颜色
        bool groundDetectedGizmo = Physics.BoxCast(boxCenter, boxHalfExtents, direction, orientation, maxDistance, floorLayerMask);

        // 设置颜色
        Gizmos.color = groundDetectedGizmo ? Color.green : Color.red;

        // --- 绘制 BoxCast 的路径和终点 ---
        // Gizmos 没有直接绘制 BoxCast 的函数，我们用线框 Cube 近似模拟

        // 绘制起点 Box
        DrawWireCube(boxCenter, boxHalfExtents * 2, orientation);

        // 计算并绘制终点 Box (如果未碰撞，则在最大距离处；如果碰撞，理论上应在碰撞点)
        // 为了简化，我们始终在最大距离处绘制终点框，颜色表示是否碰撞
        Vector3 endCenter = boxCenter + direction * maxDistance;
        DrawWireCube(endCenter, boxHalfExtents * 2, orientation);

        // 可以画线连接两个 Box 的中心，表示检测路径
        Gizmos.DrawLine(boxCenter, endCenter);
    }

    // 辅助函数：绘制带旋转的线框 Cube
    void DrawWireCube(Vector3 center, Vector3 size, Quaternion rotation)
    {
        Matrix4x4 matrix = Matrix4x4.TRS(center, rotation, size);
        Gizmos.matrix = matrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one); // 在变换后的局部空间绘制单位立方体
        Gizmos.matrix = Matrix4x4.identity; // 恢复默认矩阵
    }


    // --- StartFalling 和 FallingSequence 保持不变 ---
    void StartFalling()
    {
        if (isFalling) return;
        isFalling = true;
        Debug.Log("玩家脚下无地面，开始坠落！");
        if (playerController != null) { /* 禁用控制 */ }
        StartCoroutine(FallingSequence());
    }

    IEnumerator FallingSequence()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = startPosition + Vector3.forward * fallDepth;
        float timeElapsed = 0f;
        float fallDuration = (fallSpeed > 0) ? fallDepth / fallSpeed : 0f;
        while (timeElapsed < fallDuration) {
            transform.position = Vector3.Lerp(startPosition, targetPosition, timeElapsed / fallDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPosition;
        if (playerController != null) { /* 死亡 */ } else { gameObject.SetActive(false); }
    }
}