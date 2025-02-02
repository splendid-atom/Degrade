using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public Item[] items;  // 你可以在 Inspector 中为数组拖放物品资源，最多 10 个

    void Update()
    {
        // 使用数字键 1 到 0 来选择物品并使用它们
        for (int i = 0; i < 10; i++)  // 键 1 到 0
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))  // 监听 1 到 0 键
            {
                UseItem(i);  // 使用对应索引的物品
            }
        }
    }

    void UseItem(int index)
    {
        if (index >= 0 && index < items.Length)
        {
            items[index].Use();  // 调用物品的Use方法
        }
    }
}
