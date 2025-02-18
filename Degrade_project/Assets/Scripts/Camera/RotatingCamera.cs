using System.Collections;
using UnityEngine;

public class RotatingCamera : MonoBehaviour
{
    public float rotateTime = 0.2f;
    private Transform player;
    private bool isRotating = false;

    // 小地图和大地图摄像头
    public Transform miniMapCamera;
    public int currentRotationIndex = 0; // 旋转索引（-4 到 3）
    
    // 保存当前的旋转角度
    public float currentRotation = 0f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        transform.position = player.position;
        Rotate();
    }

    void Rotate()
    {
        if(VillageNpcController.instance.isTalking){
            // Debug.Log("Player is talking");
            return;
        }
        if (Input.GetKey(KeyCode.Q) && !isRotating)
        {
            currentRotationIndex++; // 逆时针旋转
            if (currentRotationIndex > 3) currentRotationIndex = -4;
            StartCoroutine(RotateAround(45, rotateTime));
        }
        if (Input.GetKey(KeyCode.E) && !isRotating)
        {
            currentRotationIndex--; // 顺时针旋转
            if (currentRotationIndex < -4) currentRotationIndex = 3;
            StartCoroutine(RotateAround(-45, rotateTime));
        }
    }

    IEnumerator RotateAround(float angle, float time)
    {
        float steps = 60 * time;
        float anglePerStep = angle / steps;
        isRotating = true;

        for (int i = 0; i < steps; i++)
        {
            // 主摄像头旋转
            transform.Rotate(new Vector3(0, 0, anglePerStep));

            // 更新当前旋转角度
            currentRotation += anglePerStep;
            currentRotation %= 360;

            // 同步小地图摄像头旋转
            if (miniMapCamera != null)
            {
                miniMapCamera.Rotate(new Vector3(0, 0, anglePerStep));
            }

            yield return new WaitForFixedUpdate();
        }
        isRotating = false;
    }
}
