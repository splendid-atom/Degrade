using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 引入TextMeshPro命名空间
using UnityEngine.UI; // 引入UI命名空间

[System.Serializable] // 使其可以在Inspector中显示
public class Dialogue
{
    public bool isSpeakerPlayer; // 说话者是否为player，不是则为NPC
    public string dialogueText; // 对话内容

    public Dialogue(bool isSpeakerPlayer, string dialogueText)
    {
        this.isSpeakerPlayer = isSpeakerPlayer;
        this.dialogueText = dialogueText;
    }
}


public class DialogueController : MonoBehaviour
{
    private TextMeshProUGUI playerDialogue; // 用来存储TextMeshPro组件
    private TextMeshProUGUI speakerName; // 用来存储TextMeshPro组件
    private GameObject DialogueContainer;
    private List<Dialogue> currentDialogues;
    private List<Dialogue> currentVisitedDialogues;
    private Button dialogueButton; // 用来存储Button组件

    private int currentDialogueIndex = 0; // 当前对话索引
    public List<GameObject> uiElementsToHide; // 用于存储需要隐藏的UI对象列表

    // Start is called before the first frame update
    void Start()
    {
        playerDialogue = GameObject.Find("PlayerDialogue").GetComponent<TextMeshProUGUI>();
        speakerName = GameObject.Find("SpeakerName").GetComponent<TextMeshProUGUI>();
        DialogueContainer = GameObject.Find("DialogueContainer");
        currentDialogues = VillageNpcController.instance.villageNpcDialogues;
        currentVisitedDialogues = VillageNpcController.instance.villageNpcVisitedDialogues;

        // 获取Button组件
        dialogueButton = DialogueContainer.GetComponent<Button>();

        // 给Button添加点击事件监听器
        if (dialogueButton != null)
        {
            dialogueButton.onClick.AddListener(OnDialogueContainerClick);
        }

        DialogueContainer.SetActive(false); // 初始对话框隐藏
    }

    // Update is called once per frame
    void Update()
    {
        DialogueContainer.SetActive(VillageNpcController.instance.isTalking);
        // 隐藏需要隐藏的UI元素
        if(VillageNpcController.instance.isTalking){
           HideUIElements(); 
        }
        // 更新对话内容
        if (currentDialogues.Count > 0)
        {
            playerDialogue.text = currentDialogues[currentDialogueIndex].dialogueText;
            speakerName.text = currentDialogues[currentDialogueIndex].isSpeakerPlayer 
            ? PlayerController.Instance.PlayerName : VillageNpcController.instance.npcName;
        }
        if(VillageNpcController.instance.isVisited){
            currentDialogues = currentVisitedDialogues;
        }
    }

    // 点击DialogueContainer时的事件
    void OnDialogueContainerClick()
    {
        // Debug.Log("Dialogue container clicked!");

        
        // 如果不是最后一条对话，则切换到下一条
        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            currentDialogueIndex++;
        }
        else
        {
            // 如果是最后一条对话，则关闭对话框
            
            VillageNpcController.instance.isTalking = false; // 可以根据你的需求设置，表示对话结束
            VillageNpcController.instance.FadeNpc();
            VillageNpcController.instance.isVisited = true;
            currentDialogueIndex = 0;

            // 恢复所有UI的显示
            ShowUIElements();
            DialogueContainer.SetActive(false);
        }
    }
    // 隐藏需要隐藏的UI元素
    void HideUIElements()
    {
        foreach (GameObject uiElement in uiElementsToHide)
        {
            uiElement.SetActive(false); // 隐藏UI元素
        }
    }

    // 恢复所有UI元素的显示
    void ShowUIElements()
    {
        foreach (GameObject uiElement in uiElementsToHide)
        {
            uiElement.SetActive(true); // 显示UI元素
        }
    }
}
