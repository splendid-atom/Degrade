using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    private Transform player;          // 玩家对象
    public Transform arrowIndicator;  // 小地图上的人物箭头
    public float zoomSpeed = 5f;      // 键盘缩放速度
    public float scrollSpeed = 10f;   // 滚轮缩放速度
    public float minZoom = 5f;        // 最小缩放值
    public float maxZoom = 20f;       // 最大缩放值

    private Camera miniMapCamera;     // 小地图摄像机组件
    public bool isZoomEnabled = true; // 是否允许小地图缩放

    void Start()
    {
        player = PlayerController.Instance.transform;
        // 获取小地图摄像机组件
        miniMapCamera = GetComponent<Camera>();
    }

    void Update()
    {
        // 仅当缩放启用时处理缩放
        if (isZoomEnabled)
        {
            HandleZoom();
            // 缩放人物箭头
            ScaleArrowWithZoom();
        }


    }

    // LateUpdate 用于跟随玩家移动
    void LateUpdate()
    {
        if (player != null)
        {
            // 跟随玩家移动
            Vector3 newPosition = player.position;
            newPosition.z = transform.position.z; // 保持高度不变
            transform.position = newPosition;
        }
    }

    // 处理缩放
    void HandleZoom()
    {
        if (miniMapCamera != null)
        {
            // 检测 "+" 和 "-" 按键
            if ((Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals)))
            {
                ZoomIn(zoomSpeed);
            }
            else if (Input.GetKey(KeyCode.Minus))
            {
                ZoomOut(zoomSpeed);
            }

            // 检测鼠标滚轮
            // float scroll = Input.GetAxis("Mouse ScrollWheel");
            // if (scroll > 0f)
            // {
            //     ZoomIn(scrollSpeed);
            // }
            // else if (scroll < 0f)
            // {
            //     ZoomOut(scrollSpeed);
            // }
        }
    }

    // 缩小视野（放大小地图）
    void ZoomIn(float speed)
    {
        miniMapCamera.orthographicSize = Mathf.Max(minZoom, miniMapCamera.orthographicSize - speed * Time.deltaTime);
    }

    // 放大视野（缩小小地图）
    void ZoomOut(float speed)
    {
        miniMapCamera.orthographicSize = Mathf.Min(maxZoom, miniMapCamera.orthographicSize + speed * Time.deltaTime);
    }

    // 调整箭头缩放，使其与小地图缩放保持一致
    void ScaleArrowWithZoom()
    {
        if (arrowIndicator != null)
        {
            float zoomFactor = Mathf.Clamp(miniMapCamera.orthographicSize, minZoom, maxZoom);
            arrowIndicator.localScale = new Vector3(1 / (zoomFactor / 10), 1 / (zoomFactor / 10), 1);
        }
    }
}
