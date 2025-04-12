using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageTruckAnimation : MonoBehaviour
{
    // 公共的轮子列表，包含4个轮子的Transform
    public List<Transform> wheels = new List<Transform>(4);
    
    // 控制倒车速度
    public float reverseSpeed = 5f;
    // 控制轮子旋转速度
    public float wheelRotationSpeed = 50f;
    public float initialYcoord = 0f;
    public float targetYcoord = 0.5f;
    public bool isStartMoving = false;
    public bool isArrived = false;
    public GarbageTruckTrigger GarbageTruckTrigger;
    void Start()
    {
        // 检查是否正确设置了4个轮子
        if (wheels.Count != 4)
        {
            Debug.LogWarning("请在Inspector中为垃圾车设置4个轮子!");
        }
    }

    void Update()
    {
        // if(Input.GetKey(KeyCode.DownArrow)&& !isStartMoving){
        //     isStartMoving = true;
        // }
        if(!isStartMoving&&GarbageTruckTrigger.IsPlayerInTrigger()){
            isStartMoving = true;
        }
        Debug.Log(transform.localPosition.y+" "+targetYcoord);
        if (isStartMoving)
        {
            if(transform.localPosition.y > targetYcoord&& !isArrived){
                // 向后移动车辆（可选，已注释）
                transform.Translate(Vector3.back * reverseSpeed * Time.deltaTime);

                // 旋转轮子（只绕X轴）
                foreach (Transform wheel in wheels)
                {
                    if (wheel != null)
                    {
                        // 只增加local X轴旋转
                        wheel.Rotate(Vector3.right * wheelRotationSpeed * Time.deltaTime, Space.Self);
                    }
                }                
            }
            else{
                isArrived = true;
            }
        }
    }
}