using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BambooForest : MonoBehaviour
{
    public GameObject[] bambooPrefabs;  // 竹子prefabs数组
    public int bambooCount = 50;        // 竹子数量
    
    // 圆形区域半径
    public float areaRadius = 10.0f;

    // 玩家物体，用于获取其 BoxCollider 的宽度
    public GameObject playerObject;

    // 父物体，可以通过Inspector设置
    public Transform parentObject;

    // 存储已生成的竹子位置
    private List<Vector3> bambooPositions = new List<Vector3>();
    private Vector3 randomPosition;
    private Vector3 originPosition; // 存储脚本挂载对象的初始位置

    void Start()
    {
        originPosition = transform.position; // 在Start时计算一次位置
        CreateBambooForest();
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
                foreach (Vector3 bambooPosition in bambooPositions)
                {
                    float distance = Vector3.Distance(randomPosition, bambooPosition);
                    if (distance < playerColliderWidth * 1.2)
                    {
                        validPosition = false;
                        break;
                    }
                }
            }

            // 随机选择一个竹子Prefab
            int randomIndex = Random.Range(0, bambooPrefabs.Length);
            GameObject bambooPrefab = bambooPrefabs[randomIndex];

            // 创建竹子对象
            GameObject bamboo = Instantiate(bambooPrefab, randomPosition, Quaternion.identity);

            // 如果指定了父物体，则设置父物体
            if (parentObject != null)
            {
                bamboo.transform.SetParent(parentObject);
            }
            else
            {
                bamboo.transform.parent = null;  // 如果没有指定父物体，则不设置父物体
            }

            bambooPositions.Add(randomPosition);  // 保存竹子位置
        }
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
}
