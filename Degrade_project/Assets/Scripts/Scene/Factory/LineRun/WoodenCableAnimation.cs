using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodenCableAnimation : MonoBehaviour
{
    public float rotationSpeed = 5.0f;
    private bool isRotating = true;
    // Update is called once per frame
    void Update()
    {
        if(isRotating){
            // 获取当前的旋转
            Quaternion currentRotation = transform.rotation;

            // 创建一个新的旋转，绕Y轴旋转一定的角度
            Quaternion newRotation = Quaternion.Euler(currentRotation.eulerAngles.x, currentRotation.eulerAngles.y + rotationSpeed, currentRotation.eulerAngles.z);

            // 设置物体的新旋转
            transform.rotation = newRotation;            
        }

    }
    public void ResetRotation()
    {
        isRotating = false;
    }
}
