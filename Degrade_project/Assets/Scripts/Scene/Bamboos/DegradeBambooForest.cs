using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DegradeBambooForest : MonoBehaviour
{
    public static DegradeBambooForest instance;
    public GameObject[] bambooPrefabs;  // 健康竹子prefabs数组（索引0-2）
    public GameObject[] wiltedBambooPrefabs; // 枯萎竹子prefabs数组（索引3-5）
    public int bambooCount = 50;        // 竹子数量
    
    // 圆形区域半径
    public float areaRadius = 10.0f;

    // 玩家物体，用于获取其 BoxCollider 的宽度
    public GameObject playerObject;

    // 父物体，可以通过Inspector设置
    public Transform parentObject;

    // 存储已生成的竹子信息
    private List<BambooInfo> bambooInfos = new List<BambooInfo>();
    private Vector3 randomPosition;
    private Vector3 originPosition; // 存储脚本挂载对象的初始位置

    // 枯萎相关参数
    public float wiltSpeed = 0.1f; // 枯萎速度，控制处理频率
    public bool isWilted = false;
    private int currentBambooIndex = 0;
    private float globalWiltTimer = 0; // 全局计时器，避免每次处理重置

    // 渐变透明度参数
    public float fadeDuration = 0.5f; // 渐变持续时间
    public int maxFadeBatch = 4;      // 每次处理的最大竹子数量
    public bool isAllWilted = false;
    private GameObject portalTransform;
    private bool startWilt = false;

    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        portalTransform = GameObject.Find("degradePortal");
        portalTransform.SetActive(false);
        originPosition = transform.position; // 在Start时计算一次位置
        CreateBambooForest();
    }
    // 协程：等待 delay 秒后激活传送门
    IEnumerator ActivatePortalWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        startWilt = true;
    }
    public void StartWilt()
    {
        Debug.Log("开始枯萎过程");
        isWilted = true;
        currentBambooIndex = 0;
        globalWiltTimer = 0; // 重置全局计时器
        if (portalTransform != null)
        {
            portalTransform.SetActive(true);
            StartCoroutine(ActivatePortalWithDelay(2f));
        }
        //调用枯萎竹林摄像机
        SwitchDegradeBambooCamera.instance.SwitchToDegradeBambooCamera();
    }

    void Update()
    {
        // 处理枯萎动画
        if (isWilted&&startWilt)
        {
            globalWiltTimer += Time.deltaTime;
            
            if (currentBambooIndex < bambooInfos.Count)
            {
                // 确定当前处理的批次大小
                int batchSize = Random.Range(1, maxFadeBatch);
                int endIndex = currentBambooIndex + batchSize;
                if (endIndex > bambooInfos.Count)
                {
                    endIndex = bambooInfos.Count;
                    batchSize = endIndex - currentBambooIndex;
                }

                // 获取当前处理的竹子信息
                for (int i = currentBambooIndex; i < endIndex; i++)
                {
                    BambooInfo info = bambooInfos[i];
                    
                    // 更新透明度
                    // 使用每个竹子的独立计时器
                    info.wiltTimer += Time.deltaTime;
                    float progress = Mathf.Clamp01(info.wiltTimer / fadeDuration);
                    
                    // 健康竹子透明度逐渐降低
                    info.healthyRenderer.color = new Color(
                        info.healthyRenderer.color.r,
                        info.healthyRenderer.color.g,
                        info.healthyRenderer.color.b,
                        1.0f - progress
                    );
                    
                    // 枯萎竹子透明度逐渐增加
                    info.wiltedRenderer.color = new Color(
                        info.wiltedRenderer.color.r,
                        info.wiltedRenderer.color.g,
                        info.wiltedRenderer.color.b,
                        progress
                    );

                    // 检查是否完成
                    if (info.wiltTimer >= fadeDuration)
                    {
                        info.isWilted = true;
                    }

                    // 调试输出
                    // Debug.Log($"竹子 {i}：健康透明度 {info.healthyRenderer.color.a}，枯萎透明度 {info.wiltedRenderer.color.a}，完成状态：{info.isWilted}");
                }

                // 检查当前批次的所有竹子是否都完成
                bool batchCompleted = true;
                for (int i = currentBambooIndex; i < endIndex; i++)
                {
                    if (!bambooInfos[i].isWilted)
                    {
                        batchCompleted = false;
                        break;
                    }
                }

                if (batchCompleted)
                {
                    // 标记当前批次的所有竹子为完成
                    for (int i = currentBambooIndex; i < endIndex; i++)
                    {
                        // 销毁健康的竹子对象
                        Destroy(bambooInfos[i].healthyGameObject);
                    }
                    currentBambooIndex = endIndex;
                    globalWiltTimer = 0; // 重置计时器，处理下一组

                    // 检查是否所有竹子都处理完毕
                    if (currentBambooIndex >= bambooInfos.Count)
                    {
                        isWilted = false;
                        isAllWilted = true;
                    }
                }
            }
            else
            {
                // 所有竹子都处理完毕，停止枯萎过程
                isWilted = false;
                isAllWilted = true;
            }
        }
    }

    void CreateBambooForest()
    {
        // 获取玩家的 BoxCollider 宽度
        float playerColliderWidth = playerObject.GetComponent<BoxCollider2D>().size.x;

        for (int i = 0; i < bambooCount; i++)
        {
            bool validPosition = false;

            // 在区域内找到一个有效位置
            while (!validPosition)
            {
                // 随机生成位置
                randomPosition = GetRandomPositionInCircle(areaRadius);

                // 检查这个位置与已有竹子之间的距离
                validPosition = true;
                foreach (BambooInfo bambooInfo in bambooInfos)
                {
                    float distance = Vector3.Distance(randomPosition, bambooInfo.position);
                    if (distance < playerColliderWidth * 1.2f)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }

            // 随机选择一个健康竹子Prefab
            int randomHealthyIndex = Random.Range(0, bambooPrefabs.Length);
            GameObject healthyBambooPrefab = bambooPrefabs[randomHealthyIndex];
            
            // 获取对应的枯萎竹子Prefab索引
            int randomWiltedIndex = randomHealthyIndex; // 1对4，2对5，3对6
            if (randomWiltedIndex >= wiltedBambooPrefabs.Length)
            {
                randomWiltedIndex = 0; // 如果超出范围，循环回到开头
            }
            GameObject wiltedBambooPrefab = wiltedBambooPrefabs[randomWiltedIndex];

            // 创建健康竹子对象
            GameObject healthyBamboo = Instantiate(healthyBambooPrefab, randomPosition, Quaternion.identity);
            // 创建枯萎竹子对象
            GameObject wiltedBamboo = Instantiate(wiltedBambooPrefab, randomPosition, Quaternion.identity);

            // 如果指定了父物体，则设置父物体
            if (parentObject != null)
            {
                healthyBamboo.transform.SetParent(parentObject);
                wiltedBamboo.transform.SetParent(parentObject);
            }

            // 记录竹子的信息
            BambooInfo info = new BambooInfo();
            info.position = randomPosition;
            info.healthyRenderer = healthyBamboo.GetComponent<SpriteRenderer>();
            info.wiltedRenderer = wiltedBamboo.GetComponent<SpriteRenderer>();
            info.healthyGameObject = healthyBamboo; // 存储健康竹子的游戏对象
            info.wiltTimer = 0.0f; // 初始化透明度变化计时器
            info.prefabIndex = randomHealthyIndex;
            info.distance = Vector3.Distance(randomPosition, originPosition);
            info.isWilted = false;

            // 初始化状态：健康竹子可见，枯萎竹子不可见
            info.healthyRenderer.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            info.wiltedRenderer.color = new Color(1.0f, 1.0f, 1.0f, 0.0f);

            bambooInfos.Add(info);
        }

        // 按距离从近到远排序
        bambooInfos.Sort((a, b) => a.distance.CompareTo(b.distance));
    }

    // 获取圆形区域内的随机位置
    Vector3 GetRandomPositionInCircle(float radius)
    {
        // 随机生成一个角度
        // 使用更简单的随机生成方法
        float x = Random.Range(-radius, radius);
        float y = Random.Range(-radius, radius);
        while (x * x + y * y > radius * radius) // 确保点在圆内
        {
            x = Random.Range(-radius, radius);
            y = Random.Range(-radius, radius);
        }

        return new Vector3(x + originPosition.x, y + originPosition.y, 0);
    }

    // 存储竹子的信息
    private class BambooInfo
    {
        public Vector3 position;
        public SpriteRenderer healthyRenderer; // 健康竹子的SpriteRenderer
        public SpriteRenderer wiltedRenderer;  // 枯萎竹子的SpriteRenderer
        public GameObject healthyGameObject;   // 健康竹子的游戏对象
        public float wiltTimer;               // 透明度变化计时器
        public int prefabIndex;
        public float distance;
        public bool isWilted;     // 是否已经枯萎
    }
}