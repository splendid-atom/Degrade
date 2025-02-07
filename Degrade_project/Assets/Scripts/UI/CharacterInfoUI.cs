using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class CharacterInfoUI : MonoBehaviour
{
    public Slider healthSlider;  // 生命值血条
    public TextMeshProUGUI healthText;  // 生命值文本
    public Slider shieldSlider;  // 护甲值血条
    public TextMeshProUGUI shieldText;  // 护甲值文本
    public TextMeshProUGUI PlayerNameText;  // 护甲值文本
    // public float maxHealth = 100f;  // 最大生命值
    // public float maxShield = 100f;  // 最大护甲值
    private float maxHealth;  // 最大生命值
    private float maxShield;  // 最大护甲值
    private float currentHealth;  // 当前生命值
    private float previousHealth; // 上一刻生命值
    private float currentShield;  // 当前护甲值
    private float previousShield; // 上一刻护甲值

    private void Start()
    {
        PlayerNameText.text = $"调查员：{PlayerController.Instance.PlayerName}";
        maxHealth = PlayerController.Instance.MaxHealth;
        maxShield = PlayerController.Instance.MaxShield;
        previousHealth = PlayerController.Instance.PlayerHealth;  // 初始化生命值
        currentHealth = previousHealth;
        previousShield = PlayerController.Instance.PlayerShield;  // 初始化护甲值
        currentShield = previousShield;
        UpdateHealthUI();  // 更新生命值UI
        UpdateShieldUI();  // 更新护甲值UI
    }

    private void Update()
    {
        // 更新生命值
        currentHealth = PlayerController.Instance.PlayerHealth;
        if (currentHealth != previousHealth)
        {
            StartCoroutine(SmoothHealthChange(currentHealth));  // 平滑更新生命值
            previousHealth = currentHealth;
        }

        // 更新护甲值
        currentShield = PlayerController.Instance.PlayerShield;
        if (currentShield != previousShield)
        {
            StartCoroutine(SmoothShieldChange(currentShield));  // 平滑更新护甲值
            previousShield = currentShield;
        }
    }

    // 调整生命值
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth < 0)
            currentHealth = 0;  // 防止生命值小于0
        StartCoroutine(SmoothHealthChange(currentHealth));  // 将当前生命值传递给协程
    }

    // 恢复生命值
    public void Heal(float amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
            currentHealth = maxHealth;  // 防止生命值超过最大值
        StartCoroutine(SmoothHealthChange(currentHealth));  // 将当前生命值传递给协程
    }

    // 调整护甲值
    public void TakeShieldDamage(float damage)
    {
        currentShield -= damage;
        if (currentShield < 0)
            currentShield = 0;  // 防止护甲值小于0
        StartCoroutine(SmoothShieldChange(currentShield));  // 将当前护甲值传递给协程
    }

    // 恢复护甲值
    public void HealShield(float amount)
    {
        currentShield += amount;
        if (currentShield > maxShield)
            currentShield = maxShield;  // 防止护甲值超过最大值
        StartCoroutine(SmoothShieldChange(currentShield));  // 将当前护甲值传递给协程
    }

    // 更新生命值UI显示
    private void UpdateHealthUI()
    {
        healthSlider.value = currentHealth / maxHealth;  // 更新血条
        healthText.text = $"{currentHealth:F1}/{maxHealth}";  // 更新文本
    }

    // 更新护甲值UI显示
    private void UpdateShieldUI()
    {
        shieldSlider.value = currentShield / maxShield;  // 更新护甲血条
        shieldText.text = $"{currentShield:F1}/{maxShield}";  // 更新护甲文本
    }

    // 平滑生命值变化的协程
    IEnumerator SmoothHealthChange(float targetHealth)
    {
        float startHealth = healthSlider.value * maxHealth;  // 以血条的当前值作为初始值
        float time = 0f;
        float duration = 0.05f; // 设置动画持续时间

        while (time < duration)
        {
            currentHealth = Mathf.Lerp(startHealth, targetHealth, time / duration);  // 计算平滑的生命值
            UpdateHealthUI();  // 更新UI
            time += Time.deltaTime;
            yield return null;
        }

        currentHealth = targetHealth;  // 确保最终的生命值准确
        UpdateHealthUI();  // 更新UI
    }

    // 平滑护甲值变化的协程
    IEnumerator SmoothShieldChange(float targetShield)
    {
        float startShield = shieldSlider.value * maxShield;  // 以护甲值血条的当前值作为初始值
        float time = 0f;
        float duration = 0.05f; // 设置动画持续时间

        while (time < duration)
        {
            currentShield = Mathf.Lerp(startShield, targetShield, time / duration);  // 计算平滑的护甲值
            UpdateShieldUI();  // 更新护甲UI
            time += Time.deltaTime;
            yield return null;
        }

        currentShield = targetShield;  // 确保最终的护甲值准确
        UpdateShieldUI();  // 更新UI
    }
}
