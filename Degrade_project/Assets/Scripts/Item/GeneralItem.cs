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
            audioSource.Play();
            if(!QuestUIManager.QuestManager.isTaskCompleted("真要进去吗?",2)){
                BambooMazeHint.instance.isHintOn = true;
            }
            else{
                if(!VillageSceneController.instance.wasTimeMachine){
                VillageSceneController.instance.isTimeMachine = true; 
                }
                if(!VillageSceneController.instance.wasSecondTimeMachine&&
                VillageSceneController.instance.wasTimeMachine){
                    VillageSceneController.instance.isSecondTimeMachine = true;
                }                
            }

            
        }
        if(itemID == 4){//时空碎片
            Debug.Log("TimePiece");
            FetchItemController.instance.UsePortalHint.gameObject.SetActive(false);
            InterSceneMemory.instance.isSwitchToFactory1 = true;
        }
        // 播放使用音效
        if (useSound != null)
        {
            audioSource.PlayOneShot(useSound);
        }
    }
}
