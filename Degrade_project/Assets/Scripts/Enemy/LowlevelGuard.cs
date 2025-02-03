using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowlevelGuard : EnemyBaseBehaviour
{
    public Transform pathPoint1, pathPoint2, player;
    
    [Header("视野设置")]
    public float viewAngle = 80f;         // 视野角度
    public float viewRadius = 7f;         // 视野半径
    public float detectionDelay = 0.2f;   // 检测延时

    private float detectionTimer = 0f;

    protected override void Move()
    {
        if (IsPlayerInFOV())
        {
            detectionTimer += Time.deltaTime;
            if (detectionTimer >= detectionDelay)
            {
                // 检测到玩家且达到延时条件，追击玩家
                target = player;
                moveSpeed = 2.4f;
                ChaseTarget();
            }
            else
            {
                // 还在等待延时，可在此添加警戒动画、声音等效果
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

    // 判断玩家是否处于敌人扇形视野内
    private bool IsPlayerInFOV()
    {
        // 计算敌人到玩家的向量和距离
        Vector2 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        if(distanceToPlayer > viewRadius)
            return false;
        
        toPlayer.Normalize();
        
        // 使用 isFacingRight 来确定当前的朝向
        Vector2 facingDirection = isFacingRight ? Vector2.right : Vector2.left;
        
        // 计算两者之间的夹角
        float angle = Vector2.Angle(facingDirection, toPlayer);
        
        // 返回角度是否在允许的范围内
        return angle < viewAngle / 2f;
    }


    private void Patrol()
    {
        // 如果当前 target 既不是 pathPoint1 也不是 pathPoint2，则默认设为 pathPoint1
        if (target != pathPoint1 && target != pathPoint2)
        {
            target = pathPoint1;
        }

        // 判断是否到达目标巡逻点
        if (Vector2.Distance(transform.position, target.position) < 0.01f)
        {
            target = (target == pathPoint1) ? pathPoint2 : pathPoint1;
        }
        
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        TurnDirection();
    }

    private void ChaseTarget()
    {
        transform.position = Vector2.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
        TurnDirection();
    }

    private void OnDrawGizmosSelected()
    {
        // 假设 viewAngle 和 viewRadius 已定义
        // 使用 isFacingRight 判断朝向：true时向右，false时向左
        Vector3 facingDirection = isFacingRight ? Vector3.right : Vector3.left;

        // 计算视野角度的一半
        float halfAngle = viewAngle / 2f;
        
        // 绘制左右边界线
        Gizmos.color = Color.red;
        Quaternion leftRotation = Quaternion.AngleAxis(-halfAngle, Vector3.forward);
        Quaternion rightRotation = Quaternion.AngleAxis(halfAngle, Vector3.forward);
        Vector3 leftBoundary = leftRotation * facingDirection;
        Vector3 rightBoundary = rightRotation * facingDirection;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * viewRadius);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * viewRadius);

        // 用稀疏的辐射线填充扇形区域
        Gizmos.color = Color.red;
        float angleStep = 3.0f;
        for (float angle = -halfAngle; angle <= halfAngle; angle += angleStep)
        {
            Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            Vector3 dir = rotation * facingDirection;
            Gizmos.DrawLine(transform.position, transform.position + dir * viewRadius);
        }
    }



}
