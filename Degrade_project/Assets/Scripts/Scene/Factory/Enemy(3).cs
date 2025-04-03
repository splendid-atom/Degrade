using UnityEngine;

public class Enemy3 : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;

    /// <summary>
    /// 用于控制该敌人是否可受到伤害，若为 false 则免疫伤害
    /// </summary>
    [SerializeField] bool canTakeDamage = true;

    // 可选：受击特效与音效
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] AudioClip hitSound;
    private AudioSource audioSource;

    // 血条的尺寸和颜色设置
    [SerializeField] private float barWidth = 1f; // 显示在世界空间中的宽度
    [SerializeField] private float barHeight = 0.1f; // 显示在世界空间中的高度
    [SerializeField] private Vector2 barOffset = new Vector2(0f, 0.7f); // 血条与敌人位置的偏移
    [SerializeField] private Color healthColor = Color.green;
    [SerializeField] private Color backColor = Color.red;
    
    // 用于测试伤害的变量
    [SerializeField] private bool autoHeal = false;
    [SerializeField] private float healAmount = 2f;
    
    // 碰撞检测调试
    [SerializeField] private bool debugCollision = true;
    private float lastCollisionCheckTime = 0f;
    private bool colliderConfigured = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 检查碰撞体设置
        CheckColliderSetup();
    }
    
    private void CheckColliderSetup()
    {
        // 检查是否有碰撞体
        Collider2D collider = GetComponent<Collider2D>();
        if(collider == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 没有2D碰撞体组件！添加攻击检测将无法工作。");
            return;
        }
        
        // 检查碰撞体是否为触发器
        if(!collider.isTrigger)
        {
            Debug.LogWarning($"[{gameObject.name}] 2D碰撞体没有设置为触发器(isTrigger)！可能导致无法检测攻击。");
        }
        
        // 检查是否有Rigidbody2D
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if(rb == null)
        {
            Debug.LogWarning($"[{gameObject.name}] 没有Rigidbody2D组件！添加此组件可能提高碰撞检测可靠性。");
        }
        else if(rb.bodyType != RigidbodyType2D.Kinematic && rb.bodyType != RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning($"[{gameObject.name}] Rigidbody2D不是Kinematic或Dynamic类型，可能影响碰撞检测。");
        }
        
        colliderConfigured = true;
    }

    void Start()
    {
        // 打印一些调试信息以确保组件加载正确
        Debug.Log($"敌人 {gameObject.name} 初始化，生命值: {currentHealth}/{maxHealth}");
        
        // 可以在这里添加一次小伤害来测试血条是否正常工作
        // TakeDamage(1f);
    }

    void Update()
    {
        if(autoHeal)
        {
            Heal(healAmount * Time.deltaTime); // 给敌人每秒恢复指定的生命值
        }
        
        // 定期检查碰撞设置是否正确
        if(debugCollision && Time.time > lastCollisionCheckTime + 5f)
        {
            lastCollisionCheckTime = Time.time;
            if(!colliderConfigured)
            {
                CheckColliderSetup();
            }
        }
    }

    /// <summary>
    /// 敌人受到伤害时调用，若 canTakeDamage = false，则无视伤害。
    /// </summary>
    /// <param name="damage">伤害数值</param>
    public void TakeDamage(float damage)
    {
        if (!canTakeDamage)
        {
            // 若此敌人设置为不可受伤，则直接返回
            Debug.Log($"[{gameObject.name}] 免疫伤害");
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
        Debug.Log(gameObject.name + " 受到伤害：" + damage + "，剩余生命：" + currentHealth);

        // 播放受击特效
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        // 播放受击音效
        if (audioSource != null && hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 可扩展：添加击退、闪烁等效果

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // 恢复生命值的方法
    public void Heal(float amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    }
    
    // 绘制血条 - 修改了屏幕上的显示尺寸
    private void OnGUI()
    {
        if(Camera.main == null) return;
        
        // 将世界坐标转换为屏幕坐标
        Vector2 worldPosition = transform.position + new Vector3(barOffset.x, barOffset.y, 0);
        Vector2 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        
        // 调整坐标系（GUI坐标系与屏幕坐标系不同）
        screenPosition.y = Screen.height - screenPosition.y;
        
        // 计算血条在屏幕上的尺寸 (将世界尺寸转换为像素尺寸)
        float pixelWidth = barWidth * 100; // 放大倍数，根据需要调整
        float pixelHeight = barHeight * 100; // 放大倍数，根据需要调整
        
        // 计算血条矩形
        float healthPercent = currentHealth / maxHealth;
        float healthBarWidth = pixelWidth * healthPercent;
        
        Rect backgroundRect = new Rect(screenPosition.x - pixelWidth / 2, screenPosition.y, pixelWidth, pixelHeight);
        Rect healthRect = new Rect(screenPosition.x - pixelWidth / 2, screenPosition.y, healthBarWidth, pixelHeight);
        
        // 防止GUI颜色被持续改变
        Color oldColor = GUI.color;
        
        // 绘制背景（红色部分）
        GUI.color = backColor;
        GUI.DrawTexture(backgroundRect, Texture2D.whiteTexture);
        
        // 绘制健康部分（绿色部分）
        GUI.color = healthColor;
        GUI.DrawTexture(healthRect, Texture2D.whiteTexture);
        
        // 恢复颜色
        GUI.color = oldColor;
    }
    
    // 在Scene视图中用Gizmos绘制血条
    // private void OnDrawGizmos()
    // {
    //     // 计算血条位置
    //     Vector3 barPos = transform.position + new Vector3(barOffset.x, barOffset.y, 0);
        
    //     // 获取当前健康度百分比(编辑模式下假设为100%)
    //     float healthPercent = Application.isPlaying ? (currentHealth / maxHealth) : 1.0f;
        
    //     // 绘制血条背景(红色部分)
    //     Gizmos.color = backColor;
    //     Gizmos.DrawCube(barPos, new Vector3(barWidth, barHeight, 0.01f));
        
    //     // 绘制当前生命值(绿色部分)
    //     Gizmos.color = healthColor;
    //     // 调整绿色部分宽度和位置，使其左对齐
    //     float healthWidth = barWidth * healthPercent;
    //     Vector3 healthBarPos = barPos - new Vector3((barWidth - healthWidth) / 2, 0, 0);
    //     Gizmos.DrawCube(healthBarPos, new Vector3(healthWidth, barHeight, 0.01f));
        
    //     // 绘制血条边框
    //     Gizmos.color = Color.white;
    //     Gizmos.DrawWireCube(barPos, new Vector3(barWidth, barHeight, 0.01f));
        
    //     // 绘制碰撞体范围(如果存在)
    //     Collider2D collider = GetComponent<Collider2D>();
    //     if(collider != null)
    //     {
    //         Gizmos.color = new Color(0, 1, 1, 0.2f); // 青色半透明
            
    //         if(collider is BoxCollider2D)
    //         {
    //             BoxCollider2D boxCollider = collider as BoxCollider2D;
    //             Vector3 colliderPos = transform.position + (Vector3)boxCollider.offset;
    //             Gizmos.DrawCube(colliderPos, new Vector3(boxCollider.size.x, boxCollider.size.y, 0.01f));
    //         }
    //         else if(collider is CircleCollider2D)
    //         {
    //             CircleCollider2D circleCollider = collider as CircleCollider2D;
    //             Vector3 colliderPos = transform.position + (Vector3)circleCollider.offset;
    //             // 在2D视图中画一个圆形
    //             Gizmos.DrawSphere(colliderPos, circleCollider.radius);
    //         }
    //     }
    // }

    /// <summary>
    /// 敌人死亡后的处理：播放死亡动画、清理对象等。
    /// </summary>
    private void Die()
    {
        Debug.Log(gameObject.name + " 已死亡。");
        // 此处可以添加延迟销毁、掉落物品等逻辑
        Destroy(gameObject);
    }
    
    /// <summary>
    /// 处理与玩家攻击碰撞的方法
    /// </summary>
    // private void OnTriggerEnter2D(Collider2D collision)
    // {
    //     // 记录所有碰撞以便调试
    //     if(debugCollision)
    //     {
    //         Debug.Log($"[{gameObject.name}] 触发碰撞：{collision.gameObject.name}，标签：{collision.gameObject.tag}");
    //     }
        
    //     // 检查碰撞体是否为玩家的攻击
    //     if (collision.CompareTag("PlayerAttack"))
    //     {
    //         Debug.Log($"[{gameObject.name}] 检测到玩家攻击：{collision.gameObject.name}");
            
    //         // 尝试获取攻击的伤害值
    //         // PlayerAttack attackComponent = collision.GetComponent<PlayerAttack>();
    //         // if (attackComponent != null)
    //         // {
    //         //     // 如果攻击组件存在且有伤害值，则使用该伤害值
    //         //     TakeDamage(attackComponent.damageAmount);
    //         // }

    //             TakeDamage(10f); // 默认伤害值
    //             Destroy(collision.gameObject); // 销毁攻击物体
    //             if(debugCollision)
    //             {
    //                 Debug.LogWarning($"[{gameObject.name}] 攻击物体 {collision.gameObject.name} 没有PlayerAttack组件，使用默认伤害值10");
    //             }
            
    //     }
    // }
    
    // 添加这个方法可以捕获任何碰撞，帮助调试
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(debugCollision)
        {
            Debug.Log($"[{gameObject.name}] 物理碰撞(非触发器)：{collision.gameObject.name}，标签：{collision.gameObject.tag}");
            Debug.LogWarning("检测到物理碰撞而非触发器碰撞。如果这是玩家攻击，请确保攻击碰撞体设置为触发器(isTrigger)。");
        }
    }
}

