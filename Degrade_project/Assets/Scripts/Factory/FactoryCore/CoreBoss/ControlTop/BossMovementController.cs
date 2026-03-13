using UnityEngine;
using System.Collections.Generic; // 需要 List
using System.Linq; // 可能需要 Linq (虽然下面没直接用，但保留可能有用)

public class BossMovementController : MonoBehaviour
{
    [Header("传送设置")]
    [Tooltip("玩家靠近到这个距离时触发传送")]
    [SerializeField] private float teleportMinDistance = 5.0f;
    [Tooltip("【新】传送后与玩家的最小安全距离")]
    [SerializeField] private float minTeleportDistance = 10.0f;
    [Tooltip("【新】存储不同阶段活动范围的 PolygonCollider2D 列表 (索引0对应阶段1, 索引1对应阶段2, ...)")]

     [SerializeField] private float MaxTeleportDistance = 20.0f; // 最大传送距离
    [SerializeField] private List<BoxCollider2D> phasePolygons;
    [Tooltip("【新】寻找安全传送点的最大尝试次数")]
    [SerializeField] private int maxTeleportAttempts = 50; // 防止死循环


    [Header("闲置移动设置 (可选)")]
    [SerializeField] private float idleMoveSpeed = 1.0f;
    [SerializeField] private Vector2 idleMoveRange = new Vector2(3f, 3f); // 闲置移动范围 (相对初始位置)
    [SerializeField] private float idleMoveChangeDirectionTime = 5.0f;

    // --- 内部变量 ---
    private Transform playerTransform;
    private BossController bossController; // 获取BossController引用
    private Vector3 startingPosition; // Boss 初始位置
    private Vector3 idleTargetPosition; // 闲置移动目标
    private float idleTimer = 0f;
    private bool isMoving = false; // 移动状态

    void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        bossController = GetComponent<BossController>();
        startingPosition = transform.position; // 记录初始位置，包括初始Z坐标
        SetNewIdleTarget(); // 初始化闲置目标

        // --- 检查必要组件 ---
        if (playerTransform == null) Debug.LogError("找不到玩家对象 ('Player' tag)!", this);
        if (bossController == null) Debug.LogError("找不到 BossController 组件!", this);
        if (phasePolygons == null || phasePolygons.Count == 0)
        {
            Debug.LogError("【错误】未分配阶段活动范围 (Phase Polygons)!", this);
            enabled = false; // 没有范围无法传送，禁用脚本
            return;
        }
        // 检查列表中的 PolygonCollider 是否都已赋值
        for(int i = 0; i < phasePolygons.Count; i++)
        {
            if(phasePolygons[i] == null)
            {
                 Debug.LogError($"【错误】Phase Polygons 列表中索引 {i} 处的 PolygonCollider2D 未赋值!", this);
                 enabled = false;
                 return;
            }
        }
    }

    void Update()
    {
        // 如果脚本禁用、找不到玩家/Boss控制器、或Boss眩晕，则不执行
        if (!this.enabled || playerTransform == null || bossController == null || bossController.isStunned)
        {
            isMoving = false;
            return;
        }

        // 1. 检查玩家距离，触发传送
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (distanceToPlayer < teleportMinDistance)
        {
            TriggerTeleport();
            isMoving = false; // 传送瞬间不算移动
            return; // 传送后，本帧不再执行其他移动逻辑
        }

        // 2. (可选) 执行闲置移动
        PerformIdleMovement();
    }

    // 触发传送
    void TriggerTeleport()
    {
        Vector3 targetPosition = FindSafeTeleportLocation();

        // 检查是否找到了有效位置 (不再是 Vector3.zero 判断，因为 (0,0,Z) 可能是有效位置)
        if (targetPosition != startingPosition) // 使用一个不太可能的目标位置作为失败标识，或者引入布尔返回值
        {
            // 调用 BossController 的传送方法，统一处理特效和位置更新
            bossController.Teleport(targetPosition);
            SetNewIdleTarget(); // 传送后重置闲置目标点
        }
        else
        {
            Debug.LogWarning("在当前区域内找不到安全的传送位置！Boss 将保持原地不动。", this);
            // 可以选择执行一个小的后跳或其他动作，避免完全不动
        }
    }

    // 【核心修改】在当前阶段的 Polygon 内寻找一个安全的随机传送点
    Vector3 FindSafeTeleportLocation()
    {
        if (bossController == null) return startingPosition; // 控制器丢失则失败

        int currentPhase = bossController.currentPhase;
        // 确保阶段编号有效，并能对应到列表索引 (阶段1 -> 索引0)
        int polygonIndex = currentPhase - 1;

        if (polygonIndex < 0 || polygonIndex >= phasePolygons.Count)
        {
            Debug.LogError($"当前阶段 {currentPhase} 没有对应的 PolygonCollider (索引 {polygonIndex} 无效)!", this);
            return startingPosition; // 返回一个明确的失败指示，比如初始位置
        }

        BoxCollider2D currentPolygon = phasePolygons[polygonIndex];
        if (currentPolygon == null) // 再次检查，以防万一
        {
             Debug.LogError($"索引 {polygonIndex} 处的 PolygonCollider2D 为空!", this);
             return startingPosition;
        }

        Bounds bounds = currentPolygon.bounds; // 获取多边形的边界框

        for (int i = 0; i < maxTeleportAttempts; i++)
        {
            // 1. 在边界框内生成随机 XY 点
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 randomPointXY = new Vector2(randomX, randomY);

            // 2. 检查点是否真的在多边形内部
            if (currentPolygon.OverlapPoint(randomPointXY))
            {
                // 3. 检查点与玩家的距离是否安全
                //    构建 Vector3，使用多边形所在 GameObject 的 Z 坐标，或 Boss 的初始 Z 坐标
                //float targetZ = currentPolygon.transform.position.z; // 使用多边形对象的Z坐标
                 float targetZ = startingPosition.z; // 如果 Boss 始终保持初始高度
                Vector3 potentialTarget = new Vector3(randomPointXY.x, randomPointXY.y, targetZ);

                if (Vector3.Distance(potentialTarget, playerTransform.position) >= minTeleportDistance && Vector3.Distance(potentialTarget, playerTransform.position) <= MaxTeleportDistance)
                {
                    // 找到了一个既在多边形内又距离玩家足够远的点！
                    Debug.Log($"找到安全传送点: {potentialTarget} (尝试次数: {i + 1})");
                    return potentialTarget;
                }
                // else: 点在多边形内，但离玩家太近，继续尝试
            }
            // else: 点在边界框内，但不在多边形内，继续尝试
        }

        // 尝试了 N 次仍然没有找到合适的点
        Debug.LogWarning($"尝试了 {maxTeleportAttempts} 次，未能找到阶段 {currentPhase} 区域内安全的传送点。", this);
        return startingPosition; // 返回初始位置表示失败
    }


    // (可选) 闲置时的随机移动 (保持不变)
    void PerformIdleMovement()
    {
        // 确保闲置移动的目标点也在当前活动区域内 (可选优化)
        // 可以在 SetNewIdleTarget 中加入检查，或者简化为就在小范围内移动

        idleTimer += Time.deltaTime;
        // 如果到达目标点 或 计时器超时，设置新目标
        if (Vector3.Distance(transform.position, idleTargetPosition) < 0.1f || idleTimer > idleMoveChangeDirectionTime)
        {
            SetNewIdleTarget();
            idleTimer = 0f;
        }

        // 朝着目标点缓慢移动
        transform.position = Vector3.MoveTowards(transform.position, idleTargetPosition, idleMoveSpeed * Time.deltaTime);
        // 更新移动状态标志
        isMoving = (Vector3.Distance(transform.position, idleTargetPosition) > 0.1f);
    }

    // 设置新的闲置移动目标点 (保持不变, 或可优化使其在当前Polygon内)
    void SetNewIdleTarget()
    {
        // 当前实现在初始位置附近随机选点
        float randomX = startingPosition.x + Random.Range(-idleMoveRange.x / 2, idleMoveRange.x / 2);
        float randomY = startingPosition.y + Random.Range(-idleMoveRange.y / 2, idleMoveRange.y / 2);
        // Z坐标保持 Boss 的初始Z值
        idleTargetPosition = new Vector3(randomX, randomY, startingPosition.z);

        // 【可选优化】可以检查这个 idleTargetPosition 是否在当前的 phasePolygon 内
        // 如果不在，则重新生成，直到找到一个在区域内的点，但这可能增加复杂性
        // 或简化为：闲置移动只在非常小的范围内，不太可能移出区域
    }

    // 公共接口，让BossController知道Boss是否在移动 (保持不变)
    public bool IsMoving()
    {
        return isMoving;
    }
}