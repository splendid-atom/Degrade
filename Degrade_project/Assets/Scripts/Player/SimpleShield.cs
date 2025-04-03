using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shield : MonoBehaviour
{
    [Header("护盾设置")]
    [SerializeField] float maxShieldValue = 10f;        // 最大护盾值
    [SerializeField] float shieldRegenRate = 0.5f;      // 每秒恢复的护盾值
    [SerializeField] float damagePerHit = 5f;           // 每次受到的伤害值
    [SerializeField] bool autoRegen = true;             // 是否自动恢复护盾

    [SerializeField] bool isInvincible = false;         // 是否无敌
    
    [Header("视觉效果")]
    [SerializeField] bool showDebugMessages = true;     // 是否显示调试信息
    
    private float currentShield;                         // 当前护盾值
    private bool isShieldBroken = false;                // 护盾是否已破碎
    
    void Start()
    {
        ResetShield();
        if(showDebugMessages) Debug.Log($"护盾启动，初始值：{currentShield}");
    }

    void Update()
    {
        // 护盾未破碎且开启了自动恢复时才进行恢复
        if (!isShieldBroken && autoRegen)
        {
            // 每秒自动恢复护盾值（确保不超过最大值）
            currentShield = Mathf.Min(currentShield + shieldRegenRate * Time.deltaTime, maxShieldValue);
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // 如果护盾已经破碎，不再处理碰撞
        if (isShieldBroken) return;
        
        // 检测与攻击物体的碰撞
        if(col.gameObject.CompareTag("PlayerAttack") || col.gameObject.CompareTag("EnemyAttack"))
        {
            if(showDebugMessages) Debug.Log("护盾被攻击！");
            
            // 应用伤害
            ApplyDamage(damagePerHit);
            
            // // 如果碰撞物是子弹，可以尝试销毁它
            // Bullet bullet = col.GetComponent<Bullet>();
            // if (bullet != null)
            {
                // 子弹应该会自行处理自己的销毁
            }
        }
    }

    /// <summary>
    /// 对护盾造成伤害
    /// </summary>
    /// <param name="damage">伤害值</param>
    public void ApplyDamage(float damage)
    {
        if(isInvincible) return; // 如果护盾无敌，不受伤害
        // 减少护盾值
        currentShield -= damage;
        if(showDebugMessages) Debug.Log($"护盾受击，当前护盾值：{currentShield}");

        // 检查护盾是否破碎
        if (currentShield <= 0 && !isShieldBroken)
        {
            BreakShield();
        }
    }

    /// <summary>
    /// 护盾破碎时调用
    /// </summary>
    public void BreakShield()
    {
        isShieldBroken = true;
        if(showDebugMessages) Debug.Log("护盾已破碎！");
        
        // 禁用此游戏对象
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 重置护盾状态
    /// </summary>
    public void ResetShield()
    {
        currentShield = maxShieldValue;
        isShieldBroken = false;
        
        // 确保护盾对象是激活的
        gameObject.SetActive(true);
        
        if(showDebugMessages) Debug.Log("护盾已重置");
    }
    
    /// <summary>
    /// 获取当前护盾值
    /// </summary>
    public float GetCurrentShield()
    {
        return currentShield;
    }
    
    /// <summary>
    /// 获取护盾最大值
    /// </summary>
    public float GetMaxShield()
    {
        return maxShieldValue;
    }
    
    /// <summary>
    /// 设置护盾自动恢复功能
    /// </summary>
    public void SetAutoRegen(bool enabled)
    {
        autoRegen = enabled;
    }
}