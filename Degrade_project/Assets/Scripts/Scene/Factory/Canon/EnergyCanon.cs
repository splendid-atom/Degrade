using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minimalist.Quantity;
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
    [SerializeField] private Transform AlarmsContainer;
    [SerializeField] private Transform EnergyBulletsContainer;

    private Transform playerTransform;
    private float nextAttackTime;
    private bool isShieldActive = true;//护盾为false也代表进入rage模式
    Animator animator; 
    public PolygonCollider2D polygonCollider2D;
    public PolygonCollider2D polygonCollider2DRaged;
    public bool isTimeStopped = false;

    [Header("粒子系统控制")]
    [SerializeField] public bool isParticleSystemsPlaying = true; // 控制所有粒子系统的播放/暂停
    private List<ParticleSystem> energyBulletParticleSystems = new List<ParticleSystem>(); // 存储所有能量子弹的粒子系统
    private List<Coroutine> warningCoroutines = new List<Coroutine>(); // 存储警告闪烁的协程
    private List<Coroutine> activeCoroutines = new List<Coroutine>(); // 存储所有活动协程
    private bool wasTimeStopped = false; // 跟踪上一次的 isTimeStopped 状态
    public Enemy3 Enemy3;
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
        Enemy3.canTakeDamage=false;
    }

    private void Update()
    {
        if(!isTimeStopped){
            isParticleSystemsPlaying = true;
        }
        if (isTimeStopped)
        {
            isParticleSystemsPlaying = false; // 时间停止时强制暂停粒子系统
            UpdateParticleSystemsState(); // 更新粒子系统状态
            StopWarningPulse(); // 停止警告闪烁
            if (!wasTimeStopped)
            {
                PauseAllCoroutines(); // 暂停所有协程
                wasTimeStopped = true;
            }
            return; // 时间停止时跳过攻击逻辑
        }
        else if (wasTimeStopped)
        {
            ResumeAllCoroutines(); // 恢复所有协程
            wasTimeStopped = false;
        }

        if (isShieldActive)
        {
            CheckShieldStatus(); 
        }
        if (isShieldActive && Time.time >= nextAttackTime)
        {
            StartCoroutineWithTracking(ExecuteAttack());
            nextAttackTime = Time.time + CalculateAttackInterval();
        }
        if (!isShieldActive && Time.time >= nextAttackTime)
        {
            StartCoroutineWithTracking(ExecuteRageAttack());          
            nextAttackTime = Time.time + rageAttackInterval;
        }
        UpdateParticleSystemsState(); // 更新粒子系统状态
    }

    public bool isRageMode()
    {
        return !isShieldActive;
    }

    // 封装 StartCoroutine 以跟踪协程   
    private void StartCoroutineWithTracking(IEnumerator routine)
    {
        Coroutine coroutine = StartCoroutine(WrapCoroutine(routine));
        activeCoroutines.Add(coroutine);
    }

    // 包装协程以支持暂停
    private IEnumerator WrapCoroutine(IEnumerator routine)
    {
        while (routine.MoveNext())
        {
            yield return new WaitWhile(() => isTimeStopped); // 在时间停止时暂停
            yield return routine.Current;
        }
    }

    // 暂停所有协程
    private void PauseAllCoroutines()
    {
        // 暂停由 WrapCoroutine 中的 WaitWhile 实现
    }

    // 恢复所有协程
    private void ResumeAllCoroutines()
    {
        // 恢复由 WrapCoroutine 中的 WaitWhile 自动处理
    }

    // 狂暴模式攻击
    private IEnumerator ExecuteRageAttack()
    {
        List<Vector3> attackPositions = new List<Vector3>();
        
        for (int i = 0; i < rageAttackCount; i++)
        {
            Vector3 attackPos = GetRandomPointInPolygon();
            attackPositions.Add(attackPos);
        }

        List<GameObject> warningObjects = new List<GameObject>();
        foreach (Vector3 pos in attackPositions)
        {
            GameObject warningObject = Instantiate(warningCirclePrefab, pos, Quaternion.identity, AlarmsContainer);
            SpriteRenderer warningRenderer = warningObject.GetComponent<SpriteRenderer>();
            if (warningRenderer != null)
            {
                Coroutine pulseCoroutine = StartCoroutine(PulseWarning2(warningRenderer, warningDuration));
                warningCoroutines.Add(pulseCoroutine);
                activeCoroutines.Add(pulseCoroutine); // 跟踪子协程
            }
            warningObjects.Add(warningObject);
        }

        float warningTime = 0f;
        while (warningTime < warningDuration)
        {
            warningTime += Time.deltaTime;
            yield return null;
        }

        // foreach (Coroutine coroutine in warningCoroutines)
        // {
        //     if (coroutine != null) StopCoroutine(coroutine);
        // }
        // warningCoroutines.Clear();

        List<GameObject> energyBullets = new List<GameObject>();
        for (int i = 0; i < attackPositions.Count; i++)
        {
            Destroy(warningObjects[i]);
            if (energyBulletPrefab != null)
            {
                GameObject energyBullet = Instantiate(energyBulletPrefab, attackPositions[i], Quaternion.identity, EnergyBulletsContainer);
                ParticleSystem ps = energyBullet.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    energyBulletParticleSystems.Add(ps);
                }
                energyBullets.Add(energyBullet);
            }

            float distanceToTarget = Vector2.Distance(
                new Vector2(playerTransform.position.x, playerTransform.position.y),
                new Vector2(attackPositions[i].x, attackPositions[i].y)
            );

            if (distanceToTarget <= attackRadius)
            {
                Debug.Log("狂暴模式：玩家在攻击范围内！");
                PlayerController.Instance.PlayerHealth -= 50;
            }
        }

        float bulletAnimationTime = 0f;
        while (bulletAnimationTime < 2f)
        {
            bulletAnimationTime += Time.deltaTime;
            yield return null;
        }

        foreach (GameObject bullet in energyBullets)
        {
            if (bullet != null)
            {
                ParticleSystem ps = bullet.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    energyBulletParticleSystems.Remove(ps);
                }
                Destroy(bullet);
            }
        }

        activeCoroutines.RemoveAt(activeCoroutines.Count - 1); // 移除完成的协程
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

        Vector2 canonPosition = new Vector2(transform.position.x, transform.position.y);
        Vector2 playerPosition = new Vector2(playerTransform.position.x, playerTransform.position.y);
        float distance = Vector2.Distance(canonPosition, playerPosition);

        distance = Mathf.Pow(distance, distanceScaleFactor);
        
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

        if (destroyedGenerators >= shieldGenerators.Count && isShieldActive)
        {
            DisableShield();
            Factory2Controller.Instance.SetCanonRageMode();
            polygonCollider2D = polygonCollider2DRaged;
        }
    }

    private void DisableShield()
    {
        isShieldActive = false;
        Enemy3.canTakeDamage=true;
        Enemy3.assignedDamage=1.0f;
        SwitchRageAttackCamera.instance.SwitchToRageAttackCamera();
        animator.SetBool("isShield", false);
    }

    private IEnumerator ExecuteAttack()
    {
        animator.SetBool("isShooting", true);            
        if (!playerTransform || !IsPositionInAttackRange(playerTransform.position))
        {
            yield break;
        }

        Vector3 targetPosition = playerTransform.position;
        Vector3 attackPosition = GetClosestPointInPolygon(targetPosition);
        attackPosition.z = 0.1f;

        GameObject warningObject = Instantiate(warningCirclePrefab, attackPosition, Quaternion.identity, AlarmsContainer);
        SpriteRenderer warningRenderer = warningObject.GetComponent<SpriteRenderer>();
        Coroutine pulseCoroutine = null;
        if (warningRenderer != null)
        {
            pulseCoroutine = StartCoroutine(PulseWarning2(warningRenderer, warningDuration));
            warningCoroutines.Add(pulseCoroutine);
            activeCoroutines.Add(pulseCoroutine); // 跟踪子协程
        }

        float warningTime = 0f;
        while (warningTime < warningDuration)
        {
            warningTime += Time.deltaTime;
            yield return null;
        }

        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
            warningCoroutines.Remove(pulseCoroutine);
        }

        Destroy(warningObject);
        GameObject energyBullet = null;
        if (energyBulletPrefab != null)
        {
            energyBullet = Instantiate(energyBulletPrefab, attackPosition, Quaternion.identity, EnergyBulletsContainer);
            ParticleSystem ps = energyBullet.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                energyBulletParticleSystems.Add(ps);
            }
        }

        float distanceToTarget = Vector2.Distance(
            new Vector2(playerTransform.position.x, playerTransform.position.y),
            new Vector2(attackPosition.x, attackPosition.y)
        );

        if (distanceToTarget <= attackRadius)
        {
            Debug.Log("玩家在攻击范围内！");
            PlayerController.Instance.PlayerHealth -= 50;
        }

        float bulletAnimationTime = 0f;
        while (bulletAnimationTime < 2f)
        {
            bulletAnimationTime += Time.deltaTime;
            yield return null;
        }

        if (energyBullet != null)
        {
            ParticleSystem ps = energyBullet.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                energyBulletParticleSystems.Remove(ps);
            }
            Destroy(energyBullet);
        }
        animator.SetBool("isShooting", false);

        activeCoroutines.RemoveAt(activeCoroutines.Count - 1); // 移除完成的协程
    }

    // 获取 PolygonCollider2D 内的随机点
    private Vector3 GetRandomPointInPolygon()
    {
        if (polygonCollider2D == null) return transform.position;

        Bounds bounds = polygonCollider2D.bounds;
        int maxAttempts = 100;
        for (int i = 0; i < maxAttempts; i++)
        {
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            Vector2 point = new Vector2(randomX, randomY);

            if (polygonCollider2D.OverlapPoint(point))
            {
                return new Vector3(point.x, point.y, 0.1f);
            }
        }

        Debug.LogWarning($"{gameObject.name}: Failed to find point in PolygonCollider2D, returning center.", this);
        return polygonCollider2D.bounds.center;
    }

    // 获取 PolygonCollider2D 内最近的点
    private Vector3 GetClosestPointInPolygon(Vector3 target)
    {
        if (polygonCollider2D == null) return target;
        if (polygonCollider2D.OverlapPoint(target)) return target;

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
            float alpha = 0.3f + Mathf.PingPong((Time.time - startTime) * 2, 0.7f);
            renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        activeCoroutines.RemoveAt(activeCoroutines.Count - 1); // 移除完成的协程
    }

    private IEnumerator PulseWarning2(SpriteRenderer renderer, float duration)
    {
        float startTime = Time.time;
        Color baseColor = renderer.color;
        
        // 透明度从 0.5 到 1 之间平滑过渡
        while (Time.time - startTime < duration)
        {
            float alpha = Mathf.Lerp(0.1f, 1f, (Time.time - startTime) / duration);  // 透明度线性过渡
            renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }

        // 确保透明度设置为 1
        renderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);

        activeCoroutines.RemoveAt(activeCoroutines.Count - 1); // 移除完成的协程
    }


    // 在Scene视图中绘制攻击范围
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, canonAttackRange);
        
        Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
        Vector3 examplePosition = transform.position;
        examplePosition.x += canonAttackRange * 0.5f;
        Gizmos.DrawWireSphere(examplePosition, attackRadius);
    }

    // 更新所有粒子系统的播放/暂停状态
    private void UpdateParticleSystemsState()
    {   
        foreach (ParticleSystem ps in energyBulletParticleSystems)
        {
            if (ps != null)
            {
                if (isTimeStopped)
                {
                    // 时间停止时，将时间设置为 0.07 到 0.2 秒之间的随机值并暂停
                    float randomTime = Random.Range(0.07f, 0.2f);
                    ps.time = randomTime;
                    ps.Pause();
                }
                else if (isParticleSystemsPlaying && ps.isPaused)
                {
                    float randomTime = Random.Range(0.07f, 0.2f);
                    ps.time = randomTime;
                    ps.Play();
                }
                else if (!isParticleSystemsPlaying && ps.isPlaying)
                {
                    ps.Pause();
                }
            }
        }
    }

    // 停止所有警告闪烁
    private void StopWarningPulse()
    {
        foreach (Coroutine coroutine in warningCoroutines)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        warningCoroutines.Clear();

        foreach (Transform warning in AlarmsContainer)
        {
            if (warning != null)
            {
                SpriteRenderer renderer = warning.GetComponent<SpriteRenderer>();
                if (renderer != null)
                {
                    renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 1f);
                }
            }
        }
    }

    private void OnDestroy()
    {
        energyBulletParticleSystems.Clear();
        foreach (Coroutine coroutine in activeCoroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        activeCoroutines.Clear();
        foreach (Coroutine coroutine in warningCoroutines)
        {
            if (coroutine != null) StopCoroutine(coroutine);
        }
        warningCoroutines.Clear();
    }

}