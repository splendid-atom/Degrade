using UnityEngine;

// 基础物品类
public abstract class Item : ScriptableObject
{
    public string itemName;  // 物品名称
    public int itemID;       // 物品ID
    public Sprite itemIcon;  // 物品图标
    public AudioClip useSound;    // 物品使用时的音效
    
    public string description;  // 物品描述
    public float cooldownTime = 0f; // 冷却时间
    public abstract void Use(AudioSource audioSource);  // 使用物品的抽象方法

}