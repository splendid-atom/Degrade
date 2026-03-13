using UnityEngine;
using System.Collections;

// 建议重命名为 PortalFocusCamera
public class PortalFocusCamera : MonoBehaviour
{
    // 单例模式，方便 BossController 调用
    public static PortalFocusCamera instance;

    [Header("相机引用")]
    [Tooltip("聚焦传送门时使用的专用摄像机")]
    public Camera portalCamera; // 替换 RageAttackCamera
    [Tooltip("常规游戏主相机")]
    public Camera mainCamera;

    [Header("切换效果")]
    [Tooltip("从主相机切换到传送门相机的平滑过渡时间")]
    public float transitionInTime = 1.0f;
    [Tooltip("传送门相机聚焦传送门的持续时间")]
    public float focusDuration = 3.0f; // 聚焦 3 秒
    [Tooltip("从传送门相机切换回主相机的平滑过渡时间")]
    public float transitionOutTime = 1.0f;

    // 内部状态
    private Vector3 portalCameraInitialPosition; // 记录传送门相机的预设位置
    private Quaternion portalCameraInitialRotation; // 记录传送门相机的预设旋转
    private bool isFocusingPortal = false; // 标记是否正在聚焦
    private Coroutine focusCoroutine = null; // 存储协程引用

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // --- 检查引用 ---
        if (mainCamera == null) { Debug.LogError("Main Camera not assigned!", this); enabled = false; return; }
        if (portalCamera == null) { Debug.LogError("Portal Camera not assigned!", this); enabled = false; return; }

        // 存储传送门相机的初始（预设）位置和旋转
        portalCameraInitialPosition = portalCamera.transform.position;
        portalCameraInitialRotation = portalCamera.transform.rotation;

        // 游戏开始时确保使用主摄像机，并禁用传送门相机
        SwitchToMainCameraImmediately();
    }

    // Update 不再需要，由外部触发
    // void Update() { ... }

    /// <summary>
    /// 【核心方法】由外部（如 BossController）调用，触发聚焦传送门的运镜。
    /// </summary>
    public void FocusOnPortal()
    {
        // 如果正在聚焦，则忽略新的请求
        if (isFocusingPortal)
        {
            Debug.LogWarning("正在聚焦传送门，忽略新的请求。", this);
            return;
        }
        // 停止可能正在运行的上一个协程
        if (focusCoroutine != null)
        {
            StopCoroutine(focusCoroutine);
            SwitchToMainCameraImmediately(); // 确保回到主相机
        }
        // 启动新的聚焦协程
        focusCoroutine = StartCoroutine(FocusPortalSequence());
    }

    /// <summary>
    /// 聚焦传送门的相机序列协程。
    /// </summary>
    private IEnumerator FocusPortalSequence()
    {
        isFocusingPortal = true;
        Debug.Log("[Portal Focus] Sequence Started.");

        // --- 1. 从主相机平滑过渡到传送门相机预设位置 ---
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 目标是传送门相机的预设位置和旋转
        Vector3 targetPosition = portalCameraInitialPosition;
        Quaternion targetRotation = portalCameraInitialRotation;

        // 禁用主相机，启用传送门相机 (先放到起始位置再 Lerp)
        portalCamera.transform.position = startPosition;
        portalCamera.transform.rotation = startRotation;
        mainCamera.gameObject.SetActive(false);
        portalCamera.gameObject.SetActive(true);
        Debug.Log("[Portal Focus] Switched to Portal Camera. Starting Transition In.");

        float timeElapsed = 0f;
        while (timeElapsed < transitionInTime)
        {
            float t = timeElapsed / transitionInTime;
            // t = Mathf.SmoothStep(0.0f, 1.0f, t); // 可选平滑
            portalCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            portalCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        portalCamera.transform.position = targetPosition;
        portalCamera.transform.rotation = targetRotation;
        Debug.Log("[Portal Focus] Transition In Complete. Focusing on Portal.");

        // --- 2. 聚焦传送门持续一段时间 ---
        yield return new WaitForSeconds(focusDuration);
        Debug.Log("[Portal Focus] Focus Duration Ended. Starting Transition Out.");

        // --- 3. 平滑过渡回主相机应该在的位置 ---
        // 简化：直接切换回主相机，让玩家跟随脚本接管
        // 或者，可以尝试平滑移回主相机之前的位置

        // **简化方案：直接切换**
        SwitchToMainCameraImmediately();
        Debug.Log("[Portal Focus] Switched back to Main Camera immediately.");

        /* // **可选：平滑过渡回主相机之前的位置**
        Vector3 mainCamTargetPos = startPosition; // 回到转换开始时的位置
        Quaternion mainCamTargetRot = startRotation; // 回到转换开始时的旋转

        Vector3 currentPortalCamPos = portalCamera.transform.position;
        Quaternion currentPortalCamRot = portalCamera.transform.rotation;

        mainCamera.transform.position = currentPortalCamPos;
        mainCamera.transform.rotation = currentPortalCamRot;
        portalCamera.gameObject.SetActive(false);
        mainCamera.gameObject.SetActive(true);

        timeElapsed = 0f;
        while (timeElapsed < transitionOutTime)
        {
             float t = timeElapsed / transitionOutTime;
             // t = Mathf.SmoothStep(0.0f, 1.0f, t);
             mainCamera.transform.position = Vector3.Lerp(currentPortalCamPos, mainCamTargetPos, t);
             mainCamera.transform.rotation = Quaternion.Slerp(currentPortalCamRot, mainCamTargetRot, t);
             timeElapsed += Time.deltaTime;
             yield return null;
        }
         mainCamera.transform.position = mainCamTargetPos;
         mainCamera.transform.rotation = mainCamTargetRot;
        Debug.Log("[Portal Focus] Transition Out Complete.");
        */

        // --- 4. 标记结束 ---
        isFocusingPortal = false;
        focusCoroutine = null;
        Debug.Log("[Portal Focus] Sequence Finished.");
        // 确保玩家跟随脚本是启用的 (如果之前被禁用了)
        // if (RotatingCamera.instance != null && !RotatingCamera.instance.enabled)
        // {
        //     RotatingCamera.instance.enabled = true;
        // }
    }

    // 立即切换到主相机
    private void SwitchToMainCameraImmediately()
    {
        if (portalCamera != null) portalCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        isFocusingPortal = false; // 确保重置状态
    }
}