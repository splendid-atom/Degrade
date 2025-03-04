using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollutedRiverCamera : MonoBehaviour
{
    public static PollutedRiverCamera instance;
    private Camera pollutedRiverCamera; 

    private Camera mainCamera; // 引用传统的主摄像机
    public GameObject riverPortal; // 引用河流传送门
    public bool isRiverCameraSwitched = false;
    private Vector3 riverCameraInitialPosition; // 记录河流摄像机的初始位置
    private Quaternion riverCameraInitialRotation; // 记录河流摄像机的初始旋转
    public bool isSwitchingCamera = false;
    public float transitionTime = 2f; // 镜头切换时的过渡时间
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        pollutedRiverCamera = GameObject.Find("PollutedRiverCamera").GetComponent<Camera>();
        mainCamera = GameObject.Find("MainCamera").GetComponent<Camera>();
        // 存储河流摄像机的初始位置和旋转
        riverCameraInitialPosition = pollutedRiverCamera.transform.position;
        riverCameraInitialRotation = pollutedRiverCamera.transform.rotation;
        // 游戏开始时使用主摄像机
        SwitchToMainCamera();
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("Camera Position: " + pollutedRiverCamera.transform.position);
        // 检查任务是否完成
        if (!isSwitchingCamera&&RiverAnimation.instance.isPlayerInTrigger&&RiverAnimation.instance.isPolluted&&!isRiverCameraSwitched)
        {
            isSwitchingCamera = true;
            StartCoroutine(SmoothSwitchToRiverCamera()); // 使用协程平滑切换到桥摄像机
            StartCoroutine(SwitchBackToMainCameraAfterDelay(5f)); // 5秒后切回主摄像机
        }        

    }


    // 使用协程平滑切换到桥摄像机
    IEnumerator SmoothSwitchToRiverCamera()
    {
        // 记录当前主摄像机的位置和旋转
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 瞬移桥摄像机到主摄像机的位置
        pollutedRiverCamera.transform.position = startPosition;
        pollutedRiverCamera.transform.rotation = startRotation;

        // 启用桥摄像机并禁用主摄像机
        pollutedRiverCamera.gameObject.SetActive(true); // 激活桥摄像机
        mainCamera.gameObject.SetActive(false); // 禁用主摄像机

        // 在过渡时间内平滑地移动桥摄像机到初始位置
        float timeElapsed = 0f;
        while (timeElapsed < transitionTime)
        {
            pollutedRiverCamera.transform.position = Vector3.Lerp(startPosition, riverCameraInitialPosition, timeElapsed / transitionTime);
            pollutedRiverCamera.transform.rotation = Quaternion.Slerp(startRotation, riverCameraInitialRotation, timeElapsed / transitionTime);
            timeElapsed += Time.deltaTime;
            yield return null; // 等待一帧
        }

        // 确保最终位置和旋转准确
        pollutedRiverCamera.transform.position = riverCameraInitialPosition;
        pollutedRiverCamera.transform.rotation = riverCameraInitialRotation;
        
        //开启污染河流的传送门
        if(isSwitchingCamera){
            RiverPortalAnimation.instance.riverPortal.SetActive(true);
            RiverAnimation.instance.StartShrink();
            // RiverAnimation.instance.SetRiverMaterialPolluted();
        }
    }

    // 协程：5秒后切换回主摄像机
    IEnumerator SwitchBackToMainCameraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 等待5秒

        // 启用主摄像机并禁用桥摄像机
        SwitchToMainCamera();
        isRiverCameraSwitched = true;
    }

    void SwitchToMainCamera()
    {
        // 禁用桥摄像机并启用主摄像机
        pollutedRiverCamera.gameObject.SetActive(false); // 禁用桥摄像机
        mainCamera.gameObject.SetActive(true); // 激活主摄像机
    }
}
