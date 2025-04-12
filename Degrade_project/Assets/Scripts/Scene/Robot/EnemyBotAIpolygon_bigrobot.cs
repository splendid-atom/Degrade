using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minimalist.Quantity;
public class EnemyBotAIpolygon_bigrobot : Enemy3
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
    private Vector2 currentForwardDir;
    private float previousX;
    public float moveSpeed = 2f;
    private float chaseSpeedMultiplier = 0.6f;  // 追击速度乘数
    private Transform target;
    private Animator animator;
    public Vector2 initialDirection; // 初始动画方向
    
    // 状态控制
    private enum DroneState { Patrol, Hover, Attack, Chase };
    private DroneState currentState = DroneState.Patrol;
    private float stateTimer = 0f;
    private bool playerDetected = false;
    
    // 添加朝向锁定变量，防止在攻击时抽搐
    private float facingLockTimer = 0f;
    private float facingLockDuration = 0.5f; // 锁定朝向的持续时间
    private bool facingLocked = false;
    // 引用 DronesAttack 类的实例
    private DronesAttack dronesAttack;
    // 确定水平偏移方向 
    public float horizontalOffset = 2f; 
    private Vector3 BigRobotPosition;

    void Awake()
    {
        // rb = GetComponent<Rigidbody2D>();
        if(patrolAreaCollider==null){
            patrolAreaCollider = GameObject.Find("RobotPatrolArea").GetComponent<PolygonCollider2D>();
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        // 获取 DronesAttack 组件
        dronesAttack = GetComponent<DronesAttack>();
        if (patrolAreaCollider != null)
        {
            // 获取碰撞器的边界
            patrolStartPoint = patrolAreaCollider.bounds.min;
            patrolEndPoint = patrolAreaCollider.bounds.max;

            // 设置初始动画方向
            if (initialDirection.sqrMagnitude > 0.001f) 
            {
                currentForwardDir = initialDirection.normalized;
            }

            // UpdateAnimatorDirection(currentForwardDir);
            SetRandomPatrolTarget();
        }        
    }

    void Start()
    {
        BigRobotPosition = new Vector3(-38.4f,-26.9f,0.08f);

    }

    void Update()
    {
        HealthDisplaySetting();
        if (gameObject.name == "BigRobot" && PlayerController.Instance.PlayerHealth <= 0)
        {
            transform.position = BigRobotPosition;
        }
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
        
        // 状态机逻辑
        switch (currentState)
        {
            case DroneState.Patrol:
                Patrol();
                // 如果检测到玩家，或者玩家在攻击范围内，切换到悬停状态
                if (playerDetected || playerInAttackRange)
                {
                    currentState = DroneState.Hover;
                    SetAnimationAimming(); // 侦测到玩家时播放瞄准动画

                }
                break;
                
            case DroneState.Hover:
                HoverAbovePlayer();
                // 如果玩家既不在视野范围内也不在攻击范围内，返回巡逻
                if (!playerDetected && !playerInAttackRange)
                {
                    currentState = DroneState.Patrol;
                    SetAnimationIdling(); // 离开视野时播放空闲动画
                }
                // 如果悬停到位，准备攻击
                else if (IsInPosition())
                {
                    currentState = DroneState.Attack;
                    // 锁定朝向，防止抽搐
                    SetFacingDirection(player.position.x > transform.position.x);
                }
                break;
                
            case DroneState.Attack:
                AttackPlayer();
                // 如果玩家既不在视野范围内也不在攻击范围内，返回巡逻
                if (!playerDetected && !playerInAttackRange)
                {
                    currentState = DroneState.Patrol;
                    SetAnimationIdling(); // 离开视野时播放空闲动画
                }
                // 如果玩家超出攻击范围但在视野内，开始计时
                else if (!playerInAttackRange)
                {
                    stateTimer = 0f;
                    currentState = DroneState.Chase;
                }
                break;
                
            case DroneState.Chase:
                stateTimer += Time.deltaTime;
                // 延迟追击
                if (stateTimer < attackDelayTime)
                {
                    // 原地等待，但不频繁改变朝向
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
                
                // 如果玩家回到攻击范围内，继续攻击
                if (playerInAttackRange)
                {
                    currentState = DroneState.Attack;
                    // 锁定朝向，防止抽搐
                    SetFacingDirection(player.position.x > transform.position.x);
                }
                // 如果玩家既不在视野范围内也不在攻击范围内，返回巡逻
                else if (!playerDetected && !playerInAttackRange)
                {
                    currentState = DroneState.Patrol;
                    SetAnimationIdling();
                }
                break;
        }

        // 更新敌人朝向的动画
        // UpdateAnimatorDirection(currentForwardDir);
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
        return Vector2.Distance(transform.position, hoverPosition) < 0.1f;
    }
    
    // 获取玩家头顶的悬停位置
    private Vector2 GetHoverPosition()
    {

    // 水平偏移量，可以根据需要调整 
    // 如果无人机在玩家左边，悬停位置偏左 
    // 如果无人机在玩家右边，悬停位置偏右 
    if (transform.position.x < player.position.x) 
    { // 无人机在玩家左边，悬停位置设置在玩家头顶偏左 
    return (Vector2)player.position + Vector2.up * hoverHeight - Vector2.right * horizontalOffset; } 
    else { 
        // 无人机在玩家右边，悬停位置设置在玩家头顶偏右 
        return (Vector2)player.position + Vector2.up * hoverHeight + Vector2.right * horizontalOffset; }
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
        
        transform.position = Vector2.MoveTowards(transform.position, hoverPosition, moveSpeed * 1.5f * Time.deltaTime);
        
        currentForwardDir = direction;
        
        // 只有在没有锁定朝向时才更新朝向
        if (!facingLocked)
        {
            FacePlayer();
        }
    }
    
    // 攻击玩家
    private void AttackPlayer()
    {
        // 触发 DronesAttack 的攻击方法
        if (dronesAttack != null && !dronesAttack.isFiring)
        {
            dronesAttack.setFiring();
        }

        // 确保朝向正确
        if (!facingLocked)
        {
            SetFacingDirection(player.position.x > transform.position.x);
        }
    }

    
    // 追击玩家
    private void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, player.position, 
            moveSpeed * chaseSpeedMultiplier * Time.deltaTime);
        
        currentForwardDir = direction;
        
        // 只有在没有锁定朝向时才更新朝向
        if (!facingLocked)
        {
            FacePlayer();
        }
        
        
    }
    public void SetAnimationIdling(){
        animator.SetBool("isMoving", false);
        animator.SetBool("isReadyAttacking", false);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", true);
    }
    public void SetAnimationAimming(){
        animator.SetBool("isMoving", false);
        animator.SetBool("isReadyAttacking", true);
        animator.SetBool("isAttacking", false);
        animator.SetBool("isReturning", false);

    }
    // 面向玩家
    private void FacePlayer()
    {
        Vector2 dirToPlayer = player.position - transform.position;
        bool shouldFaceRight = dirToPlayer.x > 0;
        
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
            // 更新朝向后锁定一小段时间
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
        return (angle < viewAngle * 0.5f);
    }

    private void Patrol()
    {
        if (patrolAreaCollider == null)
            return;

        // 移动到当前的巡逻目标
        transform.position = Vector2.MoveTowards(transform.position, patrolTarget, moveSpeed * Time.deltaTime);
        // Vector2 newPos = Vector2.MoveTowards(rb.position, patrolTarget, moveSpeed * Time.fixedDeltaTime);
        // rb.MovePosition(newPos);

        // 更新前进方向
        Vector2 newDir = patrolTarget - (Vector2)transform.position;
        if (newDir.sqrMagnitude > 0.001f)
        {
            currentForwardDir = newDir.normalized;
        }

        // 更新动画
        // UpdateAnimatorDirection(currentForwardDir);

        // 如果到达巡逻目标，选择新的巡逻目标
        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f)
        {
            SetRandomPatrolTarget();
        }

        // 只有在没有锁定朝向时才更新朝向
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

    private void OnDrawGizmosSelected()
    {
        // 绘制视野范围
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
        
        // 绘制攻击范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    //关于血量显示的设置
    private void HealthDisplaySetting(){
        direction = transform.localScale.x > 0 ? 1 : -1;
        // 确保 HealthDisplay 的 x 轴缩放方向与 direction 一致
        HealthDisplay.localScale = new Vector3(
            Mathf.Abs(HealthDisplay.localScale.x) * direction, // 让 x 方向匹配 direction
            HealthDisplay.localScale.y,
            HealthDisplay.localScale.z
        );
        if (quantityBhv != null)
        {
            quantityBhv.Amount = currentHealth;
        }
    }
}