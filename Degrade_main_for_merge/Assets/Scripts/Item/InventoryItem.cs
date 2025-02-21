using UnityEngine;
using System;

[System.Serializable]
public class InventoryItem
{
    public Item item;        // 物品本身（继承自 Item）
    public int amount;       // 物品数量
    public bool isObtained;  // 是否已获得

    // 声明一个委托，用于通知 UI 更新
    public Action OnAmountChanged;

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
            OnAmountChanged?.Invoke();  // 当物品数量变化时，调用事件通知 UI 更新
            Debug.Log($"Used item: {item.itemName}. Remaining: {amount}");
        }
        else
        {
            Debug.Log($"Cannot use {item.itemName}. Either out of stock or not obtained.");
        }
    }
}
