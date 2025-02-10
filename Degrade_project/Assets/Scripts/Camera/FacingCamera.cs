using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    private List<Transform> childs = new List<Transform>();  // 使用List动态管理子物体

    void Start()
    {
        // 初始化时检查当前所有子物体
        UpdateChilds();
    }

    // Update is called once per frame
    void Update()
    {
        // 每帧更新子物体列表，确保新添加的子物体被包含
        UpdateChilds();
        RotateObjects();
    }

    // 更新子物体列表
    void UpdateChilds()
    {
        childs.Clear();  // 清空当前子物体列表

        // 遍历所有当前子物体
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            // 将每个子物体添加到列表中
            if (!childs.Contains(child))  // 防止重复添加
            {
                childs.Add(child);
                // 检查对象是否有"FacingOnce"标签
                if (child.CompareTag("FacingOnce"))
                {
                    // 仅在第一次运行时旋转一次，之后不再旋转
                    child.rotation = Camera.main.transform.rotation;
                }
            }
        }
    }

    // 使子物体始终面向摄像头
    void RotateObjects()
    {
        foreach (Transform child in childs)
        {
            if (child.gameObject.layer != LayerMask.NameToLayer("MinimapOnly"))
            {
                if (!child.CompareTag("FacingOnce"))
                {
                    // 如果标签不是"FacingOnce"，则保持持续朝向摄像头
                    child.rotation = Camera.main.transform.rotation;
                }
            }
        }
    }
}
