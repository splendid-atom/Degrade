using UnityEngine;
using System.Collections;

public class TimePieceTrigger : MonoBehaviour
{
    public FetchItemController controller; // 控制器引用
    private bool isPlayerInTrigger = false; // 追踪玩家是否在触发区域
    public float collectFloatUnit = 1f;     // 上升距离
    public float collectDuration = 0.5f;    // 动画持续时间

    void Update()
    {
        // 只有玩家在触发区时才检测按键
        if (isPlayerInTrigger && Input.GetKeyDown(KeyCode.F))
        {
            // Debug.Log("Timepiece is fetched");
            StartCoroutine(FloatUpAndDisappear()); // 启动协程执行动画
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = true;
            controller?.PlayerEnteredTrigger(); // 通知控制器玩家进入
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
            controller?.PlayerExitedTrigger(); // 通知控制器玩家离开
        }
    }

    private IEnumerator FloatUpAndDisappear()
    {
        // 获取 SpriteRenderer 组件以调整 Sorting Layer 和透明度
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingLayerName = "Foreground"; // 设置 Sorting Layer 为 "Foreground"
            spriteRenderer.sortingOrder = 1; // 设置 Sorting Order 为 1
        }

        // 定义动画参数
        float duration = collectDuration; // 动画持续时间（秒）
        float elapsed = 0f;
        Vector3 startPosition = transform.position; // 动画起始位置（世界坐标）
        Vector3 cameraUp = Camera.main.transform.up; // 获取镜头的向上方向
        Vector3 endPosition = startPosition + cameraUp * collectFloatUnit; // 计算结束位置

        // 执行上升和渐变动画
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPosition, endPosition, t); // 在世界坐标系中插值

            // 在动画最后 0.2 秒开始渐变消失
            if (elapsed >= duration - 0.2f && spriteRenderer != null)
            {
                float fadeProgress = (elapsed - (duration - 0.2f)) / 0.2f; // 计算渐变进度
                Color color = spriteRenderer.color;
                color.a = Mathf.Lerp(1f, 0f, fadeProgress); // 从不透明过渡到透明
                spriteRenderer.color = color;
            }

            yield return null; // 等待下一帧
        }

        // 确保最终状态为完全透明
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f; // 设置为完全透明
            spriteRenderer.color = color;
        }

        // 动画完成后通知控制器并隐藏物体
        controller?.RemoveTimePieceAndCount(gameObject);
        gameObject.SetActive(false);
    }
}