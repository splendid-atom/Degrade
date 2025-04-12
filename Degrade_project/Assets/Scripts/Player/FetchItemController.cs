using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class FetchItemController : MonoBehaviour
{
    public static FetchItemController instance; // Singleton instance
    public GameObject TimePieceContainer;
    public List<GameObject> TimePiece = new List<GameObject>();
    public TextMeshProUGUI FetchHint; // 全局提示文本
    private int playerInTriggerCount = 0; // 计数器：玩家在多少个 TimePiece 触发区域内
    
    public List<Dialogue> CollectedTimePieceDialogues;  // 管理玩家收集完三个时空碎片后且完成所有任务时的对话
    public bool isTalking = false;
    public string npcName;
    public TextMeshProUGUI UsePortalHint; // 提示使用传送门


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicates
        }
    }
    void Start()
    {

        // 获取 TimePieceContainer 对象
        TimePieceContainer = GameObject.Find("TimePieceContainer");

        // 获取全局的 FetchHint 对象
        GameObject hintObject = GameObject.Find("FetchHintContent");
        GameObject usePortalObject = GameObject.Find("UsePortalHint");
        if (hintObject != null)
        {
            FetchHint = hintObject.GetComponent<TextMeshProUGUI>();
            FetchHint.gameObject.SetActive(false); // 初始隐藏
        }
        else
        {
            Debug.LogError("FetchHintContent not found!");
        }
        if (usePortalObject != null)
        {
            UsePortalHint = usePortalObject.GetComponent<TextMeshProUGUI>();
            UsePortalHint.gameObject.SetActive(false); // 初始隐藏
        }
        else
        {
            // Debug.LogError("UsePortalHint not found!");
        }
        // 检查场景名称，如果不是"sampleScene"，则返回
        if (InterSceneMemory.instance.isInSampleScene())
        {
            if (TimePieceContainer != null)
            {
                // 遍历所有子对象并添加到 TimePiece 列表
                foreach (Transform child in TimePieceContainer.transform)
                {
                    TimePiece.Add(child.gameObject);

                    // 确保子对象有 BoxCollider2D 组件，并设置为 Trigger
                    BoxCollider2D collider = child.GetComponent<BoxCollider2D>();
                    if (collider == null)
                    {
                        collider = child.gameObject.AddComponent<BoxCollider2D>();
                    }
                    collider.isTrigger = true;

                    // 确保子对象有 TimePieceTrigger 脚本，并绑定 FetchItemController
                    TimePieceTrigger trigger = child.GetComponent<TimePieceTrigger>();
                    if (trigger == null)
                    {
                        trigger = child.gameObject.AddComponent<TimePieceTrigger>();
                    }
                    trigger.controller = this; // 传递控制器引用
                }
            }
            else
            {
                Debug.LogError("TimePieceContainer not found!");
            }
        }

    }
    void Update()
    {
        if (InterSceneMemory.instance.isInSampleScene())
        {
            foreach (Transform child in TimePieceContainer.transform)
            {
                if (!TimePiece.Contains(child.gameObject))
                {
                    TimePiece.Add(child.gameObject);
                    TimePieceTrigger trigger = child.GetComponent<TimePieceTrigger>();
                    if (trigger != null) trigger.controller = this;
                }
            }    
        }



    }
    public void UpdateChilds(){
        foreach (Transform child in TimePieceContainer.transform)
        {
            if (!TimePiece.Contains(child.gameObject))
            {
                TimePiece.Add(child.gameObject);
                TimePieceTrigger trigger = child.GetComponent<TimePieceTrigger>();
                if (trigger != null) trigger.controller = this;
            }
        }        
    }
    // 玩家进入 TimePiece 触发区域
    public void PlayerEnteredTrigger()
    {
        playerInTriggerCount++;
        if (playerInTriggerCount > 0)
        {

            FetchHint?.gameObject.SetActive(true); // 显示提示
        }
    }

    // 玩家离开 TimePiece 触发区域
    public void PlayerExitedTrigger()
    {
        playerInTriggerCount--;
        if (playerInTriggerCount <= 0)
        {
            FetchHint?.gameObject.SetActive(false); // 隐藏提示
            playerInTriggerCount = 0; // 防止计数器变成负数
        }
    }

    // 移除 TimePiece 并更新计数
    public void RemoveTimePieceAndCount(GameObject timePiece)
    {
        if (TimePiece.Contains(timePiece))
        {
            TimePiece.Remove(timePiece);
            // Debug.Log($"TimePiece {timePiece.name} has been picked up.");
            // 此处可以调用你的物品管理逻辑，例如：
            ItemManager.itemManager.AddAmount("时空碎片", 1);
            QuestUIManager.QuestManager.quests
            [QuestUIManager.QuestManager.quests.Count-1].collectedAmount++;
            //如果获取了三个时空碎片，则完成任务
            if(QuestUIManager.QuestManager.quests
            [QuestUIManager.QuestManager.quests.Count-1].collectedAmount==3){
                QuestUIManager.QuestManager.CompleteTask
                ("",QuestUIManager.QuestManager.quests[QuestUIManager.QuestManager.quests.Count-1].id);
                isTalking = true;
                UsePortalHint.gameObject.SetActive(true);
            }
        }
    }
    
}