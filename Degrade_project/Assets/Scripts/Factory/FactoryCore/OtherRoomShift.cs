using UnityEngine;
using System.Collections;

public class SwitchRageAttackCamera_new : MonoBehaviour
{
    public static SwitchRageAttackCamera_new instance;
    public bool isRageAttackCameraSwitched = false; // 标记当前是否处于特殊相机状态

    [Header("相机引用")]
    [Tooltip("特殊运镜使用的相机 (例如 PhaseShiftCamera)")]
    public Camera specialEffectCamera; // 重命名 RageAttackCamera 为更通用的名字
    [Tooltip("常规游戏主相机")]
    public Camera mainCamera;

    [Header("切换效果")]
    [Tooltip("从主相机切换到特殊相机的平滑过渡时间")]
    public float transitionInTime = 1.0f; // 可以调整
    [Tooltip("特殊相机聚焦目标位置的持续时间")]
    public float focusDuration = 2.0f; // 聚焦 Boss 的时间
    [Tooltip("从特殊相机切换回主相机的平滑过渡时间")]
    public float transitionOutTime = 1.0f; // 可以调整

    [Tooltip("特殊相机相对于聚焦目标的【世界坐标】偏移量")]
    public float cameraOffsetFromTargety_offset = 0.0f; // 类似之前的参数
    public float cameraOffsetFromTargetz_offset = 0.0f; // 类似之前的参数
    [SerializeField] private Vector3 cameraOffsetFromTarget ; // 类似之前的参数

    // 内部状态
    private Vector3 specialCameraInitialPosition; // 记录特殊相机的“默认”位置
    private Quaternion specialCameraInitialRotation; // 记录特殊相机的“默认”旋转
    private Coroutine switchCoroutine = null; // 防止重复启动协程

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // --- 检查引用 ---
        if (mainCamera == null) { Debug.LogError("Main Camera not assigned!", this); enabled = false; return; }
        if (specialEffectCamera == null) { Debug.LogError("Special Effect Camera not assigned!", this); enabled = false; return; }

        // 存储特殊相机的初始（或默认）位置和旋转，可能用于非 Boss 转换的其他用途
        specialCameraInitialPosition = specialEffectCamera.transform.position;
        specialCameraInitialRotation = specialEffectCamera.transform.rotation;

        cameraOffsetFromTarget = new Vector3(0, cameraOffsetFromTargety_offset, cameraOffsetFromTargetz_offset);

        // 游戏开始时确保使用主摄像机
        SwitchToMainCameraImmediately();
    }

    // Update 现在只用于测试 (如果需要)
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.Space) && !isRageAttackCameraSwitched)
    //     {
    //         // 测试调用，需要提供一个目标点
    //         TriggerBossPhaseTransition(GameObject.FindGameObjectWithTag("Boss")?.transform.position ?? Vector3.zero);
    //     }
    // }

    /// <summary>
    /// 【核心方法】由外部（如 BossController）调用，触发针对 Boss 阶段转换的相机运镜。
    /// </summary>
    /// <param name="bossFinalPosition">Boss 传送完成后的最终位置。</param>
    /// <param name="playerFinalPosition">玩家传送完成后的最终位置。</param>
    public void TriggerBossPhaseTransition(Vector3 bossFinalPosition, Vector3 playerFinalPosition)
    {
        // 如果上一个切换协程还在运行，先停止它
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
            // 确保主相机是激活的，以防协程在中途停止
            SwitchToMainCameraImmediately();
            Debug.LogWarning("新的相机转换请求中断了上一个转换。", this);
        }
        // 启动新的切换协程
        switchCoroutine = StartCoroutine(BossTransitionSequence(bossFinalPosition, playerFinalPosition));
    }

    /// <summary>
    /// Boss 阶段转换的相机序列协程。
    /// </summary>
    private IEnumerator BossTransitionSequence(Vector3 bossFinalPosition, Vector3 playerFinalPosition)
    {
        isRageAttackCameraSwitched = true; // 标记进入特殊状态
        Debug.Log("[Switch Cam] Boss Transition Sequence Started.");

        // --- 1. 从主相机平滑过渡到聚焦 Boss 的视角 ---
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 计算特殊相机聚焦 Boss 的目标位置和旋转
        Vector3 focusBossPosition = bossFinalPosition + cameraOffsetFromTarget;
        // 保持特殊相机的默认旋转，或者强制看向 Boss
        Quaternion focusBossRotation = specialCameraInitialRotation; // 使用默认旋转
        // 或者: Quaternion focusBossRotation = Quaternion.LookRotation(bossFinalPosition - focusBossPosition);

        // 禁用主相机，启用特殊相机 (但先把它放到起始位置，再 Lerp)
        specialEffectCamera.transform.position = startPosition;
        specialEffectCamera.transform.rotation = startRotation;
        specialEffectCamera.gameObject.SetActive(true);
        mainCamera.gameObject.SetActive(false);
        
        Debug.Log("[Switch Cam] Switched to Special Camera. Starting Transition In.");

        float timeElapsed = 0f;
        while (timeElapsed < transitionInTime)
        {
            float t = timeElapsed / transitionInTime;
             t = Mathf.SmoothStep(0.0f, 1.0f, t);
            specialEffectCamera.transform.position = Vector3.Lerp(startPosition, focusBossPosition, t);
            specialEffectCamera.transform.rotation = Quaternion.Slerp(startRotation, focusBossRotation, t);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        specialEffectCamera.transform.position = focusBossPosition + cameraOffsetFromTarget;//z轴偏移
        specialEffectCamera.transform.rotation = focusBossRotation;
        Debug.Log("[Switch Cam] Transition In Complete. Focusing on Boss.");

        // --- 2. 聚焦 Boss 持续一段时间 ---
        yield return new WaitForSeconds(focusDuration);
        Debug.Log("[Switch Cam] Focus Duration Ended. Starting Transition Out.");

        // --- 3. 平滑过渡回主相机应该在的位置 ---
        // 此时玩家和 Boss 都在新位置了。
        // 主相机应该回到哪里？理论上是【如果玩家跟随脚本立刻启用，相机应该在的位置】
        // 这个位置很难精确计算。简化：我们直接切回，让跟随脚本处理。
        // 或者，我们可以计算出主相机跟随【玩家新位置】的目标点，然后 Lerp 过去。

        // **简化方案：直接切换**
        /*
        SwitchToMainCameraImmediately();
        */

        // **尝试平滑过渡回“理论上的”主相机位置**
        // 计算主相机跟随玩家新位置时的“理想”位置/旋转 (需要 RotatingCamera 的逻辑信息，很难准确模拟)
        // 简化：我们假设主相机应该回到它在转换开始时的状态（相对于世界或玩家）
        // 这会导致切换回主相机时有一个跳跃，由 RotatingCamera 校正
        Vector3 mainCamTargetPos = startPosition; // 回到转换开始时的位置
        Quaternion mainCamTargetRot = startRotation; // 回到转换开始时的旋转

        // 当前特殊相机的位置和旋转
        Vector3 currentSpecCamPos = specialEffectCamera.transform.position;
        Quaternion currentSpecCamRot = specialEffectCamera.transform.rotation;

        // 把主相机放到特殊相机的当前位置，然后 Lerp 回目标位置
        mainCamera.transform.position = currentSpecCamPos;
        mainCamera.transform.rotation = currentSpecCamRot;
        specialEffectCamera.gameObject.SetActive(false); // 禁用特殊相机
        mainCamera.gameObject.SetActive(true); // 启用主相机

        timeElapsed = 0f;
        while (timeElapsed < transitionOutTime)
        {
             float t = timeElapsed / transitionOutTime;
             // t = Mathf.SmoothStep(0.0f, 1.0f, t);
             mainCamera.transform.position = Vector3.Lerp(currentSpecCamPos, mainCamTargetPos, t);
             mainCamera.transform.rotation = Quaternion.Slerp(currentSpecCamRot, mainCamTargetRot, t);
             timeElapsed += Time.deltaTime;
             yield return null;
        }
         mainCamera.transform.position = mainCamTargetPos;
         mainCamera.transform.rotation = mainCamTargetRot;
        Debug.Log("[Switch Cam] Transition Out Complete.");
        PlayerController.Instance.isInvincible = false;
        PlayerController.Instance.enabled = true; 

        // --- 4. 标记结束 ---
        isRageAttackCameraSwitched = false; // 重置标记
        switchCoroutine = null; // 清除协程引用
        Debug.Log("[Switch Cam] Boss Transition Sequence Finished.");
        
        // 注意：此时还没有重新启用 RotatingCamera，这应该由 BossController 在确认转换完成后做（如果需要的话）
        // 或者，如果你希望这个脚本完全控制，可以在这里重新启用它：
        // if (RotatingCamera.instance != null) RotatingCamera.instance.enabled = true; // 需要 RotatingCamera 也是单例
    }

    // 立即切换到主相机 (用于 Start 或中断时)
    private void SwitchToMainCameraImmediately()
    {
        if (specialEffectCamera != null) specialEffectCamera.gameObject.SetActive(false);
        if (mainCamera != null) mainCamera.gameObject.SetActive(true);
        isRageAttackCameraSwitched = false;
    }

    // 废弃旧方法 (或根据需要保留用于其他功能)
    // public void SwitchToRageAttackCamera() { ... }
    // IEnumerator SmoothSwitchToRageAttackCamera() { ... }
    // IEnumerator SwitchBackToMainCameraAfterDelay() { ... }
    // void SwitchToMainCamera() { ... }
}