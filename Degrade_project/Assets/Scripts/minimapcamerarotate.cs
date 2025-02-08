using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ImprovedCameraRotationController : MonoBehaviour
{
    public Transform player; // 人物的 Transform 组件
    public Camera miniMapCamera; // 副相机（小地图相机）

    public float rotateTime = 0.2f; // 旋转所需时间
    public float rotationAngle = 45f; // 每次旋转的角度
    private bool isRotating = false; // 旋转状态标记

    private void Start()
    {
        // 初始化副相机位置，使其位于人物正上方
        miniMapCamera.transform.position = new Vector3(player.position.x, player.position.y + 10f, player.position.z);
        miniMapCamera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    private void Update()
    {
        miniMapCamera.transform.position = new Vector3(player.position.x, miniMapCamera.transform.position.y, player.position.z);

        // 处理旋转输入
        HandleRotationInput();
    }

    private void HandleRotationInput()
    {
        if (isRotating) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(RotateCameras(rotationAngle));
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(RotateCameras(-rotationAngle));
        }
    }

    private System.Collections.IEnumerator RotateCameras(float angle)
    {
        isRotating = true;
        float elapsedTime = 0f;
        Quaternion startMiniMapRotation = miniMapCamera.transform.rotation;
        Quaternion targetMiniMapRotation = startMiniMapRotation * Quaternion.Euler(0f, angle, 0f);

        while (elapsedTime < rotateTime)
        {
            miniMapCamera.transform.rotation = Quaternion.Slerp(startMiniMapRotation, targetMiniMapRotation, elapsedTime / rotateTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        miniMapCamera.transform.rotation = targetMiniMapRotation;
        isRotating = false;
    }
}
