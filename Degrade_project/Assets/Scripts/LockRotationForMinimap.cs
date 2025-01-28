using UnityEngine;

public class LockRotationForMinimap : MonoBehaviour
{
    private Camera minimapCamera;  // 引用小地图摄像机
    private Quaternion initialRotation;

    void Start()
    {
        // 获取小地图摄像机（假设它只有一个并且是唯一的）
        minimapCamera = GameObject.Find("CameraForMinimap").GetComponent<Camera>();
        
        initialRotation = transform.rotation;  // 保存初始旋转
    }

    void Update()
    {
        // 检查物体的层级是否为 Default
        if (gameObject.layer == LayerMask.NameToLayer("Default"))
        {
            // 只有当摄像机是小地图摄像机时，才锁定物体的旋转
            if (minimapCamera != null && Camera.current == minimapCamera)
            {
                transform.rotation = initialRotation;  // 重置物体旋转
            }
        }
    }
}
