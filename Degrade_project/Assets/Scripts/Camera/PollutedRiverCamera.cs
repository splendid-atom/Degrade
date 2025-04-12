using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PollutedRiverCamera : MonoBehaviour
{
    public static PollutedRiverCamera instance;
    private Camera pollutedRiverCamera; 
    private Camera mainCamera; // 引用传统的主摄像机
    // public GameObject riverPortal; // 引用河流传送门
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
        GameObject pollutedRiverCamObj = GameObject.Find("PollutedRiverCamera");
        if (pollutedRiverCamObj != null)
        {
            pollutedRiverCamera = pollutedRiverCamObj.GetComponent<Camera>();
        }

        GameObject mainCamObj = GameObject.Find("MainCamera");
        if (mainCamObj != null)
        {
            mainCamera = mainCamObj.GetComponent<Camera>();
        }

        if (pollutedRiverCamera != null)
        {
            // 存储河流摄像机的初始位置和旋转
            riverCameraInitialPosition = pollutedRiverCamera.transform.position;
            riverCameraInitialRotation = pollutedRiverCamera.transform.rotation;
        }

        // 游戏开始时使用主摄像机
        SwitchToMainCamera();
    }

    void Update()
    {
        if (isSwitchingCamera == false && 
            RiverAnimation.instance != null && 
            RiverAnimation.instance.isPlayerInTrigger != null && 
            RiverAnimation.instance.isPolluted != null && 
            isRiverCameraSwitched != null)
        {
            if (!isSwitchingCamera && 
                RiverAnimation.instance.isPlayerInTrigger && 
                RiverAnimation.instance.isPolluted && 
                !isRiverCameraSwitched)
            {
                isSwitchingCamera = true;
                StartCoroutine(SmoothSwitchToRiverCamera());
            }
        }
    }

    IEnumerator SmoothSwitchToRiverCamera()
    {
        if (mainCamera != null && pollutedRiverCamera != null)
        {
            Vector3 startPosition = mainCamera.transform.position;
            Quaternion startRotation = mainCamera.transform.rotation;

            pollutedRiverCamera.transform.position = startPosition;
            pollutedRiverCamera.transform.rotation = startRotation;

            pollutedRiverCamera.gameObject.SetActive(true);
            mainCamera.gameObject.SetActive(false);

            float timeElapsed = 0f;
            while (timeElapsed < transitionTime)
            {
                pollutedRiverCamera.transform.position = Vector3.Lerp(startPosition, riverCameraInitialPosition, timeElapsed / transitionTime);
                pollutedRiverCamera.transform.rotation = Quaternion.Slerp(startRotation, riverCameraInitialRotation, timeElapsed / transitionTime);
                timeElapsed += Time.deltaTime;
                yield return null;
            }

            pollutedRiverCamera.transform.position = riverCameraInitialPosition;
            pollutedRiverCamera.transform.rotation = riverCameraInitialRotation;
        }

        if (isSwitchingCamera != null && 
            RiverPortalAnimation.instance != null && 
            RiverPortalAnimation.instance.riverPortal != null && 
            RiverAnimation.instance != null)
        {
            if (isSwitchingCamera)
            {
                RiverPortalAnimation.instance.riverPortal.SetActive(true);
                RiverAnimation.instance.StartShrink();
                if (RiverPortalAnimation.instance != null)
                {
                    RiverPortalAnimation.instance.OnDialogueStart();
                }
            }
        }

        if (RiverPortalAnimation.instance != null && 
            RiverPortalAnimation.instance.isTalking != null)
        {
            while (RiverPortalAnimation.instance.isTalking)
            {
                yield return null;
            }
        }

        StartCoroutine(SwitchBackToMainCameraAfterDelay(1f));
    }

    IEnumerator SwitchBackToMainCameraAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        SwitchToMainCamera();
        isRiverCameraSwitched = true;
    }

    void SwitchToMainCamera()
    {
        if (pollutedRiverCamera != null && mainCamera != null)
        {
            pollutedRiverCamera.gameObject.SetActive(false);
            mainCamera.gameObject.SetActive(true);
        }
    }
}