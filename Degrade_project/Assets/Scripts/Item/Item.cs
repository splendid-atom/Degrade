using UnityEngine;

// 基础物品类
public abstract class Item : ScriptableObject
{
    public string itemName;  // 物品名称
    public int itemID;       // 物品ID
    public Sprite itemIcon;  // 物品图标
    public string description;  // 物品描述

    public abstract void Use();  // 使用物品的抽象方法
    // 查找场景中的 Player 对象
    protected PlayerController GetPlayer()
    {
        return PlayerController.Instance;  // 获取单例的 PlayerController
    }
}

// // 武器类，继承自Item
// [CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
// public class WeaponItem : Item
// {
//     public int attackPower;  // 武器攻击力

//     public override void Use()
//     {
//         Debug.Log($"Using weapon: {itemName}, Power: {attackPower}");
//     }
// }

// // 药水类，继承自Item
// [CreateAssetMenu(fileName = "New Potion", menuName = "Items/Potion")]
// public class PotionItem : Item
// {
//     public int healAmount;  // 药水恢复的生命值

//     public override void Use()
//     {
//         Debug.Log($"Using potion: {itemName}, Heal Amount: {healAmount}");
//     }
// }
