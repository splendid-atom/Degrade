using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
//space展开 收起 bug
public class QuestUIManager : MonoBehaviour
{
    public GameObject taskPrefab;               // 任务模板
    public RectTransform taskPanel;             // 任务栏
    private Button toggleButton;                // 任务栏展开/收起按钮
    private TextMeshProUGUI toggleButtonText;   // 按钮文本（"+" / "-"）
    private bool isExpanded = true;             // 是否展开

    [System.Serializable]
    public class Quest
    {
        public string title;
        public string description;
    }

    public List<Quest> quests = new List<Quest>(); // 任务列表

    void Start()
    {
        Debug.Log("QuestUIManager 初始化...");
        // 确保任务列表中是否存在任务
        Debug.Log($"任务列表中有 {quests.Count} 个任务");

        // 如果列表中有任务，创建任务项
        foreach (var quest in quests)
        {
            AddTask(quest);
        }
    }

    // 动态创建一个新的任务项
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

        // 获取标题、描述、按钮组件和折叠按钮文本
        TextMeshProUGUI taskTitle = task.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI taskDescription = task.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        Button taskButton = task.transform.Find("Button")?.GetComponent<Button>();
        TextMeshProUGUI taskButtonText = taskButton?.GetComponentInChildren<TextMeshProUGUI>();

        if (taskTitle == null || taskDescription == null || taskButton == null || taskButtonText == null)
        {
            Debug.LogError("任务项的 UI 组件未找到，请检查层级结构！");
            return;
        }

        // 设置标题和描述
        taskTitle.text = quest.title;
        taskDescription.text = quest.description;

        // 为任务按钮绑定点击事件（控制描述显示/隐藏）
        taskButton.onClick.AddListener(() => ToggleTaskDescription(taskDescription, taskButtonText));

        Debug.Log($"任务 {quest.title} 添加成功");
    }

    // 切换任务描述的显示/隐藏，并更新按钮文本
    void ToggleTaskDescription(TextMeshProUGUI taskDescription, TextMeshProUGUI taskButtonText)
    {
        if (taskDescription == null || taskButtonText == null)
        {
            Debug.LogError("taskDescription 或 taskButtonText 为空，无法切换可见性！");
            return;
        }

        bool isActive = taskDescription.gameObject.activeSelf;
        taskDescription.gameObject.SetActive(!isActive);

        // 切换按钮文本
        taskButtonText.text = isActive ? "+" : "-";

        Debug.Log($"任务描述切换为: {(!isActive ? "显示" : "隐藏")}");
    }

    // 切换任务面板的展开/收起
    void TogglePanel()
    {
        isExpanded = !isExpanded;

        // 切换任务栏可见性
        taskPanel.gameObject.SetActive(isExpanded);
        
        // 更新按钮的文本
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isExpanded ? "-" : "+";
        }
    }

    // 调用此方法来添加任务
    public void AddQuest(string title, string description)
    {
        Quest newQuest = new Quest() { title = title, description = description };
        quests.Add(newQuest);

        Debug.Log($"添加新任务: {title}");

        // 创建新的任务并将其添加到面板
        AddTask(newQuest);
    }
}
