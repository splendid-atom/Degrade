using UnityEngine;

// 基础 NPC 类
public abstract class NPC : ScriptableObject
{
    public string npcName;  // NPC 名字
    public string npcDescription;  // NPC 描述
    public Sprite npcPortrait;  // NPC 图片

    // NPC的行为
    public abstract void Interact();
}

// 商人 NPC 类，继承自 NPC
[CreateAssetMenu(fileName = "New Merchant", menuName = "NPCs/Merchant")]
public class MerchantNPC : NPC
{
    public Item[] itemsForSale;  // 商人出售的物品

    public override void Interact()
    {
        Debug.Log($"Interacting with merchant: {npcName}");
        // 显示商人出售的物品
        foreach (Item item in itemsForSale)
        {
            Debug.Log($"Selling: {item.itemName}");
        }
    }
}

// 任务发布 NPC 类，继承自 NPC
[CreateAssetMenu(fileName = "New Quest Giver", menuName = "NPCs/Quest Giver")]
public class QuestGiverNPC : NPC
{
    public string questDescription;  // 任务描述
    public bool isQuestAccepted;  // 是否接受任务

    public override void Interact()
    {
        if (isQuestAccepted)
        {
            Debug.Log($"Quest Accepted: {questDescription}");
        }
        else
        {
            Debug.Log($"Quest Giver: {npcName}, Task: {questDescription}");
        }
    }
}
