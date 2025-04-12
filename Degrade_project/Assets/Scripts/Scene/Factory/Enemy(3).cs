using UnityEngine;
using Minimalist.Quantity;
public class Enemy3 : MonoBehaviour
{
    public float maxHealth = 100f;
    public float currentHealth;
    public float assignedDamage=0;

    /// <summary>
    /// 用于控制该敌人是否可受到伤害，若为 false 则免疫伤害
    /// </summary>
    public bool canTakeDamage = true;

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
    private float lastCollisionCheckTime = 0f;
    public QuantityBhv quantityBhv; 
    public int direction = 1;
    public Transform HealthDisplay;

    private void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        if(audioSource == null && hitSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    


    void Update()
    {
        HealthDisplaySettingEnemy(); 
    }

    public void TakeDamage(float damage)
    {
        if (!canTakeDamage)
        {
            // 若此敌人设置为不可受伤，则直接返回
            Debug.Log($"[{gameObject.name}] 免疫伤害");
            return;
        }
        if(assignedDamage!=0){
            damage=assignedDamage;
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

    // // 恢复生命值的方法
    // public void Heal(float amount)
    // {
    //     currentHealth += amount;
    //     currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);
    // }
    
    private void Die()
    {
        Debug.Log(gameObject.name + " 已死亡。");
        // 此处可以添加延迟销毁、掉落物品等逻辑
        Destroy(gameObject);
    }
    
    //关于血量显示的设置
    private void HealthDisplaySettingEnemy(){
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

