using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BambooMazeExitTrigger : MonoBehaviour
{
    public static BambooMazeExitTrigger instance;
    private Collider2D BambooMazeTrigger; // 修正大小写
    public bool isInMaze = false;

    public CanvasGroup fadeCanvasGroup;  // 用于控制渐变效果的CanvasGroup
    public string targetSceneName = "SampleScene";  // 返回的目标场景名称
    public string playerStartObjectName = "PlayerStartPos";  // 玩家重生的位置对象名称

    private bool isSwitchingScene = false;  // 用于标记是否正在进行场景切换

    void Awake()
    {
        instance = this;
        BambooMazeTrigger = GetComponent<Collider2D>(); // 修正大小写
    }

    void Update()
    {
        // 不需要每帧做什么逻辑，主要通过触发器触发场景切换
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("玩家进入触发器: " + gameObject.name);
        if (collision.gameObject.CompareTag("Player"))
        {
            isInMaze = false;
            if (!isSwitchingScene)
            {
                StartCoroutine(SwitchScene());  // 当玩家离开迷宫时，启动场景切换协程
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("玩家离开触发器: " + gameObject.name);

    }

    private IEnumerator SwitchScene()
    {
        isSwitchingScene = true;  // 标记为正在切换场景

        // 先进行黑屏渐变
        yield return FadeOut();

        // 异步加载目标场景，并注册回调
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;  // 阻止场景自动激活，等加载完成后手动激活

        // 等待场景加载完成
        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                // 当场景加载完成（90%表示加载完成，剩余10%等待手动激活）
                // 触发场景加载完成后的设置
                SceneManager.sceneLoaded += OnSceneLoaded;
                break;
            }
            yield return null;
        }



        // 激活场景
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
