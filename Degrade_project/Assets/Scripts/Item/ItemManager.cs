using UnityEngine;
using System.Collections.Generic;


public class ItemManager : MonoBehaviour
{
    public List<InventoryItem> inventoryItems; // 管理每个物品的数量和获得状态
    public static ItemManager itemManager;
    private AudioSource audioSource;   // 音频源，用于播放音效
    public delegate void ItemAddedHandler(int index);
    public event ItemAddedHandler OnItemAdded;
    public void AddItem(int itemId, int amount)
    {
        for (int i = 0; i < inventoryItems.Count; i++)
        {
            if (inventoryItems[i].item.itemID == itemId && !inventoryItems[i].isObtained)
            {
                inventoryItems[i].isObtained = true;
                inventoryItems[i].amount = amount;
                // Debug.Log($"Item added at index {i}");
                OnItemAdded?.Invoke(i);
                return;
            }
        }
        // Debug.LogWarning("Item with ID " + itemId + " not found or already obtained.");
    }
    public int GetItemAmount(string itemName){
        foreach (var inventoryItem in inventoryItems)
        {
            if (inventoryItem.item.itemName == itemName)
            {
                return inventoryItem.amount;
            }
        }
        return -1;//没有这个物品
    }
    void Awake()
    {
        if (itemManager == null)
        {
            itemManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
        // 获取 AudioSource 组件
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        // 初始化物品列表（可以通过添加物品在 Inspector 中配置）
        foreach (var inventoryItem in inventoryItems)
        {
            // Debug.Log($"Item: {inventoryItem.item.itemName}, Amount: {inventoryItem.amount}, Obtained: {inventoryItem.isObtained}");
        }
    }
    public void AddAmount(string itemName, int addedAmount){
        foreach (var inventoryItem in inventoryItems)
        {
            if (inventoryItem.item.itemName == itemName)
            {
                inventoryItem.AddAmount(addedAmount);
                break;
            }
        }
    }
    void Update()
    {

    }

    public void UseItem(int index)
    {
        if (index >= 0 && index < inventoryItems.Count)
        {
            // 使用物品
            inventoryItems[index].Use(audioSource);
        }
    }
}
