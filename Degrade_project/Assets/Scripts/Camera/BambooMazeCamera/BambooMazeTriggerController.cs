using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BambooMazeTriggerController : MonoBehaviour
{
    public static BambooMazeTriggerController instance;
    private Collider2D BambooMazeTrigger;
    public bool isInMaze = false;
    public TextMeshProUGUI BambooMovementHint;  // Changed to TextMeshProUGUI for UI text
    private bool isHintShowed = false;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // 只有当场景为BambooMazeScene时才初始化提示

            if (GameObject.Find("BambooMovementHint"))
            {
                Debug.Log("BambooMovementHint");
                BambooMovementHint = GameObject.Find("BambooMovementHint").GetComponent<TextMeshProUGUI>(); // 获取TextMeshProUGUI组件
            }

            if (BambooMovementHint != null)
            {
                // 初始化时完全透明
                SetAlpha(0f);  // 设置 alpha 为 0 使文本不可见
            }
            else
            {
                Debug.LogError("BambooMovementHint GameObject 没有找到或没有附加TextMeshProUGUI组件！");
            }

            BambooMazeTrigger = GetComponent<Collider2D>();

            if (BambooMazeTrigger == null)
            {
                Debug.LogError("没有找到Collider2D组件！");
            }
        
    }

    void Update()
    {
        // 可以在这里放置其他逻辑，当前没有任何操作
    }

    // 玩家进入触发器
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !InterSceneMemory.instance.isInSampleScene())
        {
            isInMaze = true;
            // 显示提示信息，并开始渐显
            StartCoroutine(FadeInHint());
            BambooMazeCameraController.instance.isInMaze = true;
        }
    }

    // 玩家离开触发器
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !InterSceneMemory.instance.isInSampleScene())
        {
            isInMaze = false;
            // 隐藏提示信息，并开始渐隐
            if (!isHintShowed)
            {
                StartCoroutine(FadeOutHint());
            }
            BambooMazeCameraController.instance.isInMaze = false;
        }
    }

    // 渐显提示信息
    private IEnumerator FadeInHint()
    {
        float timeElapsed = 0f;
        float fadeDuration = 2f;  // 渐变时长（两秒）

        // 渐显效果
        while (timeElapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timeElapsed / fadeDuration);
            SetAlpha(alpha);  // 设置 alpha 值
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(1);  // 确保最终为完全不透明
        yield return new WaitForSeconds(1f);
        StartCoroutine(FadeOutHint());
        isHintShowed = true;
    }

    // 渐隐提示信息
    private IEnumerator FadeOutHint()
    {
        float timeElapsed = 0f;
        float fadeDuration = 1f;  // 渐变时长（两秒）

        // 渐隐效果
        while (timeElapsed < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, timeElapsed / fadeDuration);
            SetAlpha(alpha);  // 设置 alpha 值
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        SetAlpha(0);  // 确保最终为完全透明
    }

    private void SetAlpha(float alpha)
    {
        if (BambooMovementHint != null)
        {
            // 修改alpha值
            Color currentColor = BambooMovementHint.color; // 使用TextMeshProUGUI的color属性
            currentColor.a = alpha; // 修改 alpha 值
            BambooMovementHint.color = currentColor; // 设置新的颜色值
        }
    }
}
