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
        // // 使用数字键 1 到 0 来选择物品并使用它们
        // for (int i = 0; i < inventoryItems.Length; i++)  // 遍历物品列表
        // {
        //     if (Input.GetKeyDown(KeyCode.Alpha1 + i))  // 监听 1 到 0 键
        //     {
        //         UseItem(i);  // 使用对应索引的物品
        //     }
        // }
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
