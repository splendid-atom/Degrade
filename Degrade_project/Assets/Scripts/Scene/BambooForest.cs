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

    void Start()
    {
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
        float angle = Random.Range(0f, 2f * Mathf.PI);
        // 随机生成一个半径值，确保位置在指定的圆形区域内
        float randomRadius = Random.Range(0f, radius);

        // 转换为笛卡尔坐标
        float x = randomRadius * Mathf.Cos(angle);
        float y = randomRadius * Mathf.Sin(angle);

        // 以脚本挂载的对象位置为圆心返回随机位置
        return new Vector3(x + transform.position.x, y + transform.position.y, 0);
    }
}
