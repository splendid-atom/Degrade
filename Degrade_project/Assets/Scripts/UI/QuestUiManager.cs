using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class QuestUIManager : MonoBehaviour
{
    public static QuestUIManager QuestManager;
    // UI 组件引用
    public GameObject taskPrefab;             // 任务模板
    public GameObject taskPrefabCollectives;  // 收集类任务模板
    public RectTransform taskPanel;           // 任务栏面板
    private Button toggleButton;              // 任务栏展开/收起按钮
    private Button completedTasksButton;      // 已完成任务列表按钮
    private TextMeshProUGUI toggleButtonText; // 展开/收起按钮的文本（"+" / "-"）
    private bool isExpanded = true;           // 当前任务栏是否展开
    public Sprite CompletedSprite;
    public Sprite NotCompletedSprite;
    private bool isCompletedPanel = false;
    private RawImage completedTasksButtonImage;
    private bool isCompletedTasksAdded = false;
    private TextMeshProUGUI TaskText;//任务栏标题
    // private bool hasFirstTaskCompleted = false; // 标记是否有第一个任务完成
    private bool isSpecialQuestAdded = false;   // 标记特殊任务是否已添加
    private const int SPECIAL_QUEST_ID = 100;   // 特殊任务的固定ID
    private Quest specialQuest = null;          // 特殊任务引用

    // 滑动条引用（挂在 QuestScroll 对象上的 Scrollbar 组件）
    private Scrollbar questScrollbar;
    private GameObject QuestMask;
    // 定义任务列表可滑动的范围，需根据 QuestMask 遮挡区域设置
    // public float scrollMinY = -200f;  // 下边界（根据实际情况调整）
    // public float scrollMaxY = 0f;     // 上边界（根据实际情况调整）

    // 任务类定义
    [System.Serializable]
    public class Quest
    {
        public int id;
        public string title;
        public string description;
        public bool isCompleted;
        public bool isCollectable;
        private int _collectedAmount; // 使用私有字段
        public int requiredAmount;

        public System.Action OnCollectedAmountChanged;

        public int collectedAmount
        {
            get => _collectedAmount;
            set
            {
                _collectedAmount = value;
                OnCollectedAmountChanged?.Invoke();
            }
        }

        public Quest(int id, string title, string description)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.isCompleted = false;
            this._collectedAmount = 0;
        }
    }

    // 任务列表
    public List<Quest> quests = new List<Quest>();
    public int currentQuestId;
    // 音效
    private AudioSource audioSource;   // 音频源，用于播放音效

    private float TaskPanelInitialY;
    private float TaskPanelHeight;

    private void Awake()
    {
        // 确保实例化只发生一次
        if (QuestManager == null)
        {
            QuestManager = this;
            // DontDestroyOnLoad(gameObject);  
        }
        else
        {
            Destroy(gameObject);  // 如果已经存在，销毁重复的实例            
        }
        // 获取 AudioSource 组件
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        QuestMask = GameObject.Find("QuestMask");
        TaskPanelInitialY = taskPanel.anchoredPosition.y;
        TaskText = GameObject.Find("TaskText").GetComponent<TextMeshProUGUI>();
        completedTasksButton = GameObject.Find("CompletedTasksButton").GetComponent<Button>();
        completedTasksButtonImage = GameObject.Find("CompletedTasksButtonImage").GetComponent<RawImage>();
        // 确保按钮已经绑定
        if (completedTasksButton != null)
        {
            completedTasksButton.onClick.AddListener(OnCompletedTasksButtonClick);
        }
        questScrollbar = GameObject.Find("QuestScroll").GetComponent<Scrollbar>();
        // 绑定滑动条事件：当滑动条数值变化时调用 OnScrollValueChanged 方法
        if (questScrollbar != null)
        {
            questScrollbar.onValueChanged.AddListener(OnScrollValueChanged);
        }
    }

    void Update()
    {
        // Debug.Log("isAllComplete:"+isCompleteAllInitialWorldTasks());
        TaskPanelHeight = taskPanel.sizeDelta.y;
        //令滑块高度和panel高度一致
        questScrollbar.GetComponent<RectTransform>().sizeDelta = new Vector2(
            questScrollbar.GetComponent<RectTransform>().sizeDelta.x, // 保持原来的宽度
            Mathf.Min(taskPanel.sizeDelta.y,QuestMask.GetComponent<RectTransform>().sizeDelta.y)  // 让高度等于 taskPanel
        );
        // 如果当前不是已完成任务面板
        if (!isCompletedPanel)
        {
            // 如果特殊任务已添加且未完成，优先显示特殊任务
            if (isSpecialQuestAdded && specialQuest != null && !specialQuest.isCompleted)
            {
                if (!IsTaskInPanel(specialQuest))
                {
                    if(quests[3].isCompleted){
                        AddTask(specialQuest);
                    }
                    
                }
            }
            // 否则显示普通未完成任务
            foreach (var quest in quests)
            {
                if (quest.isCompleted == false && quest.id != SPECIAL_QUEST_ID)
                {
                    if (quest.id == currentQuestId)
                    {
                        break;
                    }
                    else
                    {
                        AddTask(quest);
                        currentQuestId = quest.id;
                        break;
                    }
                }
            }
            
        }
        // 已完成任务面板逻辑
        else if (isCompletedPanel && !isCompletedTasksAdded)
        {
            foreach (var quest in quests)
            {
                if (quest.isCompleted == true)
                {
                    AddTask(quest);
                }
                if (quest.isCompleted == false || quest.id == quests.Count)
                {
                    isCompletedTasksAdded = true;
                    break;
                }
            }
        }
        // 检查是否是第一个任务完成
        if (quests[0].isCompleted&&!isSpecialQuestAdded)
        {
            AddSpecialQuest(); // 添加特殊任务
        }
    }

    public bool isCompleteAllInitialWorldTasks(){
        foreach (var quest in quests)
        {
            if((quest.id <= 7 || quest.id==100) && !quest.isCompleted)
            {
                return false;
            }
        }
        return true;
    }

    // 处理滑动条数值变化，更新 taskPanel 的 y 坐标，实现任务列表滑动效果
    void OnScrollValueChanged(float value)
    {
        // 使用 Lerp 插值计算新的 y 坐标
        float newY = Mathf.Lerp(TaskPanelInitialY,TaskPanelInitialY+TaskPanelHeight, value);
        taskPanel.anchoredPosition = new Vector2(taskPanel.anchoredPosition.x, newY);
    }

    // 检查任务是否已经在面板中显示
    private bool IsTaskInPanel(Quest quest)
    {
        foreach (Transform task in taskPanel)
        {
            var taskTitle = task.Find("ButtonTittleLayout/Title")?.GetComponent<TextMeshProUGUI>();
            if (taskTitle != null && taskTitle.text == quest.title)
            {
                return true;
            }
        }
        return false;
    }

    // 重置函数，用于清空当前任务面板中的所有任务
    public void ResetTasks()
    {
        foreach (Transform task in taskPanel)
        {
            Destroy(task.gameObject);  // 删除每个任务项
        }
        currentQuestId = 0;
    }

    void OnCompletedTasksButtonClick()
    {
        if (completedTasksButtonImage != null)
        {
            ResetTasks();
            isCompletedPanel = !isCompletedPanel;
            TaskText.text = isCompletedPanel ? "已完成任务" : "当前任务";
            if (!isCompletedPanel)
            {
                completedTasksButtonImage.texture = CompletedSprite.texture;
                isCompletedTasksAdded = false;
            }
            else
            {
                completedTasksButtonImage.texture = NotCompletedSprite.texture;
            }
        }
    }

    public void AddTask(Quest quest)
    {
        GameObject task = null; // 在方法顶部声明 task
        TextMeshProUGUI taskCollectivesCounting=null;
        if (quest.isCollectable)
        {
            task = Instantiate(taskPrefabCollectives, taskPanel);
            taskCollectivesCounting = task.transform.Find("CollectivesCounting")?.GetComponent<TextMeshProUGUI>();
            if (taskCollectivesCounting != null)
            {
                // 初始设置计数文本
                taskCollectivesCounting.text = "-(" + quest.collectedAmount + "/" + quest.requiredAmount + ")";
                // 订阅收集数量变化事件
                quest.OnCollectedAmountChanged += () =>
                {
                    taskCollectivesCounting.text = "-(" + quest.collectedAmount + "/" + quest.requiredAmount + ")";
                };
            }
        }
        else
        {
            task = Instantiate(taskPrefab, taskPanel);
        }

        if (task == null)
        {
            return;
        }

        GameObject horizontalLayout = task.transform.Find("ButtonTittleLayout")?.gameObject;
        TextMeshProUGUI taskTitle = horizontalLayout?.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        Button taskButton = horizontalLayout?.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI taskButtonText = taskButton?.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI taskDescription = task.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        GameObject completeMask = taskTitle.transform.Find("CompleteMask")?.gameObject;
        GameObject completeTaskObj = completeMask?.transform.Find("CompleteTask")?.gameObject;
        RectMask2D mask = completeMask.GetComponent<RectMask2D>();

        if (taskTitle == null || taskDescription == null || taskButton == null || taskButtonText == null || horizontalLayout == null)
        {
            return;
        }

        if (completeTaskObj != null)
        {
            if (mask != null)
            {
                mask.padding = new Vector4(mask.padding.x, mask.padding.y, 300, mask.padding.w);
            }
        }

        taskTitle.text = quest.title;
        taskDescription.text = quest.description;
        if (taskTitle != null)
        {
            taskTitle.maskable = false;
        }

        if (quest.isCompleted)
        {
            if (completeTaskObj != null)
            {
                mask.padding = new Vector4(0, 0, 0, 0);
            }
        }

        var navigation = taskButton.navigation;
        navigation.mode = Navigation.Mode.None;
        taskButton.navigation = navigation;
        taskButton.onClick.AddListener(() => 
        ToggleTaskDescription(taskDescription, taskButtonText,taskCollectivesCounting));
    }

    void ToggleTaskDescription(TextMeshProUGUI taskDescription,
     TextMeshProUGUI taskButtonText,TextMeshProUGUI taskCollectivesCounting)
    {
        if (taskDescription == null || taskButtonText == null)
        {
            return;
        }

        bool isActive = taskDescription.gameObject.activeSelf;
        taskDescription.gameObject.SetActive(!isActive);
        if(taskCollectivesCounting != null){
            taskCollectivesCounting.gameObject.SetActive(!isActive);            
        }
        taskButtonText.text = isActive ? "+" : "-";
    }

    void TogglePanel()
    {
        isExpanded = !isExpanded;
        taskPanel.gameObject.SetActive(isExpanded);
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isExpanded ? "-" : "+";
        }
    }

    public void AddQuest(string title, string description)
    {
        int id = quests.Count > 0 ? quests[quests.Count - 1].id + 1 : 1;
        Quest newQuest = new Quest(id, title, description);
        quests.Add(newQuest);
    }

    public void CompleteTask(string title = null, int? id = null)
    {
        Quest quest = null;

        if (id.HasValue)
        {
            quest = quests.Find(q => q.id == id.Value);
        }
        else if (!string.IsNullOrEmpty(title))
        {
            quest = quests.Find(q => q.title == title);
        }

        if (quest != null)
        {
            quest.isCompleted = true;



            foreach (Transform task in taskPanel)
            {
                var taskTitle = task.Find("ButtonTittleLayout/Title")?.GetComponent<TextMeshProUGUI>();
                GameObject completeMask = taskTitle.transform.Find("CompleteMask")?.gameObject;
                var completeTaskObj = completeMask?.transform.Find("CompleteTask")?.gameObject;

                if (taskTitle != null && taskTitle.text == quest.title)
                {
                    if (completeTaskObj != null)
                    {
                        audioSource.Play();
                        RectMask2D mask = taskTitle.transform.Find("CompleteMask")?.GetComponent<RectMask2D>();
                        if (mask != null)
                        {
                            StartCoroutine(AnimateMaskPadding(mask, 0.15f));
                        }
                    }
                    break;
                }
            }
        }
    }

    // 添加特殊任务（ID=100），直接使用最后一个任务
    public void AddSpecialQuest()
    {
        if (!isSpecialQuestAdded && quests.Count > 0)
        {
            // 获取最后一个任务
            specialQuest = quests[quests.Count - 1];
            // 将其 ID 设置为特殊任务 ID
            specialQuest.id = SPECIAL_QUEST_ID;
            // 标记特殊任务已添加
            isSpecialQuestAdded = true;
        }
    }

    private System.Collections.IEnumerator AnimateMaskPadding(RectMask2D mask, float duration = 0.5f)
    {
        if (mask == null) yield break;

        float startPadding = mask.padding.z;
        float endPadding = 0;
        float elapsedTime = 0f;

        Vector4 padding = mask.padding;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newPaddingRight = Mathf.Lerp(startPadding, endPadding, elapsedTime / duration);
            mask.padding = new Vector4(padding.x, padding.y, newPaddingRight, padding.w);
            yield return null;
        }

        mask.padding = new Vector4(padding.x, padding.y, endPadding, padding.w);
        yield return new WaitForSeconds(1f);
        ResetTasks();
    }
}
