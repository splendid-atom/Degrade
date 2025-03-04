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
    public static DialogueController instance;
    private TextMeshProUGUI playerDialogue; // 用来存储TextMeshPro组件
    private TextMeshProUGUI speakerName; // 用来存储TextMeshPro组件
    private string currentNpcName; // 用来存储TextMeshPro组件
    private GameObject DialogueContainer;
    private List<Dialogue> currentDialogues;
    private List<Dialogue> currentVisitedDialogues;
    private Button dialogueButton; // 用来存储Button组件

    private int currentDialogueIndex = 0; // 当前对话索引
    public List<GameObject> uiElementsToHide; // 用于存储需要隐藏的UI对象列表
    private RectMask2D mask;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        playerDialogue = GameObject.Find("PlayerDialogue").GetComponent<TextMeshProUGUI>();
        speakerName = GameObject.Find("SpeakerName").GetComponent<TextMeshProUGUI>();
        DialogueContainer = GameObject.Find("DialogueContainer");
        currentDialogues = VillageNpcController.instance.villageNpcDialogues;
        currentVisitedDialogues = VillageNpcController.instance.villageNpcVisitedDialogues;
        mask = GameObject.Find("DialogueContainerMask").GetComponent<RectMask2D>();
        // 获取Button组件
        dialogueButton = DialogueContainer.GetComponent<Button>();

        // 给Button添加点击事件监听器
        if (dialogueButton != null)
        {
            dialogueButton.onClick.AddListener(OnDialogueContainerClick);
        }

        // DialogueContainer.SetActive(false); // 初始对话框隐藏
        mask.padding = new Vector4(1300, 0, 0, 0);
    }



    // Update is called once per frame
    void Update()
    {
        switchingCurrentDialogues();    
        ShowDialogueContainer();
        // 隐藏需要隐藏的UI元素
        if(isHavingDialogue()){
           HideUIElements(); 
        }
        // 更新对话内容
        if (currentDialogues.Count > 0)
        {
            playerDialogue.text = currentDialogues[currentDialogueIndex].dialogueText;
            speakerName.text = currentDialogues[currentDialogueIndex].isSpeakerPlayer 
            ? PlayerController.Instance.PlayerName : currentNpcName;
        }
        if(VillageNpcController.instance.isVisited){
            currentDialogues = currentVisitedDialogues;
        }
    }

    //是否需要隐藏画面上的其他UI
    //并且处理切换dialogue的逻辑
    public bool isHavingDialogue(){
        if(VillageNpcController.instance.isTalking||
        NewPlayerGuide.instance.isGuiding||
        SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger||
        HeepAnimation.instance.isHeepDialogue){
            if(VillageNpcController.instance.isTalking){
                currentNpcName = VillageNpcController.instance.npcName;
            }
            if(NewPlayerGuide.instance.isGuiding||
            SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger||
            HeepAnimation.instance.isHeepDialogue){
                currentNpcName = NewPlayerGuide.instance.npcName;
            }

            return true;
        }
        return false;
    }

    void switchingCurrentDialogues(){
        if(VillageNpcController.instance.isVisited){
            currentDialogues = VillageNpcController.instance.villageNpcVisitedDialogues;
        }
        if(NewPlayerGuide.instance.isGuiding){
            currentDialogues = NewPlayerGuide.instance.NewPlayerDialogues;
        }
        if(SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger){
            currentDialogues = SwitchToMazeSceneTrigger.instance.bambooMazeDialogues;
        }
        if(HeepAnimation.instance.isHeepDialogue){
            currentDialogues = HeepAnimation.instance.HeepDialogues;
        }
    }

    // 点击DialogueContainer时的事件
    void OnDialogueContainerClick()
    {
        Debug.Log("DialogueContainer clicked!");    
        // 如果不是最后一条对话，则切换到下一条
        if (currentDialogueIndex < currentDialogues.Count - 1)
        {
            currentDialogueIndex++;
        }
        else
        {
            // 如果是最后一条对话，则关闭对话框
            if(VillageNpcController.instance.isTalking){

                VillageNpcController.instance.isTalking = false; // 可以根据你的需求设置，表示对话结束
                VillageNpcController.instance.FadeNpc();
                if(!VillageNpcController.instance.isVisited){
                    // 完成任务
                    QuestUIManager.QuestManager.CompleteTask("", 3);
                }
                VillageNpcController.instance.isVisited = true;                
            }
            if(NewPlayerGuide.instance.isGuiding){
                NewPlayerGuide.instance.isGuiding = false;
                NewPlayerGuide.instance.isNewPlayerGuiding = true;
                HintUI.instance.isTalkingOver = true;
            }
            if(SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger){
                SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger = false;
                SwitchToMazeSceneTrigger.instance.isStartSwitchScene = true;
            }
            if(HeepAnimation.instance.isHeepDialogue){
                HeepAnimation.instance.isHeepDialogue = false;
                QuestUIManager.QuestManager.CompleteTask("", 4);
                // 标记 Heep 对话已显示并保存
                // HeepAnimation.instance.hasHeepDialogueShown = true;
                // PlayerPrefs.SetInt("HasHeepDialogueShown", 1);
                // PlayerPrefs.Save();
                HeepAnimation.instance.OnDialogueEnd();
            }
            currentDialogueIndex = 0;

            // 恢复所有UI的显示
            ShowUIElements();
            // DialogueContainer.SetActive(false);
            mask.padding = new Vector4(1300, 0, 0, 0);

        }
    }



    
    // 隐藏需要隐藏的UI元素
    void HideUIElements()
    {
        foreach (GameObject uiElement in uiElementsToHide)
        {
            uiElement.SetActive(false); // 隐藏UI元素
        }
        BigMapController.instance.CloseMap();
    }

    // 恢复所有UI元素的显示
    void ShowUIElements()
    {
        foreach (GameObject uiElement in uiElementsToHide)
        {
            uiElement.SetActive(true); // 显示UI元素
        }
    }
    void ShowDialogueContainer()
    {
        //村民开门1.5秒后才显示对话框
        if(VillageNpcController.instance.isTalking){
            StartCoroutine(DelayedDialogueContainer());
        }
        //电话的对话框立即显示
        else{
            // DialogueContainer.SetActive(isHavingDialogue());
            mask.padding = new Vector4(isHavingDialogue()?0:1300, 0, 0, 0);
        }
        
    }
    // 使用协程来延迟执行
    IEnumerator DelayedDialogueContainer()
    {
        yield return new WaitForSeconds(1.5f);  // 延迟 1 秒
        // DialogueContainer.SetActive(isHavingDialogue());
        mask.padding = new Vector4(isHavingDialogue()?0:1300, 0, 0, 0);

    }

}
