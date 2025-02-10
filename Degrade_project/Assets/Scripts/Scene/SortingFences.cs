using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SortingFences : MonoBehaviour
{
    private GameObject player;
    private SortingGroup sortingGroup;
    private Camera mainCamera;
    private Transform cameraPosition;
    private RotatingCamera rotatingCameraScript;

    [Header("负半周触发器")]
    public IsInsideTrigger negativeTrigger; // 负半周触发器脚本

    [Header("正半周触发器")]
    public IsInsideTrigger positiveTrigger; // 正半周触发器脚本
    private float offsetDegree = 15f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        sortingGroup = transform.Find("wooden_fence_1")?.GetComponent<SortingGroup>();
        mainCamera = Camera.main;
        rotatingCameraScript = FindObjectOfType<RotatingCamera>();

        if (negativeTrigger == null)
        {
            Debug.LogError("负半周触发器未分配，请在 Inspector 拖拽！", gameObject);
        }
        if (positiveTrigger == null)
        {
            Debug.LogError("正半周触发器未分配，请在 Inspector 拖拽！", gameObject);
        }
    }

    void Update()
    {   
        if (gameObject.layer == LayerMask.NameToLayer("MinimapOnly")){
            return;
        }
        if (player == null || sortingGroup == null || mainCamera == null)
            return;
        
        // 获取父对象的旋转角度
        float parentRotationZ = transform.parent.eulerAngles.z;
        // Debug.Log("父对象旋转角度：" + parentRotationZ);
        // // 将旋转角度限定在 -180 到 180 之间
        if (parentRotationZ > 180f) parentRotationZ -= 360f;
        // Debug.Log("父对象旋转角度：" + parentRotationZ);
        int currentRotationIndex = rotatingCameraScript?.currentRotationIndex ?? 0;

        bool isNegativeHalf = false;
        bool isPositiveHalf = false;
        //获取的是float变量，不能直接==来判断，有误差
        // 根据不同的父对象旋转角度来设置正负半周的判断
        if (parentRotationZ >= 0f - offsetDegree && parentRotationZ <= 0f + offsetDegree)
        {
            // 父对象旋转为 0 时的正负半周判断
            isNegativeHalf = (currentRotationIndex == -3 || currentRotationIndex == -4 || currentRotationIndex == 3);
            isPositiveHalf = (currentRotationIndex == 0 || currentRotationIndex == -1 || currentRotationIndex == 1);
        }
        else if (parentRotationZ >= 180f - offsetDegree && parentRotationZ <= 180f + offsetDegree) // 原来是 180
        {
            // 父对象旋转为 180 时的正负半周判断，与0度相反
            isNegativeHalf = (currentRotationIndex == 0 || currentRotationIndex == 1 || currentRotationIndex == -1);
            isPositiveHalf = (currentRotationIndex == -3 || currentRotationIndex == -4 || currentRotationIndex == 3);
        }
        else if (parentRotationZ >= -90f - offsetDegree && parentRotationZ <= -90f + offsetDegree)  // 原来是 -90
        {
            // 父对象旋转为 -90 时的正负半周判断
            isNegativeHalf = (currentRotationIndex == 1 || currentRotationIndex == 2 || currentRotationIndex == 3);
            isPositiveHalf = (currentRotationIndex == -1 || currentRotationIndex == -2 || currentRotationIndex == -3);
        }
        else if (parentRotationZ >= 90f - offsetDegree && parentRotationZ <= 90f + offsetDegree)  // 原来是 90
        {
            // 父对象旋转为 90 时的正负半周判断，与 -90 度相反
            isNegativeHalf = (currentRotationIndex == -1 || currentRotationIndex == -2 || currentRotationIndex == -3);
            isPositiveHalf = (currentRotationIndex == 1 || currentRotationIndex == 2 || currentRotationIndex == 3);
        }
        else if (parentRotationZ >= 45f - offsetDegree && parentRotationZ <= 45f + offsetDegree)  // 原来是 45
        {
            // 父对象旋转为 45 时的正负半周判断
            isNegativeHalf = (currentRotationIndex == -2 || currentRotationIndex == -3 || currentRotationIndex == -4);
            isPositiveHalf = (currentRotationIndex == 0 || currentRotationIndex == 1 || currentRotationIndex == 2);
        }
        else if (parentRotationZ >= -135f - offsetDegree && parentRotationZ <= -135f + offsetDegree)  // 原来是 -135
        {
            // 父对象旋转为 225 时的正负半周判断，与 45 度相反
            isNegativeHalf = (currentRotationIndex == 0 || currentRotationIndex == 1 || currentRotationIndex == 2);
            isPositiveHalf = (currentRotationIndex == -2 || currentRotationIndex == -3 || currentRotationIndex == -4);
        }
        else if (parentRotationZ >= 135f - offsetDegree && parentRotationZ <= 135f + offsetDegree)  // 原来是 135
        {
            // 父对象旋转为 135 时的正负半周判断
            isNegativeHalf = (currentRotationIndex == 0 || currentRotationIndex == -1 || currentRotationIndex == -2);
            isPositiveHalf = (currentRotationIndex == 2 || currentRotationIndex == 3 || currentRotationIndex == -4);
        }
        else if (parentRotationZ >= -45f - offsetDegree && parentRotationZ <= -45f + offsetDegree)  // 原来是 -45
        {
            // 父对象旋转为 325 时的正负半周判断，与 135 度相反
            isNegativeHalf = (currentRotationIndex == 2 || currentRotationIndex == 3 || currentRotationIndex == -4);
            isPositiveHalf = (currentRotationIndex == 0 || currentRotationIndex == -1 || currentRotationIndex == -2);
        }

        // 根据正负半周设置排序
        if (isNegativeHalf)
        {
            sortingGroup.sortingOrder = negativeTrigger != null && negativeTrigger.isPlayerInside ? -1 : 0;
            return;
        }

        if (isPositiveHalf)
        {
            sortingGroup.sortingOrder = positiveTrigger != null && positiveTrigger.isPlayerInside ? -1 : 0;
            return;
        }
    }
}
