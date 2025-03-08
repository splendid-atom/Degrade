using UnityEngine;
using System.Collections;

public class SwitchDegradeBambooCamera : MonoBehaviour
{
    public static SwitchDegradeBambooCamera instance;
    private Camera degradeBambooCamera; // 引用传统的桥摄像机
    private Camera mainCamera; // 引用传统的主摄像机
    public GameObject DegradeBambooContainer; // 引用枯萎竹子对象
    public bool isDegradeBambooCameraSwitched = false;

    private Vector3 degradeBambooCameraInitialPosition; // 记录桥摄像机的初始位置
    private Quaternion degradeBambooCameraInitialRotation; // 记录桥摄像机的初始旋转

    public float transitionTime = 2f; // 镜头切换时的过渡时间

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 获取场景中的摄像机
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        degradeBambooCamera = GameObject.Find("DegradeBambooCamera").GetComponent<Camera>();

        // 存储桥摄像机的初始位置和旋转
        degradeBambooCameraInitialPosition = degradeBambooCamera.transform.position;
        degradeBambooCameraInitialRotation = degradeBambooCamera.transform.rotation;

        // 游戏开始时使用主摄像机
        SwitchToMainCamera();
    }

    private void Update()
    {
    }

    public void SwitchToDegradeBambooCamera()
    {
        // Debug.Log("任务完成，切换镜头");
        StartCoroutine(SmoothSwitchToDegradeBambooCamera()); // 使用协程平滑切换到桥摄像机
        StartCoroutine(SwitchBackToMainCameraAfterDelay()); // 在所有竹子完成枯萎后切回主摄像机
    }

    // 使用协程平滑切换到桥摄像机
    IEnumerator SmoothSwitchToDegradeBambooCamera()
    {
        //等待1秒
        yield return new WaitForSeconds(1f);
        // 记录当前主摄像机的位置和旋转
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 瞬移桥摄像机到主摄像机的位置
        degradeBambooCamera.transform.position = startPosition;
        degradeBambooCamera.transform.rotation = startRotation;

        // 启用桥摄像机并禁用主摄像机
        degradeBambooCamera.gameObject.SetActive(true); // 激活桥摄像机
        mainCamera.gameObject.SetActive(false); // 禁用主摄像机

        // 在过渡时间内平滑地移动桥摄像机到初始位置
        float timeElapsed = 0f;
        while (timeElapsed < transitionTime)
        {
            degradeBambooCamera.transform.position = Vector3.Lerp(startPosition, degradeBambooCameraInitialPosition, timeElapsed / transitionTime);
            degradeBambooCamera.transform.rotation = Quaternion.Slerp(startRotation, degradeBambooCameraInitialRotation, timeElapsed / transitionTime);
            timeElapsed += Time.deltaTime;
            yield return null; // 等待一帧
        }

        // 确保最终位置和旋转准确
        degradeBambooCamera.transform.position = degradeBambooCameraInitialPosition;
        degradeBambooCamera.transform.rotation = degradeBambooCameraInitialRotation;
        //枯萎竹林对话开启
        PortalAnimation.instance.isTalking = true;
    
    }

    // 协程：在所有竹子完成枯萎后切换回主摄像机
    IEnumerator SwitchBackToMainCameraAfterDelay()
    {
        // 等待直到所有竹子完成枯萎
        while (!DegradeBambooForest.instance.isAllWilted)
        {
            yield return new WaitForSeconds(0.5f); // 每隔0.5秒检查一次
        }

        // 启用主摄像机并禁用桥摄像机
        SwitchToMainCamera();
        isDegradeBambooCameraSwitched = true;
    }

    void SwitchToMainCamera()
    {
        // 禁用桥摄像机并启用主摄像机
        degradeBambooCamera.gameObject.SetActive(false); // 禁用桥摄像机
        mainCamera.gameObject.SetActive(true); // 激活主摄像机
    }
}