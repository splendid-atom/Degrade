using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minimalist.Quantity;

public class EnemyBotAIpolygon_new : Enemy3
{
    [Header("巡逻点设置")]
    public PolygonCollider2D patrolAreaCollider; // 用于限制巡逻的区域

    private Vector2 patrolStartPoint;
    private Vector2 patrolEndPoint;
    private Vector2 patrolTarget;

    [Header("玩家参考")]
    public Transform player;

    [Header("视野设置")]
    public float viewAngle = 80f;         // 视野角度
    public float viewRadius = 7f;         // 视野半径
    public float detectionDelay = 0.2f;   // 检测延时

    [Header("攻击设置")]
    public float attackRange = 4f;        // 攻击范围
    public float hoverHeight = 3f;        // 悬停在玩家上方的高度
    public float attackDelayTime = 2f;    // 玩家脱离攻击范围后，延迟追击的时间
    public bool isFacingRight = true;     // 是否面向右侧（可以手动调整初始值）

    [Header("Z 轴设置")]
    public float targetZHeight = 0f;      // 公开的 Z 轴高度，控制无人机保持的 Z 坐标

    private Vector2 currentForwardDir;
    private float previousX;
    public float moveSpeed = 2f;
    private float chaseSpeedMultiplier = 0.6f;  // 追击速度乘数
    private Transform target;
    private Animator animator;
    public Vector2 initialDirection; // 初始动画方向
    public float TrashScale = 5.544793f;
    // 状态控制
    private enum DroneState { Patrol, Hover, Attack, Chase ,Idle}
    private DroneState currentState = DroneState.Patrol;
    private float stateTimer = 0f;
    private bool playerDetected = false;

    // 添加朝向锁定变量，防止在攻击时抽搐
    private float facingLockTimer = 0f;
    private float facingLockDuration = 0.5f; // 锁定朝向的持续时间
    private bool facingLocked = false;

    // 引用 DronesAttack 类的实例
    private DronesAttack dronesAttack;

    void Awake()
    {
        patrolAreaCollider = GameObject.Find("DronesPatrolArea").GetComponent<PolygonCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        dronesAttack = GetComponent<DronesAttack>();
        if (patrolAreaCollider != null)
        {
            patrolStartPoint = patrolAreaCollider.bounds.min;
            patrolEndPoint = patrolAreaCollider.bounds.max;

            if (initialDirection.sqrMagnitude > 0.001f)
            {
                currentForwardDir = initialDirection.normalized;
            }

            UpdateAnimatorDirection(currentForwardDir);
            SetRandomPatrolTarget();
        }

        // 初始化 Z 轴高度
        SetZHeight();
    }

    void Start()
    {
        currentHealth = 100f;
        UpdateAnimatorDirection(currentForwardDir);
    }

    void Update()
    {
        HealthDisplaySetting();
        if (player == null)
            return;

        // 处理朝向锁定计时器
        if (facingLocked)
        {
            facingLockTimer += Time.deltaTime;
            if (facingLockTimer >= facingLockDuration)
            {
                facingLocked = false;
                facingLockTimer = 0f;
            }
        }

        // 检测玩家是否在视野范围内
        playerDetected = IsPlayerInFOV();

        // 检测玩家是否在攻击范围内，即使不在视野内
        bool playerInAttackRange = IsPlayerInAttackRange();
        
        if(!TrashEnemiesController.instance.isTrashEnemiesMovable){
            currentState = DroneState.Idle;
        }
        // 状态机逻辑
        switch (currentState)
        {
            case DroneState.Idle:
                if(TrashEnemiesController.instance.isTrashEnemiesMovable){
                    currentState = DroneState.Patrol;
                }
                break;
            case DroneState.Patrol:
                Patrol();
                if (playerDetected || playerInAttackRange)
                {
                    currentState = DroneState.Hover;
                    SetAnimationAimming();
                }
                break;

            case DroneState.Hover:
                HoverAbovePlayer();
                if (!playerDetected && !playerInAttackRange)
                {
                    currentState = DroneState.Patrol;
                    SetAnimationIdling();
                }
                else if (IsInPosition())
                {
                    currentState = DroneState.Attack;
                    SetFacingDirection(player.position.x > transform.position.x);
                }
                break;

            case DroneState.Attack:
                AttackPlayer();
                if (!playerDetected && !playerInAttackRange)
                {
                    currentState = DroneState.Patrol;
                    SetAnimationIdling();
                }
                else if (!playerInAttackRange)
                {
                    stateTimer = 0f;
                    currentState = DroneState.Chase;
                }
                break;

            case DroneState.Chase:
                stateTimer += Time.deltaTime;
                if (stateTimer < attackDelayTime)
                {
                    if (!facingLocked)
                    {
                        FacePlayer();
                        facingLocked = true;
                    }
                }
                else
                {
                    ChasePlayer();
                }

                if (playerInAttackRange)
                {
                    currentState = DroneState.Attack;
                    SetFacingDirection(player.position.x > transform.position.x);
                }
                else if (!playerDetected && !playerInAttackRange)
                {
                    currentState = DroneState.Patrol;
                    SetAnimationIdling();
                }
                break;
        }

        // 确保每帧更新 Z 轴高度
        SetZHeight();

        UpdateAnimatorDirection(currentForwardDir);
    }

    // 设置朝向并锁定，防止抽搐
    private void SetFacingDirection(bool faceRight)
    {
        if (isFacingRight != faceRight)
        {
            Flip();
        }
        facingLocked = true;
        facingLockTimer = 0f;
    }

    // 判断是否已经悬停到位
    private bool IsInPosition()
    {
        Vector2 hoverPosition = GetHoverPosition();
        float random_precision = Random.Range(0.3f, 1f);
        return Vector2.Distance(transform.position, hoverPosition) < random_precision;
    }

    // 获取玩家头顶的悬停位置（加入随机性）
    private Vector2 GetHoverPosition()
    {
        float horizontalOffset = 2f;
        Vector2 basePosition;

        if (transform.position.x < player.position.x)
        {
            basePosition = (Vector2)player.position + Vector2.up * hoverHeight - Vector2.right * horizontalOffset;
        }
        else
        {
            basePosition = (Vector2)player.position + Vector2.up * hoverHeight + Vector2.right * horizontalOffset;
        }

        // 添加随机偏移
        float randomRange = 0.5f;
        float offsetX = Random.Range(-randomRange, randomRange);
        float offsetY = Random.Range(-randomRange, randomRange);

        return basePosition + new Vector2(offsetX, offsetY);
    }

    // 检查玩家是否在攻击范围内
    private bool IsPlayerInAttackRange()
    {
        return Vector2.Distance(transform.position, player.position) <= attackRange;
    }

    // 悬停在玩家头顶
    private void HoverAbovePlayer()
    {
        Vector2 hoverPosition = GetHoverPosition();
        Vector2 direction = (hoverPosition - (Vector2)transform.position).normalized;

        // 计算新的 X 和 Y 坐标
        Vector2 newPos = Vector2.MoveTowards(transform.position, hoverPosition, moveSpeed * 1.5f * Time.deltaTime);

        // 更新位置，保持 Z 轴为 targetZHeight
        transform.position = new Vector3(newPos.x, newPos.y, targetZHeight);

        currentForwardDir = direction;

        if (!facingLocked)
        {
            FacePlayer();
        }
    }

    // 攻击玩家
    private void AttackPlayer()
    {
        if (dronesAttack != null && !dronesAttack.isFiring)
        {
            dronesAttack.setFiring();
        }

        if (!facingLocked)
        {
            SetFacingDirection(player.position.x > transform.position.x);
        }
    }

    // 追击玩家
    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // 计算新的 X 和 Y 坐标
        Vector2 newPos = Vector2.MoveTowards(transform.position, player.position,
            moveSpeed * chaseSpeedMultiplier * Time.deltaTime);

        // 更新位置，保持 Z 轴为 targetZHeight
        transform.position = new Vector3(newPos.x, newPos.y, targetZHeight);

        currentForwardDir = direction;

        if (!facingLocked)
        {
            FacePlayer();
        }
    }

    public void SetAnimationIdling()
    {
        animator.SetBool("isIdling", true);
        animator.SetBool("isAimming", false);
        animator.SetBool("isShoting", false);
    }

    public void SetAnimationAimming()
    {
        animator.SetBool("isIdling", false);
        animator.SetBool("isAimming", true);
        animator.SetBool("isShoting", false);
    }

    // 面向玩家
    private void FacePlayer()
    {
        Vector2 dirToPlayer = player.position - transform.position;
        bool shouldFaceRight = dirToPlayer.x > 0;

        if (shouldFaceRight != isFacingRight)
        {
            Flip();
            facingLocked = true;
            facingLockTimer = 0f;
        }
    }

    void UpdateAnimatorDirection(Vector2 moveDirection)
    {
        moveDirection.Normalize();
        float moveX = (moveDirection.x + 1) / 2; // 映射 [-1,1] 到 [0,1]
        animator.SetFloat("Move X", moveX);
        animator.SetFloat("Move Y", moveDirection.y);
    }

    private bool IsPlayerInFOV()
    {
        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        if (distanceToPlayer > viewRadius) return false;

        toPlayer.Normalize();
        float angle = Vector2.Angle(currentForwardDir, toPlayer);
        return angle < viewAngle * 0.5f;
    }

    private void Patrol()
    {
        if (patrolAreaCollider == null)
            return;

        // 计算目标位置（仅更新 X 和 Y）
        Vector2 newPos = Vector2.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);

        // 更新位置，保持 Z 轴为 targetZHeight
        transform.position = new Vector3(newPos.x, newPos.y, targetZHeight);

        Vector2 newDir = patrolTarget - newPos;
        if (newDir.sqrMagnitude > 0.001f)
        {
            currentForwardDir = newDir.normalized;
        }

        UpdateAnimatorDirection(currentForwardDir);

        if (Vector2.Distance(newPos, patrolTarget) < 0.1f)
        {
            SetRandomPatrolTarget();
        }

        if (!facingLocked)
        {
            TurnDirection(currentForwardDir.x);
        }

        SetAnimationIdling();
    }

    private void SetRandomPatrolTarget()
    {
        float randomX = Random.Range(patrolStartPoint.x, patrolEndPoint.x);
        float randomY = Random.Range(patrolStartPoint.y, patrolEndPoint.y);
        patrolTarget = new Vector2(randomX, randomY);
    }

    private void TurnDirection(float moveX)
    {
        bool shouldFaceRight = moveX > 0;

        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
    }

    // 设置 Z 轴高度
    private void SetZHeight()
    {
        Vector3 currentPos = transform.position;
        if (Mathf.Abs(currentPos.z - targetZHeight) > 0.001f)
        {
            transform.position = new Vector3(currentPos.x, currentPos.y, targetZHeight);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 forwardDir = (currentForwardDir.sqrMagnitude < 0.001f) ? Vector3.right : (Vector3)currentForwardDir;
        float halfAngle = viewAngle * 0.5f;

        Gizmos.color = Color.red;
        Quaternion leftRotation = Quaternion.AngleAxis(-halfAngle, Vector3.forward);
        Quaternion rightRotation = Quaternion.AngleAxis(halfAngle, Vector3.forward);
        Vector3 leftBoundary = leftRotation * forwardDir;
        Vector3 rightBoundary = rightRotation * forwardDir;

        Gizmos.DrawLine(transform.position, transform.position + leftBoundary.normalized * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary.normalized * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + forwardDir.normalized * viewRadius);

        float angleStep = 3f;
        for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir = rotation * forwardDir;
            Gizmos.DrawLine(transform.position, transform.position + dir.normalized * viewRadius);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void HealthDisplaySetting()
    {
        direction = transform.localScale.x > 0 ? 1 : -1;
        HealthDisplay.localScale = new Vector3(
            Mathf.Abs(HealthDisplay.localScale.x) * direction,
            HealthDisplay.localScale.y,
            HealthDisplay.localScale.z
        );
        if (quantityBhv != null)
        {
            quantityBhv.Amount = currentHealth;
        }
    }
    public void ResetPatrolArea()
    {
        if (patrolAreaCollider != null)
        {
            // 获取碰撞器的边界
            patrolStartPoint = patrolAreaCollider.bounds.min;
            patrolEndPoint = patrolAreaCollider.bounds.max;
            SetRandomPatrolTarget();
        }      
    }
    public void ResetScale(){
        transform.localScale = new Vector3(TrashScale,TrashScale,TrashScale);
    }
    public void SetViewRange(){
        viewRadius = viewRadius * 50;
        viewAngle = 360f;
    }
    private void SetRebornHeight(){
        transform.position = new Vector3(transform.position.x,transform.position.y,targetZHeight);
    }
    public void EnableAnimator(){
        animator.enabled = true;
    }
    public void SetRebornCondition(){
        ResetPatrolArea();
        ResetScale();
        SetViewRange();
        SetRebornHeight();
        EnableAnimator();
    }
}