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
    public RectTransform taskPanel;           // 任务栏面板
    private Button toggleButton;              // 任务栏展开/收起按钮
    private TextMeshProUGUI toggleButtonText; // 展开/收起按钮的文本（"+" / "-"）
    private bool isExpanded = true;           // 当前任务栏是否展开

    // 任务类定义
    [System.Serializable]
    public class Quest
    {
        public int id;           // 任务ID
        public string title;     // 任务标题
        public string description; // 任务描述
        public bool isCompleted; // 任务是否完成

        // 构造函数，在创建任务时自动分配 ID
        public Quest(int id, string title, string description)
        {
            this.id = id;
            this.title = title;
            this.description = description;
            this.isCompleted = false; // 默认未完成
        }
    }

    // 任务列表
    public List<Quest> quests = new List<Quest>();
    private void Awake()
    {
        // 确保实例化只发生一次
        if (QuestManager == null)
            QuestManager = this;
        else
            Destroy(gameObject);  // 如果已经存在，销毁重复的实例
    }
    // 初始化
    void Start()
    {
        Debug.Log("QuestUIManager 初始化...");

        // 检查任务列表
        Debug.Log($"任务列表中有 {quests.Count} 个任务");

        // 如果任务列表不为空，创建任务项
        foreach (var quest in quests)
        {
            AddTask(quest);
        }
    }

    // 动态创建任务项并添加到任务栏
    public void AddTask(Quest quest)
    {
        Debug.Log($"正在创建任务: {quest.title}");

        // 创建任务对象（克隆 taskPrefab）
        GameObject task = Instantiate(taskPrefab, taskPanel);

        if (task == null)
        {
            Debug.LogError("任务对象创建失败！");
            return;
        }

        // 获取任务项的 UI 组件
        GameObject horizontalLayout = task.transform.Find("ButtonTittleLayout")?.gameObject;
        TextMeshProUGUI taskTitle = horizontalLayout?.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        Button taskButton = horizontalLayout?.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI taskButtonText = taskButton?.GetComponentInChildren<TextMeshProUGUI>();
        TextMeshProUGUI taskDescription = task.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        
        // 获取 CompleteTask 作为 Title 的子对象
        GameObject completeTaskObj = taskTitle?.transform.Find("CompleteTask")?.gameObject; // 获取 CompleteTask 子对象

        // 检查 UI 组件是否完整
        if (taskTitle == null || taskDescription == null || taskButton == null || taskButtonText == null || horizontalLayout == null)
        {
            Debug.LogError("任务项的 UI 组件未找到，请检查层级结构！");
            return;
        }

        // 将 CompleteTask 初始设置为隐藏
        if (completeTaskObj != null)
        {
            completeTaskObj.SetActive(false); // 初始时隐藏
        }

        // 设置标题和描述
        taskTitle.text = quest.title;
        taskDescription.text = quest.description;

        // 如果任务完成，为标题添加“（已完成）”
        if (quest.isCompleted)
        {
            taskTitle.text = $"{quest.title}(已完成)"; // 在任务标题末尾添加“（已完成）”
            // 显示 CompleteTask
            if (completeTaskObj != null)
            {
                completeTaskObj.SetActive(true); // 显示 CompleteTask
            }
        }

        // 禁用按钮的导航，防止空格键触发按钮
        var navigation = taskButton.navigation;
        navigation.mode = Navigation.Mode.None; // 禁用导航
        taskButton.navigation = navigation;

        // 为任务按钮绑定点击事件（切换任务描述的显示/隐藏）
        taskButton.onClick.AddListener(() => ToggleTaskDescription(taskDescription, taskButtonText));

        Debug.Log($"任务 {quest.title} 添加成功");
    }

    // 切换任务描述的显示/隐藏
    void ToggleTaskDescription(TextMeshProUGUI taskDescription, TextMeshProUGUI taskButtonText)
    {
        if (taskDescription == null || taskButtonText == null)
        {
            Debug.LogError("taskDescription 或 taskButtonText 为空，无法切换可见性！");
            return;
        }

        bool isActive = taskDescription.gameObject.activeSelf;
        taskDescription.gameObject.SetActive(!isActive);

        // 更新按钮文本
        taskButtonText.text = isActive ? "+" : "-";

        Debug.Log($"任务描述切换为: {(!isActive ? "显示" : "隐藏")}");
    }

    // 切换任务面板的展开/收起
    void TogglePanel()
    {
        isExpanded = !isExpanded;

        // 切换任务栏面板的可见性
        taskPanel.gameObject.SetActive(isExpanded);

        // 更新按钮的文本
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isExpanded ? "-" : "+";
        }
    }

    // 用于添加新任务
    public void AddQuest(string title, string description)
    {
        int id = quests.Count > 0 ? quests[quests.Count - 1].id + 1 : 1; // 自动分配 ID
        Quest newQuest = new Quest(id, title, description);
        quests.Add(newQuest);

        Debug.Log($"添加新任务: {title}");

        // 创建新的任务并将其添加到任务栏
        AddTask(newQuest);
    }

    // 用于标记任务为完成并更新UI
    public void CompleteTask(string title = null, int? id = null)
    {
        Quest quest = null;

        // 如果提供了id，则通过id查找任务
        if (id.HasValue)
        {
            quest = quests.Find(q => q.id == id.Value);
        }
        // 如果没有提供id，但提供了title，则通过title查找任务
        else if (!string.IsNullOrEmpty(title))
        {
            quest = quests.Find(q => q.title == title);
        }

        // 如果找到任务，则标记为完成并更新UI
        if (quest != null)
        {
            quest.isCompleted = true; // 将任务标记为完成

            // 更新UI显示
            foreach (Transform task in taskPanel)
            {
                var taskTitle = task.Find("ButtonTittleLayout/Title")?.GetComponent<TextMeshProUGUI>();
                var completeTaskObj = taskTitle?.transform.Find("CompleteTask")?.gameObject; // 获取 CompleteTask 作为 title 的子对象

                if (taskTitle != null && taskTitle.text == quest.title)
                {
                    // 将 CompleteTask 从隐藏状态改为显示
                    if (completeTaskObj != null)
                    {
                        completeTaskObj.SetActive(true); // 显示 CompleteTask
                    }
                    break;
                }
            }

            Debug.Log($"任务 {quest.title} 已完成");
        }
        else
        {
            string errorMsg = title != null ? $"未找到名为 {title} 的任务！" : $"未找到 ID 为 {id} 的任务！";
            Debug.LogError(errorMsg);
        }
    }


}
