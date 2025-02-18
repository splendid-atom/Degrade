using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BambooForestPolygon : MonoBehaviour
{
    public GameObject[] bambooPrefabs;  // 竹子 prefabs 数组
    public int bambooCount = 50;        // 竹子数量

    // 竹林范围由 PolygonCollider2D 定义
    public PolygonCollider2D bambooArea;

    // 玩家物体，用于获取其 BoxCollider 的宽度
    public GameObject playerObject;

    // 父物体，可以通过 Inspector 设置
    public Transform parentObject;


    // 存储竹子对象池
    private Queue<GameObject> bambooPool = new Queue<GameObject>();

    // 存储已生成的竹子位置
    private List<Vector3> bambooPositions = new List<Vector3>();

    void Start()
    {
        if (bambooArea == null)
        {
            Debug.LogError("PolygonCollider2D 未设置！");
            return;
        }

        // 初始化竹子对象池
        InitializeBambooPool();

        // 创建竹林
        CreateBambooForest();
    }

    // 初始化竹子对象池
    void InitializeBambooPool()
    {
        for (int i = 0; i < bambooCount; i++)
        {
            int randomIndex = Random.Range(0, bambooPrefabs.Length);
            GameObject bambooPrefab = bambooPrefabs[randomIndex];
            GameObject bamboo = Instantiate(bambooPrefab);
            bamboo.SetActive(false);  // 初始时不激活竹子
            bambooPool.Enqueue(bamboo);  // 将竹子加入池中
        }
    }

    // 创建竹林
    void CreateBambooForest()
    {
        // 获取玩家的 BoxCollider 宽度
        float playerColliderWidth = playerObject.GetComponent<BoxCollider2D>().size.x;

        for (int i = 0; i < bambooCount; i++)
        {
            bool validPosition = false;
            Vector3 randomPosition = Vector3.zero;

            // 在区域内找到一个有效位置
            while (!validPosition)
            {
                randomPosition = GetRandomPositionInPolygon();

                // 检查该位置与已有竹子之间的最小间距
                validPosition = true;
                foreach (Vector3 bambooPosition in bambooPositions)
                {
                    if (Vector3.Distance(randomPosition, bambooPosition) < playerColliderWidth * 1.2f)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }

            // 从对象池中获取一个竹子对象
            GameObject bamboo = bambooPool.Dequeue();
            bamboo.transform.position = randomPosition;  // 设置位置
            // bamboo.layer = LayerMask.NameToLayer("BambooForest");  

            bamboo.SetActive(true);  // 激活竹子

            // 先将竹子添加到 parentObject 中
            if (parentObject != null)
            {
                bamboo.transform.SetParent(parentObject);
            }

            // 保存竹子位置
            bambooPositions.Add(randomPosition);

            // 将竹子放回对象池
            bambooPool.Enqueue(bamboo);
        }
    }

    // 获取在多边形区域内的随机位置
    Vector3 GetRandomPositionInPolygon()
    {
        Bounds bounds = bambooArea.bounds;
        int maxAttempts = 20;  // 限制最大尝试次数，防止死循环

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomPoint = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            if (bambooArea.OverlapPoint(randomPoint))
            {
                return new Vector3(randomPoint.x, randomPoint.y, 0);
            }
        }

        Debug.LogWarning("无法找到合适的竹子位置，返回 PolygonCollider2D 的中心点");
        return bambooArea.bounds.center;
    }
}
