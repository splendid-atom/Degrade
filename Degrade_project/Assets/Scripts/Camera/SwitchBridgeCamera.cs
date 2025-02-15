using UnityEngine;
using System.Collections; // 引入协程所需的命名空间

public class SwitchBridgeCamera : MonoBehaviour
{
    public static SwitchBridgeCamera instance;
    private Camera bridgeCamera; // 引用传统的桥摄像机
    private Camera mainCamera; // 引用传统的主摄像机
    public GameObject bridgeObject; // 引用桥对象
    public bool isBridgeCameraSwitched = false;

    private Vector3 bridgeCameraInitialPosition; // 记录桥摄像机的初始位置
    private Quaternion bridgeCameraInitialRotation; // 记录桥摄像机的初始旋转

    public float transitionTime = 2f; // 镜头切换时的过渡时间

    private void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        // 获取场景中的摄像机
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        bridgeCamera = GameObject.Find("BridgeCamera").GetComponent<Camera>();

        // 存储桥摄像机的初始位置和旋转
        bridgeCameraInitialPosition = bridgeCamera.transform.position;
        bridgeCameraInitialRotation = bridgeCamera.transform.rotation;

        // 游戏开始时使用主摄像机
        SwitchToMainCamera();
    }

    private void Update()
    {
        // 输出任务完成状态进行调试
        Debug.Log("任务完成状态: " + QuestUIManager.QuestManager.quests[0].isCompleted);

        // 检查任务是否完成
        if (QuestUIManager.QuestManager.quests[0].isCompleted && !isBridgeCameraSwitched)
        {
            Debug.Log("任务完成，切换镜头");
            StartCoroutine(SmoothSwitchToBridgeCamera()); // 使用协程平滑切换到桥摄像机
            StartCoroutine(SwitchBackToMainCameraAfterDelay(5f)); // 5秒后切回主摄像机
            
        }
    }

    // 使用协程平滑切换到桥摄像机
    IEnumerator SmoothSwitchToBridgeCamera()
    {
        // 记录当前主摄像机的位置和旋转
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 瞬移桥摄像机到主摄像机的位置
        bridgeCamera.transform.position = startPosition;
        bridgeCamera.transform.rotation = startRotation;

        // 启用桥摄像机并禁用主摄像机
        bridgeCamera.gameObject.SetActive(true); // 激活桥摄像机
        mainCamera.gameObject.SetActive(false); // 禁用主摄像机

        // 在过渡时间内平滑地移动桥摄像机到初始位置
        float timeElapsed = 0f;
        while (timeElapsed < transitionTime)
        {
            bridgeCamera.transform.position = Vector3.Lerp(startPosition, bridgeCameraInitialPosition, timeElapsed / transitionTime);
            bridgeCamera.transform.rotation = Quaternion.Slerp(startRotation, bridgeCameraInitialRotation, timeElapsed / transitionTime);
            timeElapsed += Time.deltaTime;
            yield return null; // 等待一帧
        }

        // 确保最终位置和旋转准确
        bridgeCamera.transform.position = bridgeCameraInitialPosition;
        bridgeCamera.transform.rotation = bridgeCameraInitialRotation;
    }

    // 协程：5秒后切换回主摄像机
    IEnumerator SwitchBackToMainCameraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 等待5秒

        // 启用主摄像机并禁用桥摄像机
        SwitchToMainCamera();
        isBridgeCameraSwitched = true;
    }

    void SwitchToMainCamera()
    {
        // 禁用桥摄像机并启用主摄像机
        bridgeCamera.gameObject.SetActive(false); // 禁用桥摄像机
        mainCamera.gameObject.SetActive(true); // 激活主摄像机
    }
}
