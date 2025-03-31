using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyCanon : MonoBehaviour
{
    [Header("攻击设置")]
    [SerializeField] private float attackRadius = 5f;         // 子弹伤害判定半径
    [SerializeField] private float canonAttackRange = 30f;    // 大炮攻击范围半径
    [SerializeField] private float minAttackInterval = 1.5f;  // 最高攻击频率（秒）
    [SerializeField] private float maxAttackInterval = 10f;   // 最低攻击频率（秒）
    [SerializeField] private float warningDuration = 1.5f;    // 攻击预警持续时间
    [SerializeField] private float attackDamage = 10f;        // 攻击伤害
    [SerializeField] private GameObject energyBulletPrefab;   // 能量子弹预制体
    [SerializeField] private GameObject warningCirclePrefab;  // 预警圆圈预制体

    [Header("距离映射设置")]
    [SerializeField] private float minDistance = 5f;   // 最近距离（最高攻击频率）
    [SerializeField] private float maxDistance = 30f;  // 最远距离（最低攻击频率）
    [SerializeField] [Range(0.1f, 3f)] private float distanceScaleFactor = 1f;  // 距离映射系数

    [Header("护盾设置")]
    [SerializeField] private GameObject shield;  // 护盾对象
    [SerializeField] private List<GameObject> shieldGenerators = new List<GameObject>(4);  // 护盾发生器列表

    [Header("狂暴模式设置")]
    [SerializeField] private float rageAttackInterval = 0.1f;  // 狂暴模式攻击间隔
    [SerializeField] private int rageAttackCount = 3;         // 同时攻击点数量

    private Transform playerTransform;
    private float nextAttackTime;
    private bool isShieldActive = true;
    Animator animator; 
    public PolygonCollider2D polygonCollider2D;
    private void Start()
    {
        animator = GetComponent<Animator>();
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        nextAttackTime = Time.time + CalculateAttackInterval();

        if (polygonCollider2D == null)
        {
            Debug.LogError($"{gameObject.name}: PolygonCollider2D is not assigned!", this);
        }
    }

    private void Update()
    {
        if(isShieldActive){
            CheckShieldStatus(); 
        }
        if (isShieldActive && Time.time >= nextAttackTime)
        {
            StartCoroutine(ExecuteAttack());
            nextAttackTime = Time.time + CalculateAttackInterval();
        }
        if (!isShieldActive && Time.time >= nextAttackTime)
        {
            StartCoroutine(ExecuteRageAttack());            
            nextAttackTime = Time.time + rageAttackInterval;
        }
    }

    // 狂暴模式攻击
    private IEnumerator ExecuteRageAttack()
    {
        // isRageAttackInProgress = true;
        List<Vector3> attackPositions = new List<Vector3>();

        // 生成多个随机攻击位置，限制在 PolygonCollider2D 内
        for (int i = 0; i < rageAttackCount; i++)
        {
            Vector3 attackPos = GetRandomPointInPolygon();
            attackPositions.Add(attackPos);
        }

        List<GameObject> warningObjects = new List<GameObject>();
        foreach (Vector3 pos in attackPositions)
        {
            GameObject warningObject = Instantiate(warningCirclePrefab, pos, Quaternion.identity);
            SpriteRenderer warningRenderer = warningObject.GetComponent<SpriteRenderer>();
            if (warningRenderer != null)
            {
                StartCoroutine(PulseWarning(warningRenderer, warningDuration));
            }
            warningObjects.Add(warningObject);
        }

        yield return new WaitForSeconds(warningDuration);

        List<GameObject> energyBullets = new List<GameObject>();
        for (int i = 0; i < attackPositions.Count; i++)
        {
            Destroy(warningObjects[i]);
            if (energyBulletPrefab != null)
            {
                GameObject energyBullet = Instantiate(energyBulletPrefab, attackPositions[i], Quaternion.identity);
                energyBullets.Add(energyBullet);
            }

            float distanceToTarget = Vector2.Distance(
                new Vector2(playerTransform.position.x, playerTransform.position.y),
                new Vector2(attackPositions[i].x, attackPositions[i].y)
            );

            if (distanceToTarget <= attackRadius)
            {
                Debug.Log("狂暴模式：玩家在攻击范围内！");
                PlayerController.Instance.PlayerHealth-=50;
            }
        }

        float bulletAnimationDuration = 2f;
        yield return new WaitForSeconds(bulletAnimationDuration);

        foreach (GameObject bullet in energyBullets)
        {
            if (bullet != null) Destroy(bullet);
        }

        // isRageAttackInProgress = false;
    }

    // 检查攻击位置是否在大炮攻击范围内
    private bool IsPositionInAttackRange(Vector3 position)
    {
        float distance = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.y),
            new Vector2(position.x, position.y)
        );
        return distance <= canonAttackRange;
    }

    // 根据距离计算攻击间隔
    private float CalculateAttackInterval()
    {
        if (!playerTransform) return maxAttackInterval;

        // 只考虑2D平面距离（忽略z轴，考虑到2.5D效果是通过摄像机倾斜实现的）
        Vector2 canonPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        float distance = Vector2.Distance(canonPosition, playerPosition);

        // 应用距离缩放因子，使映射更加灵活
        distance = Mathf.Pow(distance, distanceScaleFactor);
        
        // 在最小和最大距离之间进行线性插值，计算攻击间隔
        float normalizedDistance = Mathf.Clamp01((distance - minDistance) / (maxDistance - minDistance));
        return Mathf.Lerp(minAttackInterval, maxAttackInterval, normalizedDistance);
    }

    private void CheckShieldStatus()
    {
        int destroyedGenerators = 0;
        
        foreach (GameObject generator in shieldGenerators)
        {
            if (generator == null || !generator.activeSelf)
            {
                destroyedGenerators++;
            }
        }

        // 如果所有护盾发生器都被销毁，禁用护盾
        if (destroyedGenerators >= shieldGenerators.Count && isShieldActive)
        {
            DisableShield();
        }
    }

    private void DisableShield()
    {
        isShieldActive = false;
        animator.SetBool("isShield", false);
    }

    private IEnumerator ExecuteAttack()
    {
        animator.SetBool("isShooting", true);
        if (!playerTransform || !IsPositionInAttackRange(playerTransform.position))
        {
            yield break;
        }

        // 使用玩家的位置，但确保在 PolygonCollider2D 内
        Vector3 targetPosition = playerTransform.position;
        Vector3 attackPosition = GetClosestPointInPolygon(targetPosition); // 如果玩家位置不在多边形内，找最近点
        attackPosition.z = 0.1f; // 设置固定 Z 值

        GameObject warningObject = Instantiate(warningCirclePrefab, attackPosition, Quaternion.identity);
        SpriteRenderer warningRenderer = warningObject.GetComponent<SpriteRenderer>();
        if (warningRenderer != null)
        {
            StartCoroutine(PulseWarning(warningRenderer, warningDuration));
        }

        yield return new WaitForSeconds(warningDuration);

        Destroy(warningObject);
        GameObject energyBullet = null;
        if (energyBulletPrefab != null)
        {
            energyBullet = Instantiate(energyBulletPrefab, attackPosition, Quaternion.identity);
        }

        float distanceToTarget = Vector2.Distance(
            new Vector2(playerTransform.position.x, playerTransform.position.y),
            new Vector2(attackPosition.x, attackPosition.y)
        );

        if (distanceToTarget <= attackRadius)
        {
            Debug.Log("玩家在攻击范围内！");
            PlayerController.Instance.PlayerHealth-=50;
        }

        float bulletAnimationDuration = 2f;
        yield return new WaitForSeconds(bulletAnimationDuration);

        if (energyBullet != null) Destroy(energyBullet);
        animator.SetBool("isShooting", false);
    }
    // 获取 PolygonCollider2D 内的随机点
    private Vector3 GetRandomPointInPolygon()
    {
        if (polygonCollider2D == null) return transform.position; // 回退到默认位置

        Bounds bounds = polygonCollider2D.bounds;
        int maxAttempts = 100; // 防止无限循环
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 point = new Vector2(randomX, randomY);

            if (polygonCollider2D.OverlapPoint(point))
            {
                return new Vector3(point.x, point.y, 0.1f); // 使用固定的 Z 值
            }
        }

        Debug.LogWarning($"{gameObject.name}: Failed to find point in PolygonCollider2D, returning center.", this);
        return polygonCollider2D.bounds.center; // 如果找不到，返回中心点
    }

    // 获取 PolygonCollider2D 内最近的点
    private Vector3 GetClosestPointInPolygon(Vector3 target)
    {
        if (polygonCollider2D == null) return target;
        if (polygonCollider2D.OverlapPoint(target)) return target; // 如果目标已在多边形内，直接返回

        Vector2 closestPoint = polygonCollider2D.ClosestPoint(target);
        return new Vector3(closestPoint.x, closestPoint.y, target.z);
    }   
    // 预警圆圈的脉动/闪烁效果
    private IEnumerator PulseWarning(SpriteRenderer renderer, float duration)
    {
        float startTime = Time.time;
        Color baseColor = renderer.color;
        
        while (Time.time - startTime < duration)
        {
            // 创建脉动效果
            float alpha = 0.3f + Mathf.PingPong((Time.time - startTime) * 2, 0.7f);
            renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
    }

    // 在Scene视图中绘制攻击范围
    private void OnDrawGizmos()
    {
        // 绘制大炮的攻击范围（以大炮为中心的圆形区域）
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);  // 半透明橙色
        Gizmos.DrawWireSphere(transform.position, canonAttackRange);
        
        // 绘制当前子弹判定半径（示例）
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);  // 半透明红色
        Vector3 examplePosition = transform.position;
        examplePosition.x += canonAttackRange * 0.5f; // 在攻击范围内显示一个示例判定圈
        Gizmos.DrawWireSphere(examplePosition, attackRadius);
    }
}