using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverAnimation : MonoBehaviour
{
    public static RiverAnimation instance;
    private BoxCollider2D triggerCollider;
    public Material cleanRiverMaterial;
    public Material pollutedRiverMaterial;
    public bool isPolluted = false;
    public bool isPlayerInTrigger = false;
    private GameObject pollutedRiverCover;
    private Transform targetRect;
    public float shrinkSpeed = 50f; // 控制缩小速度
    private bool isShrinking = false;
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (targetRect == null)
        {
            targetRect = GetComponent<Transform>();
        }
        pollutedRiverCover = GameObject.Find("pollutedRiverCover");

        if (pollutedRiverCover == null)
        {
            Debug.LogError("RiverAnimation: 找不到 PollutedRiverCover！");
            return;
        }

        // 获取所有 BoxCollider2D 组件
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();

        // 确保至少有两个 BoxCollider2D
        if (colliders.Length >= 2)
        {
            triggerCollider = colliders[1]; // 获取第二个 BoxCollider2D
            triggerCollider.isTrigger = true; // 确保它是触发器
        }
        else
        {
            Debug.LogError("RiverAnimation: 该对象没有足够的 BoxCollider2D 组件！");
        }
    }

    void Update()
    {
        if (VillageSceneController.instance.isTimeMachine)
        {
            if (!isPolluted)
            {
                SetRiverMaterialClean();
            }
            if (QuestUIManager.QuestManager.quests[3].isCompleted)
            {
                isPolluted = true;
            }
        }

        // if (Input.GetKeyDown(KeyCode.P)) // 按下 P 开始缩放
        // {
        //     StartCoroutine(ShrinkCoroutine(3f)); // 3秒内完成缩放
        // }   
    }
    public void StartShrink()
    {
        if(!isShrinking){
            isShrinking = true;
            StartCoroutine(ShrinkCoroutine(3f)); // 3秒内完成缩放
        }
        
    }

    IEnumerator ShrinkCoroutine(float duration)
    {
        yield return new WaitForSeconds(1.5f);
        float elapsedTime = 0f;
        float initialTargetY = targetRect.localScale.y;
        float initialPollutedY = pollutedRiverCover.transform.localScale.y;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / duration; // 计算进度（0~1）

            // 计算新的 Y 轴缩放值
            float newTargetY = Mathf.Lerp(initialTargetY, 0, progress);
            float newPollutedY = Mathf.Lerp(initialPollutedY, initialPollutedY + initialTargetY, progress);

            // 应用缩放
            targetRect.localScale = new Vector3(targetRect.localScale.x, newTargetY, targetRect.localScale.z);
            pollutedRiverCover.transform.localScale = new Vector3(pollutedRiverCover.transform.localScale.x, newPollutedY, pollutedRiverCover.transform.localScale.z);

            yield return null; // 等待下一帧
        }

        // 确保最终值正确
        targetRect.localScale = new Vector3(targetRect.localScale.x, 0, targetRect.localScale.z);
        pollutedRiverCover.transform.localScale = new Vector3(pollutedRiverCover.transform.localScale.x, initialPollutedY + initialTargetY, pollutedRiverCover.transform.localScale.z);
    }
    // void Shrink()
    // {
    //     while(targetRect.localScale.y > 0){}
    //     float shrinkAmount = shrinkSpeed * Time.deltaTime * 0.01f;

    //     // 目标河流缩小
    //     Vector3 targetScale = targetRect.localScale;
    //     float newTargetY = Mathf.Max(0, targetScale.y - shrinkAmount);

    //     // 污染河流覆盖物扩大
    //     Vector3 pollutedScale = pollutedRiverCover.transform.localScale;
    //     float newPollutedY = pollutedScale.y + (targetScale.y - newTargetY); // 保持整体高度不变

    //     // 应用新的缩放
    //     targetRect.localScale = new Vector3(targetScale.x, newTargetY, targetScale.z);
    //     pollutedRiverCover.transform.localScale = new Vector3(pollutedScale.x, newPollutedY, pollutedScale.z);
    // }

    public void SetRiverMaterialPolluted()
    {
        SetRiverMaterial(pollutedRiverMaterial); // 切换材质 
    }

    public void SetRiverMaterialClean()
    {
        SetRiverMaterial(cleanRiverMaterial); // 切换材质 
    }

    private void SetRiverMaterial(Material newMaterial)
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = newMaterial;
        }
        else
        {
            Debug.LogError("RiverAnimation: 找不到 Renderer 组件！");
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerCollider != null && other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            Debug.Log("玩家进入了 River 的触发器！");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (triggerCollider != null && other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            Debug.Log("玩家离开了 River 的触发器！");
        }
    }
}
