using UnityEngine;
using System.Collections;

public class SwitchTrashEnemyCamera : MonoBehaviour
{
    public bool isTrashEnemyCameraSwitched = false;

    private Vector3 TrashEnemyCameraInitialPosition; // 记录桥摄像机的初始位置
    private Quaternion TrashEnemyCameraInitialRotation; // 记录桥摄像机的初始旋转

    public float transitionTime = 2f; // 镜头切换时的过渡时间
    public Camera TrashEnemyCamera;
    public Camera mainCamera;
    public float delayBeforeSwitch = 1f;
    public float delayBeforeSwitchBack = 9f;

    private void Start()
    {
        TrashEnemyCameraInitialPosition = TrashEnemyCamera.transform.position;
        TrashEnemyCameraInitialRotation = TrashEnemyCamera.transform.rotation;
        // 游戏开始时使用主摄像机
        SwitchToMainCamera();
    }

    public void SwitchToTrashEnemyCamera()
    {
        StartCoroutine(SmoothSwitchToTrashEnemyCamera()); // 使用协程平滑切换到桥摄像机
        StartCoroutine(SwitchBackToMainCameraAfterDelay()); // 在所有竹子完成枯萎后切回主摄像机
    }

    // 使用协程平滑切换到桥摄像机
    IEnumerator SmoothSwitchToTrashEnemyCamera()
    {
        //等待1秒
        yield return new WaitForSeconds(delayBeforeSwitch);
        // 记录当前主摄像机的位置和旋转
        Vector3 startPosition = mainCamera.transform.position;
        Quaternion startRotation = mainCamera.transform.rotation;

        // 瞬移桥摄像机到主摄像机的位置
        TrashEnemyCamera.transform.position = startPosition;
        TrashEnemyCamera.transform.rotation = startRotation;

        // 启用桥摄像机并禁用主摄像机
        TrashEnemyCamera.gameObject.SetActive(true); // 激活桥摄像机
        mainCamera.gameObject.SetActive(false); // 禁用主摄像机

        // 在过渡时间内平滑地移动桥摄像机到初始位置
        float timeElapsed = 0f;
        while (timeElapsed < transitionTime)
        {
            TrashEnemyCamera.transform.position = Vector3.Lerp(startPosition, TrashEnemyCameraInitialPosition, timeElapsed / transitionTime);
            TrashEnemyCamera.transform.rotation = Quaternion.Slerp(startRotation, TrashEnemyCameraInitialRotation, timeElapsed / transitionTime);
            timeElapsed += Time.deltaTime;
            yield return null; // 等待一帧
        }
        // 确保最终位置和旋转准确
        TrashEnemyCamera.transform.position = TrashEnemyCameraInitialPosition;
        TrashEnemyCamera.transform.rotation = TrashEnemyCameraInitialRotation;
    }

    // 协程：在所有竹子完成枯萎后切换回主摄像机
    IEnumerator SwitchBackToMainCameraAfterDelay(){
        yield return new WaitForSeconds(delayBeforeSwitchBack);
        // 启用主摄像机并禁用桥摄像机
        SwitchToMainCamera();
        isTrashEnemyCameraSwitched = true;
    }
    void SwitchToMainCamera()
    {
        TrashEnemyCamera.gameObject.SetActive(false); // 禁用桥摄像机
        mainCamera.gameObject.SetActive(true); // 激活主摄像机
    }
}