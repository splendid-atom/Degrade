using System.Collections;
using UnityEngine;
using UnityEngine.UI;  // 引入UI命名空间以使用RawImage和CanvasGroup
using TMPro;

public class VillageSceneController : MonoBehaviour
{
    public static VillageSceneController instance;
    public bool isTimeMachine = false;  // 标记是否处于时间机器状态
    public bool wasTimeMachine = false;  // 记录上一次的时间机器状态
    private RawImage timeMachineMask;  // 需要设置的RawImage（遮罩）
    private CanvasGroup canvasGroup;  // 用来控制透明度的CanvasGroup
    public bool isTimeMachineMasked = false;
    private TextMeshProUGUI WatchHint; // 提醒使用时空手表
    public bool isSecondTimeMachine = false;  // 标记是否处于时间机器状态
    public bool wasSecondTimeMachine = false;  // 记录上一次的时间机器状态

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GameObject watchHintObj = GameObject.Find("WatchHint");
        if (watchHintObj != null)
        {
            WatchHint = watchHintObj.GetComponent<TextMeshProUGUI>();
        }

        GameObject maskObj = GameObject.Find("TimeMachineMask");
        if (maskObj != null)
        {
            timeMachineMask = maskObj.GetComponent<RawImage>();
            if (timeMachineMask != null)
            {
                canvasGroup = timeMachineMask.GetComponent<CanvasGroup>();
                if (canvasGroup == null)  // 如果没有CanvasGroup组件，添加一个
                {
                    canvasGroup = timeMachineMask.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        if (WatchHint != null)
        {
            WatchHint.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        ToggleWatchHintDisplay();
        
        if (wasTimeMachine != null && isTimeMachine != null)
        {
            if (!wasTimeMachine && isTimeMachine)
            {
                TimeMachine();
                wasTimeMachine = true;
            }
        }

        if (wasSecondTimeMachine != null && isSecondTimeMachine != null)
        {
            if (!wasSecondTimeMachine && isSecondTimeMachine)
            {
                TimeMachine();
                StartCoroutine(WaitAndStartDegradeBamboo());
                wasSecondTimeMachine = true;
            }
        }
    }

    IEnumerator WaitAndStartDegradeBamboo()
    {
        yield return new WaitForSeconds(3f);  // Wait for 3 seconds
        if (DegradeBambooForest.instance != null)
        {
            DegradeBambooForest.instance.StartWilt();
        }
    }

    public void ToggleWatchHintDisplay()
    {
        if (QuestUIManager.QuestManager != null && 
            QuestUIManager.QuestManager.currentQuestId != null &&
            WatchHint != null)
        {
            if (QuestUIManager.QuestManager.currentQuestId == 4 &&
                isTimeMachine != null && 
                wasTimeMachine != null &&
                !isTimeMachine && 
                !wasTimeMachine)
            {
                WatchHint.gameObject.SetActive(true);
                return;
            }

            WatchHint.gameObject.SetActive(false);
        }
    }

    void TimeMachine()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeInAndOut());
        }
    }

    IEnumerator FadeInAndOut()
    {
        if (canvasGroup != null)
        {
            float elapsedTime = 0f;
            float duration = 1f;
            while (elapsedTime < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 1f;

            yield return new WaitForSeconds(1f);

            elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = 0f;
            isTimeMachineMasked = true;
        }
    }
}