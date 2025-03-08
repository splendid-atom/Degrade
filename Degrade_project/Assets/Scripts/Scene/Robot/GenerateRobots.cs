using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateRobots : MonoBehaviour
{
    [Header("敌人Prefab")]
    public GameObject enemyPrefab;  // 你的敌人Prefab

    [Header("生成区域")]
    public PolygonCollider2D patrolAreaCollider;  // 限制敌人生成区域的PolygonCollider2D

    [Header("生成数量")]
    public int numberOfEnemies = 10;  // 生成敌人的数量

    [Header("敌人容器")]
    public Transform dronesContainer;  // 生成的敌人将被放置在该容器下

    private List<GameObject> enemies = new List<GameObject>();  // 存储生成的敌人

    // Start is called before the first frame update
    void Start()
    {
        dronesContainer = GameObject.Find("RobotContainer").transform;
        patrolAreaCollider = GameObject.Find("RobotPatrolArea").GetComponent<PolygonCollider2D>();
        if (patrolAreaCollider != null && enemyPrefab != null && dronesContainer != null)
        {
            GenerateEnemies();
        }
    }
    // 生成敌人
    void GenerateEnemies()
    {
        // 获取巡逻区域的多边形顶点
        Vector2[] patrolAreaPoints = patrolAreaCollider.points;
        
        for (int i = 0; i < numberOfEnemies; i++)
        {
            // 随机生成位置
            Vector2 randomPoint = GetRandomPointInPolygon(patrolAreaPoints);
            Vector2 randomStartPatrolPos = GetRandomPointInPolygon(patrolAreaPoints);
            // 将局部坐标转换为世界坐标
            Vector3 worldPosition = patrolAreaCollider.transform.TransformPoint(randomPoint);
            Vector3 randomStartPatrolPos3 = patrolAreaCollider.transform.TransformPoint(randomStartPatrolPos);
            // 实例化敌人并存储，将其设置为 dronesContainer 的子对象
            GameObject enemy = Instantiate(enemyPrefab, worldPosition, Quaternion.identity);
            enemy.transform.SetParent(dronesContainer);  // 将敌人放到 dronesContainer 下

            // 设置敌人的初始方向
            EnemyBotAIpolygon enemyScript = enemy.GetComponent<EnemyBotAIpolygon>();
            if (enemyScript != null)
            {
                // 计算生成位置和目标点之间的方向
                Vector2 directionToTarget = (randomPoint - (Vector2)enemy.transform.position).normalized;
                // Debug.Log("Enemy initial direction: " + randomPoint+" "+(Vector2)enemy.transform.position);
                // Debug.Log("Enemy initial direction: " + worldPosition);
                // 设置敌人的初始方向
                enemyScript.initialDirection = directionToTarget;
            }

            enemies.Add(enemy);
        }
    }



    // 获取多边形区域内的随机点（局部坐标系）
    Vector2 GetRandomPointInPolygon(Vector2[] polygonPoints)
    {
        // 获取多边形的包围框
        float minX = Mathf.Infinity, maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity, maxY = -Mathf.Infinity;

        foreach (var point in polygonPoints)
        {
            if (point.x < minX) minX = point.x;
            if (point.x > maxX) maxX = point.x;
            if (point.y < minY) minY = point.y;
            if (point.y > maxY) maxY = point.y;
        }

        // 随机生成一个点，并检查它是否在多边形内
        Vector2 randomPoint;
        do
        {
            randomPoint = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        } while (!IsPointInsidePolygon(randomPoint, polygonPoints));

        return randomPoint;
    }

    // 判断点是否在多边形内
    bool IsPointInsidePolygon(Vector2 point, Vector2[] polygon)
    {
        int j = polygon.Length - 1;
        bool inside = false;
        for (int i = 0; i < polygon.Length; i++)
        {
            if ((polygon[i].y > point.y) != (polygon[j].y > point.y) &&
                (point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) / (polygon[j].y - polygon[i].y) + polygon[i].x))
            {
                inside = !inside;
            }
            j = i;
        }
        return inside;
    }
}
