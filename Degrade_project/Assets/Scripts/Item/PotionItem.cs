using UnityEngine;

// 药水类，继承自Item
[CreateAssetMenu(fileName = "New Potion", menuName = "Items/Potion")]
public class PotionItem : Item
{
    public int healAmount;  // 药水恢复的生命值
    public int shieldAmount;//恢复护甲值

    // 重写 Use() 方法，实现具体的使用逻辑
    public override void Use()
    {
        // 在控制台打印出使用药水的信息
        Debug.Log($"Using potion: {itemName}, Heal Amount: {healAmount}");
        if (healAmount > 0)
        {
            // 限制玩家生命值在0到100之间
            PlayerController.Instance.PlayerHealth = Mathf.Clamp(PlayerController.Instance.PlayerHealth + healAmount, 0, 100);
            Debug.Log($"Player Health: {PlayerController.Instance.PlayerHealth}");
        }
        if (shieldAmount > 0)
        {
            // 限制玩家护甲值在0到200之间
            PlayerController.Instance.PlayerShield = Mathf.Clamp(PlayerController.Instance.PlayerShield + shieldAmount, 0, 200);
            Debug.Log($"Player Shield: {PlayerController.Instance.PlayerShield}");
        }
 
    }
}
