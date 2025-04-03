using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] float maxHealth = 100f;
    public float currentHealth;

    [SerializeField] bool canTakeDamage = true;

    // 可选：受击特效与音效
    [SerializeField] ParticleSystem hitEffect;
    [SerializeField] AudioClip hitSound;
    private AudioSource audioSource;

    private void Awake()
    {

    }
    private void Start()
    {

        audioSource = GetComponent<AudioSource>();
    }

    public void TakeDamage(float damage)
    {
        if (!canTakeDamage)
        {
            // 若此敌人设置为不可受伤，则直接返回
            return;
        }

        //currentHealth -= damage;
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

    /// <summary>
    /// 敌人死亡后的处理：播放死亡动画、清理对象等。
    /// </summary>
    private void Die()
    {
        Debug.Log(gameObject.name + " 已死亡。");
        // 此处可以添加延迟销毁、掉落物品等逻辑
        Destroy(gameObject);
    }
}
