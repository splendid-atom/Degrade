// InterSceneMemory.cs
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
    public List<Transform> bambooMazeChilds = new List<Transform>();
    public List<Transform> sampleSceneChilds = new List<Transform>();
    public bool isBambooMazeChildsStored = false;
    public bool isSampleSceneChildsStored = false;
    public bool isSwitchToFactory1 = false;
    public string targetSceneName = "Factory1";  // 目标场景名称
    public CanvasGroup fadeCanvasGroup;  // 用于控制渐变效果的CanvasGroup
    public bool isStartSwitchScene = false;

    private bool isSwitchingScene = false; // 用于标记是否正在进行场景切换s
    public bool isGameStart = true;
    public bool isTimeSlowed = false;
    void Awake()
    {
        if (instance == null)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            lastSceneName = currentSceneName;
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded; // 监听场景加载事件
            // SceneManager.LoadSceneAsync("Menu");
        }
        else
        {
            Destroy(gameObject);
        }

        
    }

    private IEnumerator SwitchToFactory()
    {
        isSwitchingScene = true;  // 标记为正在切换场景

        // 先进行黑屏渐变
        yield return FadeOut();

        // 异步加载目标场景，并设置不自动激活
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;  // 阻止场景自动激活

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)  // 当进度达到 90% 时表示场景已加载完毕
            {
                // 此时可以开始设置玩家位置
                SceneManager.sceneLoaded += OnSceneLoaded;
                break;
            }
            yield return null;
        }

        // 延迟一小段时间，确保所有对象初始化完毕
        yield return new WaitForSeconds(0.5f); // 延迟半秒
        // 等待玩家位置设置完成后再激活场景
        asyncLoad.allowSceneActivation = true;

        // 切换场景后进行渐变恢复
        yield return FadeIn();

        isSwitchingScene = false;  // 标记为切换完成
        isSwitchToFactory1 = false;
    }
    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(1f);  // 等待 1 秒
        // 让黑屏的透明度逐渐增加到1
        float timeElapsed = 0f;
        float fadeDuration = 1f;  // 渐变时长，可以根据需要调整
        while (timeElapsed < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1;  // 确保最终为完全不透明
    }

    private IEnumerator FadeIn()
    {
        // 让黑屏的透明度逐渐降低到0
        float timeElapsed = 0f;
        float fadeDuration = 1f;  // 渐变时长，可以根据需要调整
        while (timeElapsed < fadeDuration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(1, 0, timeElapsed / fadeDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 0;  // 确保最终为完全透明
    }
    public bool isInSampleScene()
    {
        return currentSceneName == "SampleScene";
    }
    public bool isInBambooMaze()
    {
        return currentSceneName == "BambooMazeScene";
    }
    public bool isInFactory1(){
        return currentSceneName == "Factory1";
    }
    void Update()
    {
        // 检测场景切换
        string activeSceneName = SceneManager.GetActiveScene().name;
        if (activeSceneName != currentSceneName)
        {
            lastSceneName = currentSceneName;
            currentSceneName = activeSceneName;
        }

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
        }

        // 记录竹林迷宫进入状态
        if (currentSceneName == "BambooMazeScene" && !isBeenToBambooMaze)
        {
            isBeenToBambooMaze = true;
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
        if (isSwitchToFactory1 && !isSwitchingScene)
        {
            // 如果场景切换标志为true且没有正在执行切换场景的协程，则开始执行
            StartCoroutine(SwitchToFactory());
        }

    }

    // 当场景加载时，恢复任务完成状态
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string loadedSceneName = scene.name; // 直接获取新加载的场景
        fadeCanvasGroup.alpha = 0;
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
        }

        // **修正存储逻辑**
        if (loadedSceneName == "BambooMazeScene")
        {
            if (!isBambooMazeChildsStored)
            {
                isBambooMazeChildsStored = true;
                bambooMazeChilds = new List<Transform>(FacingCamera.instance.childs);
            }
            else
            {
                FacingCamera.instance.RestoreChilds(bambooMazeChilds);
            }
        }
        else if (loadedSceneName == "SampleScene")
        {
            if (!isSampleSceneChildsStored)
            {
                isSampleSceneChildsStored = true;
                sampleSceneChilds = new List<Transform>(FacingCamera.instance.childs);
            }
            else
            {
                FacingCamera.instance.RestoreChilds(sampleSceneChilds);
            }
        }

        FacingCamera.instance.UpdateChilds(true);
    }


    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // 取消监听，防止内存泄漏
    }
}