using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Splines; // Make sure this using statement is present
using UnityFactorySceneHDRP;
public class TrashEnemiesController : MonoBehaviour
{
    public static TrashEnemiesController instance;
    [Header("Spawning Settings")]
    [Tooltip("要生成的 Trash Robot Prefab")]
    public GameObject trashRobotPrefab; // 在 Inspector 中指定 Robot Prefab

    [Tooltip("要生成的 Trash Drone Prefab")]
    public GameObject trashDronePrefab; // 在 Inspector 中指定 Drone Prefab

    [Tooltip("生成 Prefab 的时间间隔（秒）")]
    public float spawnInterval = 3f;

    [Tooltip("生成的 Prefab 是否成为此对象的子对象？")]
    public bool makeChild = true; // 控制是否设置父对象

    [Tooltip("（可选）指定一个生成点，如果为 None，则在此对象位置生成")]
    public Transform spawnPoint;
    [SerializeField] public SplineContainer _spline; // (需要跟随的Spline路径容器)
    [SerializeField] private float _duration = 50f;

    public List<GameObject> TrashEnemies;
    public PolygonCollider2D finalPolygonCollider;
    private Coroutine _spawnCoroutine; // 用于存储协程的引用，方便管理
    private bool _spawnRobotNext = true; // 状态变量，用于决定下次生成哪个
    public Transform RebornTrashContainer;
    public TrashDiableBelt TrashDiableBelt;
    public SwitchTrashEnemyCamera SwitchTrashEnemyCamera;
    public float delayBeforeReborn = 5f;
    public float delayBetweenCameraAndReborn = 10f;
    public bool isTrashEnemiesMovable = false;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        // --- 重要：检查两个 Prefab 是否都已在 Inspector 中指定 ---
        if (trashRobotPrefab == null || trashDronePrefab == null)
        {
            Debug.LogError("错误：需要在 TrashEnemiesController 的 Inspector 中同时指定 'Trash Robot Prefab' 和 'Trash Drone Prefab'！", this);
            return; // 如果任一 Prefab 未指定，则不启动生成逻辑
        }

        // --- 确定生成点 ---
        if (spawnPoint == null)
        {
            spawnPoint = this.transform; // 如果没有指定生成点，就用自身 Transform
        }

        // --- 启动生成协程 ---
        // Debug.Log("开始交替生成 Robot 和 Drone...", this);
        _spawnCoroutine = StartCoroutine(SpawnRandomRoutine());
    }

    // 当此脚本或对象被禁用时调用
    private void OnDisable()
    {
        // 停止协程，防止在对象禁用后继续尝试生成
        if (_spawnCoroutine != null)
        {
            // Debug.Log("停止生成，因为对象被禁用。", this);
            StopCoroutine(_spawnCoroutine);
            _spawnCoroutine = null; // 清除引用
        }
    }

    /// <summary>
    /// 负责按时间间隔交替生成 Robot 和 Drone 的协程。
    /// </summary>
    private IEnumerator SpawnRandomRoutine() // 可以改个名字
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            GameObject prefabToInstantiate;

            // 随机选择一个
            if (Random.value < 0.5f) // 50% 概率选 Robot
            {
                prefabToInstantiate = trashRobotPrefab;
            }
            else // 50% 概率选 Drone
            {
                prefabToInstantiate = trashDronePrefab;
            }

            // ... 后续的检查、Instantiate 和 SetParent 逻辑与上面相同 ...
            if (prefabToInstantiate == null) { /* ... */ continue; }
            if (spawnPoint == null) { /* ... */ yield break; }
            GameObject newInstance = Instantiate(prefabToInstantiate, spawnPoint.position, Quaternion.identity);
            CustomSplineAnimateTrashEnemies trashEnemiesSpline = newInstance.GetComponent<CustomSplineAnimateTrashEnemies>();
            trashEnemiesSpline.SetSpline(_spline);
            trashEnemiesSpline.SetDuration(_duration);
            trashEnemiesSpline.ResetTime();
            TrashEnemies.Add(newInstance);
            if (makeChild) { newInstance.transform.SetParent(this.transform); }
            // Debug.Log($"已随机生成 '{newInstance.name}'。", newInstance);

            // 注意：随机生成时不需要切换 _spawnRobotNext 状态
        }
    }

    public void RebornTrashEnemies(){
        foreach (GameObject trashEnemy in TrashEnemies)
        {
            trashEnemy.transform.SetParent(RebornTrashContainer);
            trashEnemy.GetComponent<CustomSplineAnimateTrashEnemies>().isOnBelt = false;
            //机器人
            if(trashEnemy.GetComponent<EnemyBotAIpolygon_robot>() != null){
                EnemyBotAIpolygon_robot EnemyBotAIpolygon_robot = trashEnemy.GetComponent<EnemyBotAIpolygon_robot>();
                EnemyBotAIpolygon_robot.enabled = true;
                EnemyBotAIpolygon_robot.patrolAreaCollider = finalPolygonCollider;
                EnemyBotAIpolygon_robot.SetRebornCondition();
            }
            //无人机
            if(trashEnemy.GetComponent<EnemyBotAIpolygon_new>() != null){
                EnemyBotAIpolygon_new EnemyBotAIpolygon_new = trashEnemy.GetComponent<EnemyBotAIpolygon_new>();
                EnemyBotAIpolygon_new.enabled = true;
                EnemyBotAIpolygon_new.patrolAreaCollider = finalPolygonCollider;
                EnemyBotAIpolygon_new.SetRebornCondition();
            }       
        }
    }

    // Update 方法如果不需要可以保持为空
    void Update()
    {
        foreach (GameObject trashEnemy in TrashEnemies)
        {
            if(trashEnemy==null){
                TrashEnemies.Remove(trashEnemy);
            }
            else{
                continue;
            }
        }
        // if(Input.GetKeyDown(KeyCode.R)){
        //     StartCoroutine(RebornTrashEnemiesProcess());
        // }
    }
    public IEnumerator RebornTrashEnemiesProcess(){
        SwitchTrashEnemyCamera.SwitchToTrashEnemyCamera();
        yield return new WaitForSeconds(delayBeforeReborn);
        RebornTrashEnemies();
        StopCoroutine(_spawnCoroutine);
        TrashDiableBelt.DisablePortalCollider();
        yield return new WaitForSeconds(delayBetweenCameraAndReborn);
        isTrashEnemiesMovable = true;
    }
}