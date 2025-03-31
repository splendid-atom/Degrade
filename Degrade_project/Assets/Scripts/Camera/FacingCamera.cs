
// FacingCamera.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    public static FacingCamera instance;
    public List<Transform> childs = new List<Transform>();  // 使用List动态管理子物体
    private Camera mainCamera;
    private bool returnInitial = false;
    private Quaternion initialCameraRotation;

    void Awake()
    {
        instance = this;
        mainCamera = Camera.main;
        initialCameraRotation = mainCamera.transform.rotation;
        UpdateChilds(true);  // 初始化时更新子物体列表
    }

    void Start()
    {
        UpdateChilds(true);  // 初始化时更新子物体列表
    }

    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
            {
                initialCameraRotation = mainCamera.transform.rotation; // 重新获取初始旋转
            }
        }
        UpdateChilds(true);
        RotateObjects();
    }

    public void UpdateChilds(bool isStart)
    {
        if (isStart)
        {
            childs.Clear();  // 仅在Start时清空当前子物体列表
        }

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            AddChildRecursive(child);
        }
    }

    void AddChildRecursive(Transform child)
    {
        if (!childs.Contains(child))
        {
            childs.Add(child);

            // 如果该物体的标签是"FacingOnce"，则设置旋转
            if (child.CompareTag("FacingOnce"))
            {
                child.rotation = mainCamera.transform.rotation;
            }

            // 对于所有子物体，包括"RotatingContainer"的子物体，继续递归
            for (int i = 0; i < child.childCount; i++)
            {
                AddChildRecursive(child.GetChild(i));
            }
        }
    }

    void RotateObjects()
    {
        for (int i = childs.Count - 1; i >= 0; i--)
        {
            Transform child = childs[i];
            if (child == null)
            {
                childs.RemoveAt(i);
                continue;
            }

            if (child.CompareTag("NoRotation"))
            {
                continue;
            }

            if (!child.CompareTag("FacingOnce"))
            {
                if (returnInitial)
                {
                    child.rotation = initialCameraRotation;
                }
                else
                {
                    child.rotation = mainCamera.transform.rotation;
                }
            }

            // RotateObjectsRecursively(child);
        }
    }

    void RotateObjectsRecursively(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Transform child = parent.GetChild(i);

            if (child == null)
            {
                continue;
            }

            if (!child.CompareTag("NoRotation"))
            {
                if (returnInitial)
                {
                    child.rotation = initialCameraRotation;
                }
                else
                {
                    child.rotation = mainCamera.transform.rotation;
                }
            }

            RotateObjectsRecursively(child);
        }
    }

    // 恢复childs列表
    public void RestoreChilds(List<Transform> savedChilds)
    {
        childs = savedChilds;
        // 恢复后需要重新更新旋转
        RotateObjects();
    }
}