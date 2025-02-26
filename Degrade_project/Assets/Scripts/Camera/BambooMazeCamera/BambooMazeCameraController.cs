using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BambooMazeCameraController : MonoBehaviour
{
    public static BambooMazeCameraController instance;
    private Transform player;
    private Camera mainCamera;  // 引用摄像机
    private Transform cameraContainer; // 摄像机的容器
    private Vector3 initialCameraRotation;
    private Vector3 initialCameraPosition;
    private Vector3 positionForMaze;
    public float xRotationForMaze = -69.76f;
    public bool isInMaze = false;
    private bool isTransitioning = false; // 标记是否正在渐变
    public float duration = 0.5f; // 渐变时长
    private GameObject BambooMazeContainer;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {

        BambooMazeContainer = GameObject.Find("BambooMazeContainer");
        // 如果没有找到主摄像机，输出调试信息
        mainCamera = GameObject.FindWithTag("MainCamera")?.GetComponent<Camera>();
        // 获取摄像机容器
        cameraContainer = mainCamera.transform.parent;
        if (mainCamera == null)
        {
            Debug.LogError("主摄像机未找到！");
            return;
        }

        // 获取摄像机容器内的初始位置和旋转
        initialCameraPosition = mainCamera.transform.localPosition;
        initialCameraRotation = mainCamera.transform.localRotation.eulerAngles;

        // 获取玩家和迷宫的引用
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("玩家对象未找到！");
        }


        // 设置迷宫的位置
        positionForMaze = new Vector3(-0.024f, -7.28f, -3.2f); // 迷宫的位置
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera == null || player == null || cameraContainer == null)
        {
            return; // 如果任何一个关键对象未找到，直接跳出
        }


        if (isInMaze && !isTransitioning)
        {
            StartCoroutine(SmoothTransitionToMaze()); // 开始渐变到迷宫位置
        }
        else if (!isInMaze && !isTransitioning)
        {
            StartCoroutine(SmoothTransitionToInitial()); // 开始渐变到初始位置
        }

        // 调试信息
        if (isInMaze)
        {
            // Debug.Log("摄像机已进入迷宫，位置：" + mainCamera.transform.localPosition + " 旋转：" + mainCamera.transform.localRotation.eulerAngles);
        }
        else
        {
            // Debug.Log("摄像机未进入迷宫，位置：" + mainCamera.transform.localPosition + " 旋转：" + mainCamera.transform.localRotation.eulerAngles);
        }
    }
    // 渐变到迷宫位置的协程
    private IEnumerator SmoothTransitionToMaze()
    {
        isTransitioning = true;
        float elapsedTime = 0f;
        
        Vector3 startPosition = mainCamera.transform.localPosition;
        Quaternion startRotation = mainCamera.transform.localRotation;

        while (elapsedTime < duration)
        {
            mainCamera.transform.localPosition = Vector3.Lerp(startPosition, positionForMaze, elapsedTime / duration);
            mainCamera.transform.localRotation = Quaternion.Slerp(startRotation, Quaternion.Euler(xRotationForMaze, initialCameraRotation.y, initialCameraRotation.z), elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 确保最后位置和旋转准确
        mainCamera.transform.localPosition = positionForMaze;
        mainCamera.transform.localRotation = Quaternion.Euler(xRotationForMaze, initialCameraRotation.y, initialCameraRotation.z);
        isTransitioning = false;
    }

    // 渐变到初始位置的协程
    private IEnumerator SmoothTransitionToInitial()
    {
        isTransitioning = true;
        float elapsedTime = 0f;

        Vector3 startPosition = mainCamera.transform.localPosition;
        Quaternion startRotation = mainCamera.transform.localRotation;

        while (elapsedTime < duration)
        {
            mainCamera.transform.localPosition = Vector3.Lerp(startPosition, initialCameraPosition, elapsedTime / duration);
            mainCamera.transform.localRotation = Quaternion.Slerp(startRotation, Quaternion.Euler(initialCameraRotation), elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null; // 等待下一帧
        }

        // 确保最后位置和旋转准确
        mainCamera.transform.localPosition = initialCameraPosition;
        mainCamera.transform.localRotation = Quaternion.Euler(initialCameraRotation);
        isTransitioning = false;
    }
}
