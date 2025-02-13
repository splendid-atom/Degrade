using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    private List<Transform> childs = new List<Transform>();  // 使用List动态管理子物体

    void Start()
    {
        // 初始化时检查当前所有子物体
        UpdateChilds(true);  // 标记为初始化时调用
    }

    // Update is called once per frame
    void Update()
    {
        // 每帧更新子物体列表，确保新添加的子物体被包含
        UpdateChilds(false);  // 标记为每帧更新
        RotateObjects();
    }

    // 更新子物体列表
    void UpdateChilds(bool isStart)
    {
        if (isStart)
        {
            childs.Clear();  // 仅在Start时清空当前子物体列表
        }

        // 遍历所有当前子物体
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            AddChildRecursive(child);  // 递归地添加子物体
        }
    }

    // 递归添加子物体并处理旋转逻辑
    void AddChildRecursive(Transform child)
    {
        if (!childs.Contains(child))  // 防止重复添加
        {
            childs.Add(child);

            // 如果该物体的标签是"FacingOnce"，则设置旋转
            if (child.CompareTag("FacingOnce"))
            {
                // 仅在第一次运行时旋转一次，之后不再旋转
                child.rotation = Camera.main.transform.rotation;
            }

            // 对于所有子物体，包括"RotatingContainer"的子物体，继续递归
            for (int i = 0; i < child.childCount; i++)
            {
                AddChildRecursive(child.GetChild(i));  // 递归地添加所有子物体
            }
        }
    }

    // 使子物体始终面向摄像头
    void RotateObjects()
    {
        for (int i = childs.Count - 1; i >= 0; i--)  // 反向遍历，防止在移除元素时出错
        {
            Transform child = childs[i];

            // 如果当前物体已经销毁，移除它
            if (child == null)
            {
                childs.RemoveAt(i);
                continue;
            }

            // 如果当前物体是 "NoRotation"，则忽略旋转
            if (child.CompareTag("NoRotation"))
            {
                continue;
            }

            // 如果物体不是 "FacingOnce"，则继续旋转
            if (child.gameObject.layer != LayerMask.NameToLayer("MinimapOnly"))
            {
                if (!child.CompareTag("FacingOnce"))
                {
                    child.rotation = Camera.main.transform.rotation;
                }
            }

            // 对于子物体，如果它是 "RotatingContainer"，继续递归旋转它的子物体
            RotateObjectsRecursively(child);
        }
    }

    // 递归旋转子物体（确保递归处理每个子物体）
    void RotateObjectsRecursively(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)  // 反向遍历，防止在移除元素时出错
        {
            Transform child = parent.GetChild(i);

            // 如果当前物体已经销毁，跳过
            if (child == null)
            {
                continue;
            }

            // 如果当前物体不是 "NoRotation"，则旋转它
            if (!child.CompareTag("NoRotation"))
            {
                child.rotation = Camera.main.transform.rotation;
            }

            // 继续递归其子物体
            RotateObjectsRecursively(child);
        }
    }
}
