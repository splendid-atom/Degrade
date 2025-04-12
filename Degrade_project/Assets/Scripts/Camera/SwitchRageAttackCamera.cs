using UnityEngine;
using System.Collections;

public class SwitchRageAttackCamera : MonoBehaviour
{
    public static SwitchRageAttackCamera instance;
    // private Camera RageAttackCamera; // 引用传统的桥摄像机
    // private Camera mainCamera; // 引用传统的主摄像机
    public bool isRageAttackCameraSwitched = false;

    private Vector3 RageAttackCameraInitialPosition; // 记录桥摄像机的初始位置
    private Quaternion RageAttackCameraInitialRotation; // 记录桥摄像机的初始旋转

    public float transitionTime = 2f; // 镜头切换时的过渡时间
    public Camera RageAttackCamera;
    public Camera mainCamera;
    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // 获取场景中的摄像机
        // mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        // RageAttackCamera = GameObject.Find("RageAttackCamera").GetComponent<Camera>();

        // 存储桥摄像机的初始位置和旋转
        RageAttackCameraInitialPosition = RageAttackCamera.transform.position;
        RageAttackCameraInitialRotation = RageAttackCamera.transform.rotation;

        // 游戏开始时使用主摄像机
        SwitchToMainCamera();
    }

    public void SwitchToRageAttackCamera()
    {
        // Debug.Log("任务完成，切换镜头");
        StartCoroutine(SmoothSwitchToRageAttackCamera()); // 使用协程平滑切换到桥摄像机
        StartCoroutine(SwitchBackToMainCameraAfterDelay()); // 在所有竹子完成枯萎后切回主摄像机
    }

    // 使用协程平滑切换到桥摄像机
    IEnumerator SmoothSwitchToRageAttackCamera()
    {
        //等待1秒
        yield return new WaitForSeconds(3f);
        // 记录当前主摄像机的位置和旋转
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 瞬移桥摄像机到主摄像机的位置
        RageAttackCamera.transform.position = startPosition;
        RageAttackCamera.transform.rotation = startRotation;

        // 启用桥摄像机并禁用主摄像机
        RageAttackCamera.gameObject.SetActive(true); // 激活桥摄像机
        mainCamera.gameObject.SetActive(false); // 禁用主摄像机

        // 在过渡时间内平滑地移动桥摄像机到初始位置
        float timeElapsed = 0f;
        while (timeElapsed < transitionTime)
        {
            RageAttackCamera.transform.position = Vector3.Lerp(startPosition, RageAttackCameraInitialPosition, timeElapsed / transitionTime);
            RageAttackCamera.transform.rotation = Quaternion.Slerp(startRotation, RageAttackCameraInitialRotation, timeElapsed / transitionTime);
            timeElapsed += Time.deltaTime;
            yield return null; // 等待一帧
        }

        // 确保最终位置和旋转准确
        RageAttackCamera.transform.position = RageAttackCameraInitialPosition;
        RageAttackCamera.transform.rotation = RageAttackCameraInitialRotation;
        //枯萎竹林对话开启
        // PortalAnimation.instance.isTalking = true;
    
    }

    // 协程：在所有竹子完成枯萎后切换回主摄像机
    IEnumerator SwitchBackToMainCameraAfterDelay(){

        yield return new WaitForSeconds(9f);
        // 启用主摄像机并禁用桥摄像机
        SwitchToMainCamera();
        isRageAttackCameraSwitched = true;
    }

    void SwitchToMainCamera()
    {
        // 禁用桥摄像机并启用主摄像机
        RageAttackCamera.gameObject.SetActive(false); // 禁用桥摄像机
        mainCamera.gameObject.SetActive(true); // 激活主摄像机
    }
}