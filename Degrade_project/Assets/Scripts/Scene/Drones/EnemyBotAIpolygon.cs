using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBotAIpolygon : Enemy
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

    public float AttackRange = 4f;
    private float detectionTimer = 0f;
    private Vector2 currentForwardDir;
    private float previousX;
    public float moveSpeed = 2f;
    Transform target;
    Animator animator;
    public Vector2 initialDirection; // 初始动画方向
    public float stopDistance = 3f; // 设定敌人到玩家的最小停止距离
    
    void Awake()
    {
        
        patrolAreaCollider = GameObject.Find("DronesPatrolArea").GetComponent<PolygonCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();

        if (patrolAreaCollider != null)
        {
            // 获取碰撞器的边界
            patrolStartPoint = patrolAreaCollider.bounds.min;
            patrolEndPoint = patrolAreaCollider.bounds.max;

            // 设置初始动画方向
            if (initialDirection != null)
            {
                // 如果initialDirection为左方向
                if (initialDirection.sqrMagnitude > 0.001f) 
                {
                    currentForwardDir = initialDirection.normalized;
                }

                UpdateAnimatorDirection(currentForwardDir);
                SetRandomPatrolTarget();
            }

        }        
    }

    void Start()
    {
        SetRandomPatrolTarget();
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < viewRadius)
        {
            ChargeAttack();
        }
        else
        {
            Patrol();
        }

        UpdateAnimatorDirection(currentForwardDir);

        
    }

    void Update()
    {
        if (player == null)
            return;

        float distance = Vector2.Distance(transform.position, player.position);
        if (distance < viewRadius)
        {
            ChargeAttack();
        }
        else
        {
            Patrol();
        }

        // 更新敌人朝向的动画
        UpdateAnimatorDirection(currentForwardDir);
    }
    void UpdateAnimatorDirection(Vector2 moveDirection) 
    {
        moveDirection.Normalize();
        
        float moveX = (moveDirection.x + 1) / 2; // 映射 [-1,1] 到 [0,1]
        animator.SetFloat("Move X", moveX);
        animator.SetFloat("Move Y", moveDirection.y);
    }

    void FixedUpdate()
    {
        // 这里可以加入物理相关的移动代码，若需要
    }

    void Attack()
    {
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            transform.Translate(direction * moveSpeed * 2 * Time.deltaTime);
            UpdateAnimatorDirection(direction);
            TurnDirection(direction.x);
        }
    }
    void ChargeAttack()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, player.position);

        // 计算敌人应该停在玩家上方的目标位置
        Vector2 targetPosition = (Vector2)player.position + Vector2.up * stopDistance + Vector2.right * Random.Range(-2f, 2f);

        // 只有在敌人距离目标位置较远时才移动
        if (Vector2.Distance(transform.position, targetPosition) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * 4 * Time.deltaTime);
            UpdateAnimatorDirection(direction);
            TurnDirection(direction.x);
        }
    }






    public void Move()
    {
        if (IsPlayerInFOV())
        {
            detectionTimer += Time.deltaTime;
            if (detectionTimer >= detectionDelay)
            {
                target = player;
                moveSpeed = 2.4f;
                ChaseTarget();
            }
            else
            {
                moveSpeed = 2f;
                Patrol();
            }
        }
        else
        {
            detectionTimer = 0f;
            Patrol();
        }
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

        // 更新前进方向
        Vector2 newDir = patrolTarget - (Vector2)transform.position;
        currentForwardDir = newDir.normalized;

        // 更新动画
        UpdateAnimatorDirection(currentForwardDir);

        // 如果到达巡逻目标，选择新的巡逻目标
        if (Vector2.Distance(transform.position, patrolTarget) < 0.01f)
        {
            SetRandomPatrolTarget();
        }

        TurnDirection(currentForwardDir.x);
    }

    private void SetRandomPatrolTarget()
    {
        float randomX = Random.Range(patrolStartPoint.x, patrolEndPoint.x);
        float randomY = Random.Range(patrolStartPoint.y, patrolEndPoint.y);
        patrolTarget = new Vector2(randomX, randomY);
    }


    private void ChaseTarget()
    {
        if (target == null) return;

        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);

        Vector2 newDir = patrolTarget - new Vector2(transform.position.x, transform.position.y);
        if (newDir.sqrMagnitude > 0.0001f)
        {
            currentForwardDir = newDir.normalized;
        }

        TurnDirection(currentForwardDir.x);
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
    }

    bool isFacingRight = true;
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
}
