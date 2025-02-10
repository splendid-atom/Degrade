using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BigMapController : MonoBehaviour
{
    public GameObject bigMapUI;            // 大地图 UI
    public Camera bigMapCamera;            // 大地图摄像机
    public RectTransform bigMapContainer;  // 大地图容器
    public RectTransform bigMapImage;      // 大地图 UI Image
    public RectTransform playerArrow;      // 玩家箭头
    public Transform player;               // 玩家对象
    public Image fogMaskImage;             // 遮罩层的 Image 组件

    public float zoomSpeed = 5f;           // 缩放速度
    public float minZoom = 10f;            // 最小缩放
    public float maxZoom = 100f;           // 最大缩放
    public float returnSpeed = 5f;         // 缩小后平滑回原点速度

    private bool isMapOpen = false;
    private Vector3 lastPlayerPosition;    // 玩家上一帧的位置

    private bool isDragging = false;
    private Vector2 dragStartPosition;
    private Vector2 imageStartPosition;

    void Start()
    {
        bigMapUI.SetActive(false);
        if (player != null)
        {
            lastPlayerPosition = player.position;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            ToggleMap();
        }

        if (isMapOpen)
        {
            if (IsMouseInBigMapContainer())
            {
                HandleBigMapZoom();
                HandleBigMapDrag();
            }
            UpdatePlayerArrow();
            ResetMapPositionIfMinimized();
        }
    }

    void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        bigMapUI.SetActive(isMapOpen);
    }

    void HandleBigMapZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0f)
        {
            Zoom(scroll);
        }
    }

    void Zoom(float scroll)
    {
        float currentSize = bigMapCamera.orthographicSize;
        float newSize = Mathf.Clamp(currentSize - scroll * zoomSpeed, minZoom, maxZoom);
        bigMapCamera.orthographicSize = newSize;
    }

    void HandleBigMapDrag()
    {
        if (Input.GetMouseButtonDown(0)) // 按下左键
        {
            isDragging = true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bigMapContainer, Input.mousePosition, null, out dragStartPosition);
            imageStartPosition = bigMapImage.anchoredPosition;
        }
        else if (Input.GetMouseButton(0) && isDragging) // 拖动
        {
            Vector2 currentMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bigMapContainer, Input.mousePosition, null, out currentMousePosition);
            Vector2 dragOffset = currentMousePosition - dragStartPosition;
            Vector2 newPosition = imageStartPosition + dragOffset;

            // 获取大地图 UI 的尺寸
            float mapWidth = bigMapImage.rect.width;
            float mapHeight = bigMapImage.rect.height;

            // 获取大地图容器的尺寸
            float containerWidth = bigMapContainer.rect.width;
            float containerHeight = bigMapContainer.rect.height;

            // 限制地图不超出容器边界
            newPosition.x = Mathf.Clamp(newPosition.x, -containerWidth / 1.5f, containerWidth / 1.5f);
            newPosition.y = Mathf.Clamp(newPosition.y, -containerHeight / 1.5f, containerHeight / 1.5f);

            // 更新大地图的位置
            bigMapImage.anchoredPosition = newPosition;
        }
        else if (Input.GetMouseButtonUp(0)) // 释放左键
        {
            isDragging = false;
        }
    }

    void ResetMapPositionIfMinimized()
    {
        if (Mathf.Approximately(bigMapCamera.orthographicSize, maxZoom))
        {
            bigMapImage.anchoredPosition = Vector2.Lerp(
                bigMapImage.anchoredPosition,
                Vector2.zero,
                Time.deltaTime * returnSpeed
            );
        }
    }

    bool IsMouseInBigMapContainer()
    {
        return bigMapContainer != null && RectTransformUtility.RectangleContainsScreenPoint(bigMapContainer, Input.mousePosition, null);
    }

    void UpdatePlayerArrow()
    {
        if (player == null || playerArrow == null || bigMapCamera == null || bigMapContainer == null)
            return;

        // 获取玩家在世界中的坐标
        Vector3 playerWorldPosition = player.position;

        // 计算玩家在大地图上的相对坐标
        Vector3 relativePosition = playerWorldPosition - bigMapCamera.transform.position;

        // 获取大地图 UI 的尺寸
        float mapWidth = bigMapImage.rect.width;
        float mapHeight = bigMapImage.rect.height;

        // 计算归一化坐标 (-1 到 1 之间)
        float normalizedX = Mathf.Clamp(relativePosition.x / bigMapCamera.orthographicSize, -1f, 1f);
        float normalizedY = Mathf.Clamp(relativePosition.y / bigMapCamera.orthographicSize, -1f, 1f);

        // 计算箭头在 `bigMapImage` 里的 UI 坐标
        playerArrow.anchoredPosition = new Vector2(
            normalizedX * mapWidth * 0.5f,
            normalizedY * mapHeight * 0.5f
        ) + bigMapImage.anchoredPosition; // **考虑 `bigMapImage` 的位置偏移**
    }

    public void SetFogOfWar(float percentage)
    {
        // 设置大地图遮罩透明度，模拟 fog of war
        if (fogMaskImage != null)
        {
            Color color = fogMaskImage.color;
            color.a = Mathf.Clamp01(1.0f - percentage); // 通过探索进度改变透明度
            fogMaskImage.color = color;
        }
    }
}
