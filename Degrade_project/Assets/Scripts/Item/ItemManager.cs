using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public InventoryItem[] inventoryItems;  // 管理每个物品的数量和获得状态
    public static ItemManager itemManager;
    private AudioSource audioSource;   // 音频源，用于播放音效

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
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); // 如果没有 AudioSource 组件，动态添加一个
        }
    }

    void Start()
    {
        // 初始化物品列表（可以通过添加物品在 Inspector 中配置）
        foreach (var inventoryItem in inventoryItems)
        {
            Debug.Log($"Item: {inventoryItem.item.itemName}, Amount: {inventoryItem.amount}, Obtained: {inventoryItem.isObtained}");
        }
    }

    void Update()
    {

    }

    public void UseItem(int index)
    {
        if (index >= 0 && index < inventoryItems.Length)
        {
            // 使用物品
            inventoryItems[index].Use(audioSource);
        }
    }
}
