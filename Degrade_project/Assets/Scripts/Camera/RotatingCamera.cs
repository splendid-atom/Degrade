using System.Collections;
using UnityEngine;

public class RotatingCamera : MonoBehaviour
{
    public static RotatingCamera Instance;
    public float rotateTime = 0.2f;
    private Transform player;
    private bool isRotating = false;

    // 小地图和大地图摄像头
    public Transform miniMapCamera;
    public int currentRotationIndex = 0; // 旋转索引（-4 到 3）
    
    // 保存当前的旋转角度
    public float currentRotation = 0f;
    // public bool isEnableRotating = true;
    void Awake(){
        Instance = this;
    }
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
        if(VillageNpcController.instance!=null){
            if (VillageNpcController.instance.isTalking)
            {
                return;
            }            
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

    IEnumerator RotateAround(float angle, float time,bool isOnBelt = false)
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
        if(isOnBelt){
            DisableRotation();
        }
    }

    public void SetRotationTo90Degrees()
    {
        if (isRotating) return;

        // 检查当前旋转角度是否已经是 90° 的倍数
        if (Mathf.Approximately(currentRotation, 90f)) return;  // 如果已经是 90° 的倍数，不旋转

        // 计算目标角度（最接近的 90° 倍数）
        float targetRotation = 90f;
        Debug.Log("targetRotation:" + targetRotation);
        float rotationDifference = targetRotation - currentRotation;

        // 确保旋转角度在 -180° 到 180° 之间，防止错误方向旋转
        if (rotationDifference > 180) rotationDifference -= 360;
        if (rotationDifference < -180) rotationDifference += 360;

        // 计算新的 currentRotationIndex
        currentRotationIndex = Mathf.RoundToInt(targetRotation / 45f) % 8;
        if (currentRotationIndex > 3) currentRotationIndex -= 8;

        // 触发旋转协程
        StartCoroutine(RotateAround(rotationDifference, rotateTime,true));
    }
    public void EnableRotation()
    {
        isRotating = false;        
    }
    public void DisableRotation()
    {
        isRotating = true;
    }

}
