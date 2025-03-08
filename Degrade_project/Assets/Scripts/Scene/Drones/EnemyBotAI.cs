using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBotAI : Enemy {

    [Header("巡逻点设置")]
    public Transform[] patrolPoints;       // 可以在 Inspector 拖拽多个巡逻点
    private int currentPatrolIndex = 0;    // 当前要去的巡逻点索引

    [Header("玩家参考")]
    public Transform player;

    [Header("视野设置")]
    public float viewAngle = 80f;         // 视野角度
    public float viewRadius = 7f;         // 视野半径
    public float detectionDelay = 0.2f;   // 检测延时

    // 内部计时器
    private float detectionTimer = 0f;

    // 用于保存当前真正的“前进方向”（由移动目标决定）
    private Vector2 currentForwardDir = Vector2.right;
    private float previousX; // 存储上一帧的 x 坐标
    public float moveSpeed = 2f;
    Transform target;
    Animator animator;


    void Start() {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        previousX = transform.position.x;  // 初始化为敌人当前位置的 x 坐标
    }

    void Update() {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < viewRadius) {
            ChargeAttack();
        } else {
            Patrol();
        }
        // float currentX = transform.position.x; // 当前 x 坐标
        // // 判断 x 坐标是否增加或减少
        // if (currentX > previousX) {
        //     animator.SetFloat("Move X", 1);
        // } else if (currentX < previousX) {
        //     animator.SetFloat("Move X", 0);
        // }
        // // 更新 previousX 为当前 x 坐标，供下次比较
        // previousX = currentX;
        TurnDirection();
    }

    void FixedUpdate(){
            

    }


    void ChargeAttack() {
        // 实现冲刺攻击：快速向玩家方向移动并可能附带伤害逻辑
        Vector2 direction = (player.position - transform.position).normalized;
        transform.Translate(direction * moveSpeed * 2 * Time.deltaTime);
        //Debug.Log("机器狗正在冲刺攻击");
    }
    public void Move()
    {
        // 每帧都先尝试更新当前 ForwardDir（前进方向），
        // 如果没有巡逻点、或正在追击玩家，都要根据实际移动目标来更新。
        // 下面的逻辑示例仅供参考，可根据自己代码结构进行调整。

        if (IsPlayerInFOV())
        {
            detectionTimer += Time.deltaTime;
            if (detectionTimer >= detectionDelay)
            {
                // 检测到玩家且达到延时条件 -> 追击玩家
                target = player;
                moveSpeed = 2.4f;
                ChaseTarget();
            }
            else
            {
                // 尚未达检测延时，可进行警戒
                moveSpeed = 2f;
                Patrol();
            }
        }
        else
        {
            // 未发现玩家
            detectionTimer = 0f;
            Patrol();
        }
    }

    /// <summary>
    /// 判断玩家是否处于敌人扇形视野内（基于当前前进方向）。
    /// </summary>
    /// <returns></returns>
    private bool IsPlayerInFOV()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // 如果玩家超过视野半径，直接返回 false
        if (distanceToPlayer > viewRadius) return false;

        // 先归一化
        toPlayer.Normalize();

        // 计算角度：使用当前“前进方向”和“朝向玩家”向量之间的夹角
        float angle = Vector2.Angle(currentForwardDir, toPlayer);

        // 若夹角在可视域（视野角度的一半）内，则视为检测到
        return (angle < viewAngle * 0.5f);
    }

    /// <summary>
    /// 多点巡逻逻辑：依次前往 patrolPoints 中的点，到达后切换到下一个巡逻点。
    /// </summary>
    private void Patrol()
    {
        // 如果没有设置巡逻点，直接返回
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // 确保 currentPatrolIndex 不超范围
        if (currentPatrolIndex >= patrolPoints.Length)
        {
            currentPatrolIndex = 0;
        }

        // 当前的巡逻点
        Transform patrolTarget = patrolPoints[currentPatrolIndex];

        // 移动到当前巡逻点
        transform.position = Vector2.MoveTowards(transform.position,
                                                 patrolTarget.position,
                                                 moveSpeed * Time.deltaTime);

        // 更新前进方向（基于当前位置到目标点）
        Vector2 newDir = (patrolTarget.position - transform.position);
        if (newDir.sqrMagnitude > 0.0001f)
        {
            currentForwardDir = newDir.normalized;
        }

        // 如果到达巡逻点，切换到下一个
        float distance = Vector2.Distance(transform.position, patrolTarget.position);
        if (distance < 0.01f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        TurnDirection();
    }

    /// <summary>
    /// 追击玩家逻辑。
    /// </summary>
    private void ChaseTarget()
    {
        if (target == null) return;

        transform.position = Vector2.MoveTowards(transform.position,
                                                 target.position,
                                                 moveSpeed * Time.deltaTime);

        Vector2 newDir = (target.position - transform.position);
        if (newDir.sqrMagnitude > 0.0001f)
        {
            currentForwardDir = newDir.normalized;
        }

        TurnDirection();
    }

    /// <summary>
    /// 调试绘制视野扇形区域（基于 currentForwardDir）。
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // 如果在编辑器里暂停时，currentForwardDir 可能为 0 向量，给个默认值
        Vector3 forwardDir = (currentForwardDir.sqrMagnitude < 0.001f) ? Vector3.right : (Vector3)currentForwardDir;

        // 扇形边界
        float halfAngle = viewAngle * 0.5f;

        Gizmos.color = Color.red;

        // 左右边界线
        Quaternion leftRotation = Quaternion.AngleAxis(-halfAngle, Vector3.forward);
        Quaternion rightRotation = Quaternion.AngleAxis(halfAngle, Vector3.forward);
        Vector3 leftBoundary = leftRotation * forwardDir;
        Vector3 rightBoundary = rightRotation * forwardDir;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary.normalized * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary.normalized * viewRadius);

        // 中心线
        Gizmos.DrawLine(transform.position, transform.position + forwardDir.normalized * viewRadius);

        // 选画一些辐射线以体现扇形范围
        float angleStep = 3f;
        for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir = rotation * forwardDir;
            Gizmos.DrawLine(transform.position, transform.position + dir.normalized * viewRadius);
        }
    }

    /// <summary>
    /// 根据 currentForwardDir.x 来判断左右朝向，如果和 isFacingRight 不一致则翻转角色
    /// </summary>
    
    bool isFacingRight = true;
    private void TurnDirection()
    {
        // 如果往右走，currentForwardDir.x 会大于 0；往左走则小于 0
        if (currentForwardDir.x > 0 && !isFacingRight)
        {
            animator.SetFloat("Move X", 1);
            Flip();
        }
        else if (currentForwardDir.x < 0 && isFacingRight)
        {
            animator.SetFloat("Move X", 0);
            Flip();
        }
    }

    /// <summary>
    /// 实际执行水平翻转的函数
    /// </summary>
    private void Flip()
    {
        // 先切换朝向布尔值
        isFacingRight = !isFacingRight;

        // 再把本地坐标的 x 缩放取反，实现水平方向翻转
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }



}
