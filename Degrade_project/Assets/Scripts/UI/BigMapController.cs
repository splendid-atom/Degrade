using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;
using System.Text;

//bug:地图拖动时迷雾位置不对

public class BigMapController : MonoBehaviour
{
    public GameObject bigMapUI;
    public Camera bigMapCamera;
    public RectTransform bigMapContainer;
    public RectTransform bigMapImage;
    public RectTransform playerArrow;
    private Transform player;

    public float zoomSpeed = 5f;
    public float minZoom = 10f;
    public float maxZoom = 100f;
    public float returnSpeed = 5f;

    private bool isMapOpen = false;
    private Vector3 lastPlayerPosition;
    private Vector2 originalMapPosition;

    private Vector2 dragStartPosition;
    private Vector2 imageStartPosition;

    private bool[,] exploredMap;
    private int mapWidth = 2500;
    private int mapHeight = 2400;
    private float exploreRadius = 10f;

    public Image fogOfWarImage;
    private Texture2D fogTexture;

    public int subGridSize = 1; // 每个大格子中的小格子数
    private int bigMapWidth;
    private int bigMapHeight;
    private Coroutine fogUpdateCoroutine;
    private Vector3 dragOrigin;
    public float CameraFogScaling = 2f;
    private Vector2 playerArrowInitialPosition;
    private Vector2 playerArrowPos;
    public RectTransform DragContainer; // 拖动容器
    public float rotateTime = 0.2f;
    private bool isRotating = false;
    public int currentRotationIndex = 0; // 旋转索引（-4 到 3）
    // 保存当前的旋转角度
    public float currentRotation = 0f;

    void Start()
    {
        player = PlayerController.Instance.transform;
        playerArrowInitialPosition = playerArrow.anchoredPosition;
        bigMapUI.SetActive(false);


        originalMapPosition = bigMapImage.anchoredPosition;

        bigMapWidth = mapWidth / subGridSize;
        bigMapHeight = mapHeight / subGridSize;
        exploredMap = new bool[bigMapWidth, bigMapHeight];

        fogTexture = new Texture2D(mapWidth, mapHeight, TextureFormat.RGBA32, false);
        fogTexture.wrapMode = TextureWrapMode.Clamp;
        fogTexture.filterMode = FilterMode.Bilinear;
        ClearFogTexture();

        LoadExplorationData();

        fogUpdateCoroutine = StartCoroutine(UpdateFogOfWar());
        if (player != null)
        {
            lastPlayerPosition = player.position;
        }        
    }
    void Rotate()
    {
        if (Input.GetKey(KeyCode.Q) && !isRotating)
        {
            currentRotationIndex++; // 逆时针旋转
            if (currentRotationIndex > 3) currentRotationIndex = -4;
            StartCoroutine(RotateAround(-45, rotateTime));
        }
        if (Input.GetKey(KeyCode.E) && !isRotating)
        {
            currentRotationIndex--; // 顺时针旋转
            if (currentRotationIndex < -4) currentRotationIndex = 3;
            StartCoroutine(RotateAround(45, rotateTime));
        }
    }
    IEnumerator RotateAround(float angle, float time)
    {
        float steps = 60 * time;
        float anglePerStep = angle / steps;
        isRotating = true;

        for (int i = 0; i < steps; i++)
        {
            // 主摄像头旋转
            DragContainer.transform.Rotate(new Vector3(0, 0, anglePerStep));

            // 更新当前旋转角度
            currentRotation += anglePerStep;
            currentRotation %= 360;
            yield return new WaitForFixedUpdate();
        }
        isRotating = false;
    }


    void OnApplicationQuit()
    {
        SaveExplorationData(); // 在应用退出时保存数据
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
                // HandleBigMapZoom();
                HandleBigMapDrag();
            }
            // ResetMapPositionIfMinimized();
        }
        // 在任何时候都更新探索区域
        UpdatePlayerArrow();
        Rotate();
    }

    void ToggleMap()
    {
        isMapOpen = !isMapOpen;
        bigMapUI.SetActive(isMapOpen);



    }

    // void HandleBigMapZoom()
    // {
    //     float scroll = Input.GetAxis("Mouse ScrollWheel");
    //     if (Mathf.Abs(scroll) > 0f)
    //     {
    //         Zoom(scroll);
    //     }
    // }

    // void Zoom(float scroll)
    // {
    //     // 获取当前的正交相机大小
    //     float currentSize = bigMapCamera.orthographicSize;

    //     // 计算新的缩放值，确保在设定的最大和最小缩放范围内
    //     float newSize = Mathf.Clamp(currentSize - scroll * zoomSpeed, minZoom, maxZoom);

    //     // 更新相机的正交尺寸
    //     bigMapCamera.orthographicSize = newSize;

    //     // 计算缩放比例
    //     float scaleFactor = newSize / currentSize;

    //     // 更新大地图和迷雾图层的缩放
    //     bigMapImage.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    //     fogOfWarImage.rectTransform.localScale = new Vector3(scaleFactor, scaleFactor, 1);
    // }


    void HandleBigMapDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 获取鼠标按下时的本地坐标
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bigMapContainer, Input.mousePosition, null, out dragStartPosition);
            imageStartPosition = DragContainer.anchoredPosition; // 使用 DragContainer
        }

        if (Input.GetMouseButton(0))
        {
            // 获取当前鼠标的位置并计算偏移量
            Vector2 currentMousePosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(bigMapContainer, Input.mousePosition, null, out currentMousePosition);
            Vector2 dragOffset = currentMousePosition - dragStartPosition;

            // 计算新位置
            Vector2 newPosition = imageStartPosition + dragOffset;

            // 限制拖动的边界
            float containerWidth = bigMapContainer.rect.width;
            float containerHeight = bigMapContainer.rect.height;
            float mapWidth = DragContainer.rect.width;
            float mapHeight = DragContainer.rect.height;

            float minX = -(mapWidth - containerWidth)/1.5f;
            float maxX = (mapWidth - containerWidth)/1.5f;
            float minY = -(mapHeight - containerHeight)/1.5f;
            float maxY = (mapHeight - containerHeight)/1.5f;

            newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
            newPosition.y = Mathf.Clamp(newPosition.y, minY, maxY);

            // 更新DragContainer的位置
            DragContainer.anchoredPosition = newPosition;
        }

    }



    // void ResetMapPositionIfMinimized()
    // {
    //     if (Mathf.Approximately(bigMapCamera.orthographicSize, maxZoom))
    //     {
    //         bigMapImage.anchoredPosition = Vector2.Lerp(
    //             bigMapImage.anchoredPosition,
    //             originalMapPosition,
    //             Time.deltaTime * returnSpeed
    //         );
    //         UpdatePlayerArrow();
    //     }
    // }

    bool IsMouseInBigMapContainer()
    {
        return bigMapContainer != null && RectTransformUtility.RectangleContainsScreenPoint(bigMapContainer, Input.mousePosition, null);
    }

    void UpdatePlayerArrow()
    {
        if (player == null || playerArrow == null || bigMapCamera == null || bigMapContainer == null)
            return;

        // 获取玩家的世界位置
        Vector3 playerWorldPosition = player.position;
        Vector3 relativePosition = playerWorldPosition - bigMapCamera.transform.position;

        float mapWidth = bigMapImage.rect.width;
        float mapHeight = bigMapImage.rect.height;

        // 计算归一化位置
        float normalizedX = Mathf.Clamp(relativePosition.x / bigMapCamera.orthographicSize, -1f, 1f);
        float normalizedY = Mathf.Clamp(relativePosition.y / bigMapCamera.orthographicSize, -1f, 1f);

        // 更新箭头的位置
        Vector2 newArrowPosition = new Vector2(
            normalizedX * mapWidth * 0.5f,
            normalizedY * mapHeight * 0.5f
        ) + bigMapImage.anchoredPosition;

        Vector2 moveDirection = PlayerController.Instance.moveDirection;
        // 根据玩家的移动方向设置箭头的旋转角度
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        playerArrow.rotation = Quaternion.Euler(0, 0, angle - 90f); // 减去90度以匹配Unity的坐标系

        // 更新箭头的位置
        playerArrow.anchoredPosition = newArrowPosition;
    }



    IEnumerator UpdateFogOfWar()
    {
        while (true)
        {
            UpdateExploredAreas();
            yield return new WaitForSeconds(0.1f); // 每0.1秒更新一次迷雾
        }
    }

    void UpdateExploredAreas()
    {
        
        // 获取箭头的位置而非玩家位置
        playerArrowPos = playerArrow.anchoredPosition;
        if(playerArrowPos==playerArrowInitialPosition){
            return;
        }
        // 将箭头位置转换为大地图坐标系中的位置
        Vector2 playerMapPos = new Vector2(
            (playerArrowPos.x + bigMapImage.rect.width / 2) / subGridSize,
            (playerArrowPos.y + bigMapImage.rect.height / 2) / subGridSize
        );
        // Debug.Log("Player Arrow Position: " + playerArrowPos);
        int playerX = Mathf.FloorToInt(playerMapPos.x);
        int playerY = Mathf.FloorToInt(playerMapPos.y);
       
        // 限制更新的迷雾范围
        int minX = Mathf.Max(playerX - Mathf.FloorToInt(exploreRadius), 0);
        int maxX = Mathf.Min(playerX + Mathf.FloorToInt(exploreRadius), bigMapWidth - 1);
        int minY = Mathf.Max(playerY - Mathf.FloorToInt(exploreRadius), 0);
        int maxY = Mathf.Min(playerY + Mathf.FloorToInt(exploreRadius), bigMapHeight - 1);

        List<Color> colorsToUpdate = new List<Color>();

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                exploredMap[x, y] = true;

                for (int subX = 0; subX < subGridSize; subX++)
                {
                    for (int subY = 0; subY < subGridSize; subY++)
                    {
                        int pixelX = x * subGridSize + subX;
                        int pixelY = y * subGridSize + subY;

                        // 只更新在有效范围内的像素
                        if (pixelX >= 0 && pixelX < mapWidth && pixelY >= 0 && pixelY < mapHeight)
                        {                           
                            colorsToUpdate.Add(Color.clear);  // 清除迷雾                            
                        }
                    }
                }
            }
        }

        if (colorsToUpdate.Count > 0)
        {
            int width = (maxX - minX + 1) * subGridSize;
            int height = (maxY - minY + 1) * subGridSize;

            if (colorsToUpdate.Count != width * height)
            {
                Debug.Log("颜色数组大小与目标区域不匹配！");
                return;
            }

            fogTexture.SetPixels(minX * subGridSize, minY * subGridSize, width, height, colorsToUpdate.ToArray());
            fogTexture.Apply();
        }

        ApplyFogOfWar();
    }





    void UpdateFogCell(int bigX, int bigY)
    {
        for (int subX = 0; subX < subGridSize; subX++)
        {
            for (int subY = 0; subY < subGridSize; subY++)
            {
                int pixelX = bigX * subGridSize + subX;
                int pixelY = bigY * subGridSize + subY;

                // 只更新已探索区域的像素
                if (pixelX >= 0 && pixelX < mapWidth && pixelY >= 0 && pixelY < mapHeight)
                {
                    fogTexture.SetPixel(pixelX, pixelY, Color.clear); // 清除迷雾
                }
            }
        }
        fogTexture.Apply();
    }

    void ApplyFogOfWar()
    {
        // 只在迷雾发生变化时更新
        if (fogOfWarImage.sprite == null || fogOfWarImage.sprite.texture != fogTexture)
        {
            fogOfWarImage.sprite = Sprite.Create(fogTexture, new Rect(0, 0, mapWidth, mapHeight), new Vector2(0.5f, 0.5f));
        }
    }

    void ClearFogTexture()
    {
        // 用 List 存储像素，然后批量更新
        List<Color> pixels = new List<Color>(mapWidth * mapHeight);
        for (int i = 0; i < pixels.Capacity; i++)
        {
            pixels.Add(Color.black); // 初始化为黑色迷雾
        }
        fogTexture.SetPixels(pixels.ToArray());
        fogTexture.Apply();
    }

    void SaveExplorationData()
    {
        StringBuilder sb = new StringBuilder();
        for (int x = 0; x < bigMapWidth; x++)
        {
            for (int y = 0; y < bigMapHeight; y++)
            {
                sb.Append(exploredMap[x, y] ? "1" : "0");
            }
        }
        // 每次游戏结束时保存一次
        File.WriteAllText("explorationData.txt", sb.ToString());
    }

    void LoadExplorationData()
    {
        Debug.Log("LoadExplorationData");
        // 初始化 exploredMap
        exploredMap = new bool[bigMapWidth, bigMapHeight];

        // 可以只在地图初始化时读取一次，而不是每次更新
        if (File.Exists("explorationData.txt"))
        {
            string data = File.ReadAllText("explorationData.txt");
            for (int y = 0; y < bigMapHeight; y++)
            {
                for (int x = 0; x < bigMapWidth; x++)
                {
                    int index = y+x*bigMapHeight;
                    if (index < data.Length)
                    {
                        exploredMap[x, y] = (data[index] == '1');
                    }
                }
            }
        }

        // 在加载完数据后，更新迷雾地图
        UpdateExploredAreasAtStart();
    }

    void UpdateExploredAreasAtStart()
    {
        // 初始化边界变量
        int minX = bigMapWidth;
        int maxX = 0;
        int minY = bigMapHeight;
        int maxY = 0;

        // 遍历 exploredMap 来确定边界
        for (int y = 0; y < bigMapHeight; y++)
        {
            for (int x = 0; x < bigMapWidth; x++)
            {
                if (exploredMap[x, y])
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        // 如果没有探索过的区域，则不需要更新
        if (minX == bigMapWidth || minY == bigMapHeight)
        {
            return;
        }

        // 限制更新的迷雾范围
        minX = Mathf.Max(minX - Mathf.FloorToInt(exploreRadius), 0);
        maxX = Mathf.Min(maxX + Mathf.FloorToInt(exploreRadius), bigMapWidth - 1);
        minY = Mathf.Max(minY - Mathf.FloorToInt(exploreRadius), 0);
        maxY = Mathf.Min(maxY + Mathf.FloorToInt(exploreRadius), bigMapHeight - 1);

        // 更新迷雾纹理
        UpdateFogTexture();
    }

    void UpdateFogTexture()
    {
        Color[] allColors = new Color[mapWidth * mapHeight];

        for (int y = 0; y < bigMapHeight; y++)
        {
            for (int x = 0; x < bigMapWidth; x++)
            {
                Color color = exploredMap[x, y] ? Color.clear : Color.black; // 如果已探索，则设置为透明，否则设置为黑色
                for (int subX = 0; subX < subGridSize; subX++)
                {
                    for (int subY = 0; subY < subGridSize; subY++)
                    {
                        int pixelX = x * subGridSize + subX;
                        int pixelY = y * subGridSize + subY;
                        allColors[pixelX + pixelY * mapWidth] = color;
                    }
                }
            }
        }

        fogTexture.SetPixels(allColors);
        fogTexture.Apply();
    }



}

