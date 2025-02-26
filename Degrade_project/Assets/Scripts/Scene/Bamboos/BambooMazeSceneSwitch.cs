using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro; // 引入TextMeshPro命名空间

public class SwitchToMazeSceneTrigger : MonoBehaviour
{
    public static SwitchToMazeSceneTrigger instance;
    public string targetSceneName = "BambooMazeScene";  // 目标场景名称
    public string playerStartObjectName = "PlayerStartPos";  // 目标位置对象名称

    public CanvasGroup fadeCanvasGroup;  // 用于控制渐变效果的CanvasGroup
    // 新增List，用于存储对话内容
    public List<Dialogue> bambooMazeDialogues;  // 管理每个对话的说话者和内容
    public bool isInMazeSwitchTrigger = false;
    public bool isStartSwitchScene = false;

    private bool isSwitchingScene = false; // 用于标记是否正在进行场景切换

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        if (isStartSwitchScene && !isSwitchingScene)
        {
            // 如果场景切换标志为true且没有正在执行切换场景的协程，则开始执行
            StartCoroutine(SwitchScene());
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isInMazeSwitchTrigger)
        {
            isInMazeSwitchTrigger = true;
        }

        if (!isStartSwitchScene)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            // 当玩家进入触发器时，开始平滑切换场景
            if (!isSwitchingScene)  // 确保场景切换只执行一次
            {
                StartCoroutine(SwitchScene());
            }
        }
    }

    private IEnumerator SwitchScene()
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
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 在新场景加载完成后设置玩家的位置
        GameObject playerStartPosObject = GameObject.Find("PlayerStartPos");  // 查找名为 "PlayerStartPos" 的对象
        if (playerStartPosObject != null)
        {
            // 找到玩家对象
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // 设置玩家在新场景中的位置
                player.transform.position = playerStartPosObject.transform.position;
            }
        }
        else
        {
            Debug.LogError("Player start position object not found in the new scene.");
        }

        // 移除事件监听，防止重复调用
        SceneManager.sceneLoaded -= OnSceneLoaded;
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
}
