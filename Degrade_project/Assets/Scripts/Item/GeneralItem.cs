using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 通用item
[CreateAssetMenu(fileName = "New GeneralItem", menuName = "Items/GeneralItem")]
public class GeneralItem : Item
{
    // 重写 Use() 方法，实现具体的使用逻辑
    public override void Use(AudioSource audioSource)
    {
        Debug.Log($"Using item: {itemID}");
        if(itemID == 5){//时光手表
            VillageSceneController.instance.isTimeMachine = true;
        }
    }
}
