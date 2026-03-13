using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq; // 需要用到Linq来方便地随机选取

public class FloorCollapseController : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private string floorTag = "Floor"; // 地板块使用的标签
    [SerializeField] private GameObject warningPrefab; // 预警图标的预制件
    [SerializeField] private float warningDuration = 2.0f; // 预警显示时间
    [SerializeField] private float collapseDepth = 5.0f; // 地板下降的深度 (Z轴负方向)
    [SerializeField] private float collapseSpeed = 10.0f; // 地板下降的速度
    [SerializeField] private float restoreDelay = 10.0f; // 地板恢复的延迟时间 (如果需要恢复)
    [SerializeField] private bool canRestore = true; // 地板是否会自动恢复

    [Header("运行时参数")]
    [SerializeField] private int collapseCount = 2; // 每次塌陷的数量 (可由BossController修改)
    [SerializeField] private float triggerInterval = 8.0f; // 自动触发塌陷的时间间隔 (如果需要自动触发)
    [SerializeField] public bool autoTrigger = true; // 是否自动按间隔触发

    private List<FloorTile> allFloorTiles = new List<FloorTile>(); // 存储所有地板信息
    private List<FloorTile> currentlyCollapsedTiles = new List<FloorTile>(); // 记录当前已塌陷的地板
    private float autoTriggerTimer = 0f;

    void Start()
    {
        // 找到场景中所有带特定标签的地板块
        GameObject[] floorObjects = GameObject.FindGameObjectsWithTag(floorTag);
        foreach (GameObject obj in floorObjects)
        {
            allFloorTiles.Add(new FloorTile(obj.transform));
        }
        Debug.Log($"找到了 {allFloorTiles.Count} 个地板块。");

        if (warningPrefab == null) Debug.LogWarning("未指定地板塌陷预警 Prefab!");
    }

    void Update()
    {
        if (!this.enabled) return; // 如果脚本被BossController禁用，则不执行

        if (autoTrigger)
        {
            autoTriggerTimer += Time.deltaTime;
            if (autoTriggerTimer >= triggerInterval)
            {
                autoTriggerTimer = 0f;
                TriggerRandomCollapse();
            }
        }
    }

    // 由BossController调用，用于设置塌陷参数
    public void SetCollapseParameters(int count, float warningTime, float interval = -1f)
    {
        this.collapseCount = count;
        this.warningDuration = warningTime;
        if (interval > 0)
        {
            this.triggerInterval = interval;
        }
         Debug.Log($"地板塌陷参数更新: 数量={count}, 预警={warningTime}秒");
    }


    // 触发一次随机地板塌陷
    public void TriggerRandomCollapse()
    {
        // 筛选出当前可用的地板块 (未塌陷且不在预警中)
        List<FloorTile> availableTiles = allFloorTiles.Where(tile => tile.isAvailable && !tile.isWarning).ToList();

        if (availableTiles.Count == 0)
        {
            Debug.LogWarning("没有可用的地板块进行塌陷了！");
            return;
        }

        // 随机选择指定数量的地板块
        int countToCollapse = Mathf.Min(collapseCount, availableTiles.Count);
        System.Random rng = new System.Random();
        List<FloorTile> tilesToCollapse = availableTiles.OrderBy(x => rng.Next()).Take(countToCollapse).ToList();

        Debug.Log($"准备塌陷 {tilesToCollapse.Count} 个地板块...");

        // 对选中的地板块启动预警和塌陷流程
        foreach (FloorTile tile in tilesToCollapse)
        {
            StartCoroutine(CollapseSequence(tile));
        }
    }

    // 单个地板块的预警 -> 塌陷 -> (可选)恢复 协程
    private IEnumerator CollapseSequence(FloorTile tile)
    {
        tile.isWarning = true; // 标记为预警状态

        // 1. 显示预警
        GameObject warningInstance = null;
        if (warningPrefab != null)
        {
            // 在地板上方一点的位置生成预警图标
            Vector3 warningPos = tile.originalPosition + Vector3.back * 0.2f; // Y轴向上一点，避免和地板重叠
            warningInstance = Instantiate(warningPrefab, warningPos, Quaternion.identity, tile.tileTransform); // 作为子对象方便管理
        }
        // TODO: 可以让预警图标闪烁或播放动画

        yield return new WaitForSeconds(warningDuration);

        // 2. 销毁预警，开始塌陷
        if (warningInstance != null) Destroy(warningInstance);
        tile.isWarning = false;
        tile.isAvailable = false; // 标记为不可用 (已塌陷)
        currentlyCollapsedTiles.Add(tile); // 加入已塌陷列表
        Debug.Log($"地板块 {tile.tileTransform.name} 开始塌陷!");

        Vector3 targetPosition = tile.originalPosition + Vector3.forward * collapseDepth; // Z轴向后移动 (根据你的坐标系调整)
        Vector3 startPosition = tile.tileTransform.position;
        float timeElapsed = 0f;

        // 禁用碰撞体，防止玩家站在正在下落的地板上
        Collider floorCollider = tile.tileTransform.GetComponent<Collider>();
        if (floorCollider != null) floorCollider.enabled = false;

        while (timeElapsed < (collapseDepth / collapseSpeed))
        {
            tile.tileTransform.position = Vector3.Lerp(startPosition, targetPosition, (timeElapsed * collapseSpeed) / collapseDepth);
            timeElapsed += Time.deltaTime;
            yield return null; // 等待下一帧
        }
        tile.tileTransform.position = targetPosition; // 确保精确到达目标位置

        // 3. (可选) 安排恢复
        if (canRestore)
        {
            StartCoroutine(RestoreSequence(tile));
        }
    }

    // (可选) 地板块恢复协程
    private IEnumerator RestoreSequence(FloorTile tile)
    {
        yield return new WaitForSeconds(restoreDelay);

        Debug.Log($"地板块 {tile.tileTransform.name} 开始恢复!");
        Vector3 startPosition = tile.tileTransform.position;
        Vector3 targetPosition = tile.originalPosition;
        float distance = Vector3.Distance(startPosition, targetPosition);
        float timeElapsed = 0f;
        float restoreSpeed = collapseSpeed; // 可以用不同的恢复速度

         while (timeElapsed < (distance / restoreSpeed))
        {
            tile.tileTransform.position = Vector3.Lerp(startPosition, targetPosition, (timeElapsed * restoreSpeed) / distance);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        tile.tileTransform.position = targetPosition; // 确保精确恢复

        // 恢复碰撞体
        Collider floorCollider = tile.tileTransform.GetComponent<Collider>();
        if (floorCollider != null) floorCollider.enabled = true;

        tile.isAvailable = true; // 标记为可用
        currentlyCollapsedTiles.Remove(tile); // 从已塌陷列表中移除
    }


    // 用于存储地板块状态的辅助类
    private class FloorTile
    {
        public Transform tileTransform;
        public Vector3 originalPosition;
        public bool isAvailable = true; // 是否未塌陷
        public bool isWarning = false; // 是否处于预警状态

        public FloorTile(Transform transform)
        {
            this.tileTransform = transform;
            this.originalPosition = transform.position;
        }
    }

    // 可以在Boss战结束时调用，恢复所有地板
    public void RestoreAllTilesImmediately()
    {
         StopAllCoroutines(); // 停止所有正在进行的塌陷和恢复

         foreach(var tile in allFloorTiles)
         {
             if(!tile.isAvailable || tile.isWarning)
             {
                 if (tile.isWarning)
                 {
                    // 如果有预警图标，需要找到并销毁
                    Transform warning = tile.tileTransform.Find(warningPrefab.name + "(Clone)"); // 查找子对象
                    if(warning != null) Destroy(warning.gameObject);
                 }

                 tile.tileTransform.position = tile.originalPosition;
                 Collider floorCollider = tile.tileTransform.GetComponent<Collider>();
                 if (floorCollider != null) floorCollider.enabled = true;
                 tile.isAvailable = true;
                 tile.isWarning = false;
             }
         }
         currentlyCollapsedTiles.Clear();
         Debug.Log("所有地板块已立即恢复。");
    }
}