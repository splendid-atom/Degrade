using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // 导入SceneManager

public class InterSceneMemory : MonoBehaviour
{
    public static InterSceneMemory instance;
    public string currentSceneName;
    public string lastSceneName;
    public bool setPlayerPos = false;
    public bool isBeenToBambooMaze = false;
    public List<bool> questCompletions = new List<bool>();
    public bool isBridgeRised = false;
    void Awake()
    {
        if (instance == null)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            lastSceneName = currentSceneName;
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 监听场景加载事件
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        // 确保 QuestManager 存在
        if (QuestUIManager.QuestManager != null)
        {
            var quests = QuestUIManager.QuestManager.quests;

            // 重新调整 questCompletions 以匹配 quests 长度
            if (questCompletions.Count != quests.Count)
            {
                questCompletions = new List<bool>(new bool[quests.Count]);
            }

            // 更新任务完成状态
            for (int i = 0; i < quests.Count; i++)
            {
                questCompletions[i] = quests[i].isCompleted;
            }

            // 打印所有任务状态
            string questStatus = "Quest Completion Status: ";
            for (int i = 0; i < questCompletions.Count; i++)
            {
                questStatus += $"Quest {i}: {questCompletions[i]}  ";
            }
            // Debug.Log(questStatus);
        }

        // 记录竹林迷宫进入状态
        if (currentSceneName == "BambooMazeScene" && !isBeenToBambooMaze)
        {
            isBeenToBambooMaze = true;
        }

        // 检测场景切换
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName != currentSceneName)
        {
            lastSceneName = currentSceneName;
            currentSceneName = activeSceneName;
        }

        // 处理玩家位置
        if (!setPlayerPos && lastSceneName == "BambooMazeScene" && currentSceneName == "SampleScene")
        {
            GameObject playerStart = GameObject.Find("PlayerStartPos");
            if (playerStart != null && PlayerController.Instance != null)
            {
                PlayerController.Instance.transform.position = playerStart.transform.position;
                setPlayerPos = true;
            }
        }
    }

    // 当场景加载时，恢复任务完成状态
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
       if (QuestUIManager.QuestManager != null)
        {
            if(!QuestUIManager.QuestManager.quests
            [QuestUIManager.QuestManager.quests.Count - 1].isCompleted){
                QuestUIManager.QuestManager.AddSpecialQuest();
            }
            var quests = QuestUIManager.QuestManager.quests;
            int count = Mathf.Min(quests.Count, questCompletions.Count);
            
            for (int i = 0; i < count; i++)
            {
                quests[i].isCompleted = questCompletions[i];
            }
            
            // Debug.Log("任务状态已恢复至 QuestManager");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 取消监听，防止内存泄漏
    }
}
