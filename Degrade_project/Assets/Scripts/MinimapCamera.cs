using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapCamera : MonoBehaviour
{
    public Transform player;      // 玩家对象
    public Transform arrowIndicator;  // 小地图上的人物箭头
    public float zoomSpeed = 5f;  // 键盘缩放速度
    public float scrollSpeed = 10f; // 滚轮缩放速度
    public float minZoom = 5f;    // 最小缩放值
    public float maxZoom = 20f;   // 最大缩放值

    private Camera miniMapCamera; // 小地图摄像机组件

    // Start is called before the first frame update
    void Start()
    {
        // 获取小地图摄像机组件
        miniMapCamera = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        // 处理缩放
        HandleZoom();
        // 缩放人物箭头
        ScaleArrowWithZoom();
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
            // 检查是否按下 Shift 键
            bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

            // 检测 "+" 和 "-" 按键，检查 Shift 是否按下
            // if ((Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals)) && isShiftPressed) // "+" 或 "=" 键并且 Shift 键按下时重置缩放
            // {
            //     ResetZoom();  // 重置小地图的缩放
            // }
            if ((Input.GetKey(KeyCode.Plus) || Input.GetKey(KeyCode.Equals))) // "+" 或 "=" 键放大
            {
                ZoomIn(zoomSpeed);
            }
            else if (Input.GetKey(KeyCode.Minus)) // "-" 键缩小
            {
                ZoomOut(zoomSpeed);
            }

            // 检测鼠标滚轮
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0f)
            {
                // 根据滚轮方向调整缩放
                if (scroll > 0f)
                {
                    ZoomIn(scrollSpeed);
                }
                else if (scroll < 0f)
                {
                    ZoomOut(scrollSpeed);
                }
            }
        }
    }

    // 重置小地图的缩放为初始大小
    void ResetZoom()
    {
        miniMapCamera.orthographicSize = minZoom;  // 或者使用一个你认为适合的默认值
        // 设置箭头的最大最小缩放值
        float zoomFactor = Mathf.Clamp(miniMapCamera.orthographicSize, minZoom, maxZoom);

        // 设置箭头的缩放值为固定的大小
        arrowIndicator.localScale = new Vector3(1/(zoomFactor/10), 1/(zoomFactor/10), 1);

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
            // 设置箭头的最大最小缩放值
            float zoomFactor = Mathf.Clamp(miniMapCamera.orthographicSize, minZoom, maxZoom);

            // 设置箭头的缩放值为固定的大小
            arrowIndicator.localScale = new Vector3(1/(zoomFactor/10), 1/(zoomFactor/10), 1);
        }
    }

}
