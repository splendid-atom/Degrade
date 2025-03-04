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
    public void AddAmount(int addedAmount)
    {
        if(amount<=0&&addedAmount>0){
            isObtained = true;
        }
        amount += addedAmount;
        OnAmountChanged?.Invoke();  // 当物品数量变化时，调用事件通知 UI 更新
    }
    // 使用物品
    public void Use(AudioSource audioSource)
    {
        if (amount > 0 && isObtained)
        {
            // 检查“时空碎片”数量是否小于3
            if (item.itemName == "时空碎片"){
                if(amount < 3){
                    Debug.Log("时空碎片数量不足，至少需要3个。");
                    return; // 数量不足，阻止使用
                }
                else{
                    amount -= 3;
                    item.Use(audioSource);
                    OnAmountChanged?.Invoke();  // 通知 UI 更新
                    Debug.Log($"Used item: {item.itemName}. Remaining: {amount}");
                    return;
                }
            }
            amount--;
            item.Use(audioSource);
            OnAmountChanged?.Invoke();  // 通知 UI 更新
            Debug.Log($"Used item: {item.itemName}. Remaining: {amount}");
        }
        else
        {
            Debug.Log($"Cannot use {item.itemName}. Either out of stock or not obtained.");
        }
    }
}
