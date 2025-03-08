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

    // Start is called before the first frame update
    void Start()
    {
        WatchHint = GameObject.Find("WatchHint").GetComponent<TextMeshProUGUI>();
        timeMachineMask = GameObject.Find("TimeMachineMask").GetComponent<RawImage>();
        if (timeMachineMask != null)
        {
            canvasGroup = timeMachineMask.GetComponent<CanvasGroup>();
            if (canvasGroup == null)  // 如果没有CanvasGroup组件，添加一个
            {
                canvasGroup = timeMachineMask.gameObject.AddComponent<CanvasGroup>();
            }
        }
        WatchHint.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        ToggleWatchHintDisplay();
        // 当isTimeMachine由false变为true时，执行TimeMachine
        if (!wasTimeMachine && isTimeMachine)
        {
            TimeMachine();
            // 更新wasTimeMachine的状态
            wasTimeMachine = true;
        }
        if (!wasSecondTimeMachine && isSecondTimeMachine)
        {
            TimeMachine();
            StartCoroutine(WaitAndStartDegradeBamboo());
            // 更新wasTimeMachine的状态
            wasSecondTimeMachine = true;
        }

    }
    IEnumerator WaitAndStartDegradeBamboo()
    {
        yield return new WaitForSeconds(3f);  // Wait for 3 seconds
        DegradeBambooForest.instance.StartWilt();
    }
    public void ToggleWatchHintDisplay(){
        if(QuestUIManager.QuestManager.currentQuestId==4&&
        !isTimeMachine&&!wasTimeMachine){
            WatchHint.gameObject.SetActive(true);
            return;
        }

        WatchHint.gameObject.SetActive(false);
    }


    // TimeMachine函数，控制遮罩的渐变透明度
    void TimeMachine()
    {
        if (canvasGroup != null)
        {
            StartCoroutine(FadeInAndOut());
        }
    }

    // 控制渐变的协程
    IEnumerator FadeInAndOut()
    {
        // 渐变透明度为1
        float elapsedTime = 0f;
        float duration = 1f;  // 渐变持续时间为1秒
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);  // 从0到1的渐变
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 1f;  // 确保完全透明

        // 等待1秒
        yield return new WaitForSeconds(1f);

        // 渐变透明度为0
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);  // 从1到0的渐变
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = 0f;  // 确保完全透明
        isTimeMachineMasked = true;

    }
}
