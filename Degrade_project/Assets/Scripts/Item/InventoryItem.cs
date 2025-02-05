using UnityEngine;

[System.Serializable]
public class InventoryItem
{
    public Item item;        // 物品本身（继承自 Item）
    public int amount;       // 物品数量
    public bool isObtained;  // 是否已获得

    // 构造函数
    public InventoryItem(Item item, int amount, bool isObtained)
    {
        this.item = item;
        this.amount = amount;
        this.isObtained = isObtained;
    }

    // 使用物品
    public void Use(AudioSource audioSource)
    {
        if (amount > 0 && isObtained)
        {
            amount--;
            item.Use(audioSource);
            Debug.Log($"Used item: {item.itemName}. Remaining: {amount}");
        }
        else
        {
            Debug.Log($"Cannot use {item.itemName}. Either out of stock or not obtained.");
        }
    }
}
