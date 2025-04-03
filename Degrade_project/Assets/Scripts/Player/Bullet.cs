using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("子弹属性")]
    public float speed = 10f; // 子弹速度
    public float damage = 10f; // 子弹伤害
    public float lifeTime = 5f; // 子弹最大存活时间
    
    [Header("爆炸动画")]
    public GameObject explosionAnimPrefab; // 爆炸动画预制体
    
    [Header("Debug信息")]
    public bool debugMode = false; // 是否开启调试模式

    private float angle; // 子弹的旋转角度
    private bool isDestroying = false; // 标记是否正在销毁过程中
    private Rigidbody2D rb; // 刚体引用

     void Start()
    {
        // 获取刚体组件
        rb = GetComponent<Rigidbody2D>();
        
        // 确保子弹具有必要的碰撞器组件
        if (GetComponent<Collider2D>() == null)
        {
            if (debugMode) Debug.LogWarning("子弹缺少Collider2D组件，正在添加一个CircleCollider2D");
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.isTrigger = true; // 设置为触发器
        }
        
        // 设置子弹的生命周期
        StartCoroutine(DestroyAfterLifetime());
    }

    void Update()
    {
        // 如果没有使用刚体驱动移动，则手动控制移动
        if (rb == null || rb.isKinematic)
        {
            transform.Translate(Vector3.right * speed * Time.deltaTime, Space.Self);
        }
        else
        {
            // 子弹的角度由速度决定
            //rb = GetComponent<Rigidbody2D>();
            rb.velocity = transform.right * speed; // 设置初始速度
        }
    }

    // 当子弹离开摄像机视野时
    public void OnBecameInvisible()
    {
        if (!isDestroying)
        {
            if (debugMode) Debug.Log("子弹离开视野，准备销毁");
            DestroyBullet(false); // 离开视野时不播放爆炸效果
        }
    }

    // 碰撞检测 - 物理碰撞
    // private void OnCollisionEnter2D(Collision2D collision)
    // {
    //     if (debugMode) Debug.Log($"子弹发生物理碰撞：{collision.gameObject.name}，Tag: {collision.gameObject.tag}");
        
    //     // 忽略与玩家的碰撞
    //     if (!isDestroying && collision.gameObject.tag != "Player")
    //     {
    //         HandleHit(collision.gameObject);
    //     }
    // }

    // 碰撞检测 - 触发器
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (debugMode) Debug.Log($"子弹发生触发器碰撞：{collision.gameObject.name}，Tag: {collision.gameObject.tag}");
        
        // 忽略与玩家的碰撞
        if (!isDestroying && !collision.gameObject.CompareTag("Player")
        && collision.gameObject.CompareTag("Enemy"))
        {
            /// 防止多次触发
            // if (isDestroying) return;
            // isDestroying = true;
            
            if (debugMode) Debug.Log($"子弹击中物体：{collision.gameObject.name}");
            
            // 尝试对任何物体造成伤害
            EnemyBotAIpolygon_new enemy = collision.gameObject.GetComponent<EnemyBotAIpolygon_new>();
            EnemyBotAIpolygon_robot enemyRobot = collision.gameObject.GetComponent<EnemyBotAIpolygon_robot>();
            Enemy3 enemy3 = collision.gameObject.GetComponent<Enemy3>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"{collision.gameObject.name} 扣除了 {damage} 点生命值");
            }
            if (enemyRobot != null)
            {
                enemyRobot.TakeDamage(damage);
                Debug.Log($"{collision.gameObject.name} 扣除了 {damage} 点生命值");
            }
            if (enemy3 != null)
            {
                enemy3.TakeDamage(damage);
                Debug.Log($"{collision.gameObject.name} 扣除了 {damage} 点生命值");
            }

            
            // 播放爆炸动画并销毁子弹
            DestroyBullet(true);
            StopCoroutine(DestroyAfterLifetime());
            if (debugMode) Debug.Log($"播放动画");
        }
    }



    // 延迟销毁的协程
    private IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(lifeTime);
        if (!isDestroying)
        {
            if (debugMode) Debug.Log("子弹生命周期结束");
            DestroyBullet(true); 
        }
    }

    // 设置子弹的旋转和方向
    public void SetDirection(float newAngle)
    {
        angle = newAngle;
        
        // 更新子弹的朝向
        transform.rotation = Quaternion.Euler(0, 0, angle);
        

    }

    // 处理子弹销毁和爆炸动画
    private void DestroyBullet(bool playExplosion)
    
    {
        if (debugMode) Debug.Log($"进入 DestroyBullet，isDestroying = {isDestroying}");
        if (isDestroying) return; // 防止多次调用
        isDestroying = true;
        
        if (playExplosion)
        {
            PlayExplosionAnimation();
        }
        
        // 销毁子弹
        Destroy(gameObject);
    }

    // 播放爆炸动画
    private void PlayExplosionAnimation()
    {
        if (explosionAnimPrefab != null)
        {
            if (debugMode) Debug.Log("播放爆炸动画");
            
            Transform parent = GameObject.Find("Environment").transform;
            GameObject explosion;
            if (parent != null)
            {
                explosion = Instantiate(explosionAnimPrefab, transform.position, Quaternion.identity, parent);
            }
            else
            {
                Debug.LogError("未找到 Environment 对象，爆炸动画将生成在根级别");
                explosion = Instantiate(explosionAnimPrefab, transform.position, Quaternion.identity);
            }
            
            Animator animator = explosion.GetComponent<Animator>();
            if (animator != null)
            {
                AnimationClip[] clips = animator.runtimeAnimatorController.animationClips;
                float explosionDuration = 0f;
                foreach (AnimationClip clip in clips)
                {
                    explosionDuration = Mathf.Max(explosionDuration, clip.length);
                }
                explosionDuration = Mathf.Max(explosionDuration, 0.5f);
                Destroy(explosion, explosionDuration);
            }
            else
            {
                Destroy(explosion, 1f);
            }
        }
    }
}