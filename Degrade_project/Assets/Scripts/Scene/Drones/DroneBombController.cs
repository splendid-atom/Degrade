using UnityEngine;

public class DroneBombController : MonoBehaviour
{
    [Header("Game Settings")]
    public GameObject droneBombPrefab;
    public Vector2 fieldSize = new Vector2(10f, 10f);
    public float gridSize = 1f;
    public float bombFrequency = 2f;
    public int bombCount = 3;
    public bool isVerticalBombing = true;

    private Transform player;
    private float nextBombTime = 0f;
    private float fieldMinX, fieldMaxX, fieldMinY, fieldMaxY;
    private int columns, rows;
    private bool lastWasVertical = false;

    [SerializeField] float explosionDamage = 50f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] LayerMask damageLayerMask;   // 伤害图层掩码
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Make sure the player has the tag 'Player'");
            return;
        }
        // 以当前 GameObject 为中心计算 field 范围
        Vector3 center = transform.position;

        fieldMinX = center.x - fieldSize.x / 2f;
        fieldMaxX = center.x + fieldSize.x / 2f;
        fieldMinY = center.y - fieldSize.y / 2f;
        fieldMaxY = center.y + fieldSize.y / 2f;

        columns = Mathf.FloorToInt(fieldSize.x / gridSize);
        rows = Mathf.FloorToInt(fieldSize.y / gridSize);
    }




    void Update()
    {
        if (Time.time >= nextBombTime)
        {
            if (lastWasVertical)
                SpawnHorizontalBombWave();
            else
                SpawnVerticalBombWave();

            lastWasVertical = !lastWasVertical;
            nextBombTime = Time.time + (1f / bombFrequency);
        }
    }


    void SpawnVerticalBombWave()
    {
        int playerGridX = Mathf.Clamp(
            Mathf.FloorToInt((player.position.x - fieldMinX) / gridSize),
            0, columns - 1
        );

        int startCol = Mathf.Max(0, playerGridX - bombCount / 2);
        int endCol = Mathf.Min(columns - 1, startCol + bombCount - 1);

        int safeCol = Random.Range(startCol, endCol + 1);

        for (int col = startCol; col <= endCol; col++)
        {
            if (col == safeCol) continue;

            float xPos = fieldMinX + col * gridSize + gridSize / 2f;
            bool fromBottom = Random.value > 0.5f;
            float yPos = fromBottom ? fieldMinY - 1f : fieldMaxY + 1f;
            Vector3 spawnPos = new Vector3(xPos, yPos, 0f);
            Vector3 direction = fromBottom ? Vector3.up : Vector3.down;

            SpawnDrone(spawnPos, direction);
        }
    }

    void SpawnHorizontalBombWave()
    {
        int playerGridY = Mathf.Clamp(
            Mathf.FloorToInt((player.position.y - fieldMinY) / gridSize),
            0, rows - 1
        );

        int startRow = Mathf.Max(0, playerGridY - bombCount / 2);
        int endRow = Mathf.Min(rows - 1, startRow + bombCount - 1);

        int safeRow = Random.Range(startRow, endRow + 1);

        for (int row = startRow; row <= endRow; row++)
        {
            if (row == safeRow) continue;

            float yPos = fieldMinY + row * gridSize + gridSize / 2f;
            bool fromLeft = Random.value > 0.5f;
            float xPos = fromLeft ? fieldMinX - 1f : fieldMaxX + 1f;
            Vector3 spawnPos = new Vector3(xPos, yPos, 0f);
            Vector3 direction = fromLeft ? Vector3.right : Vector3.left;

            SpawnDrone(spawnPos, direction);
        }
    }

    void SpawnDrone(Vector3 position, Vector3 direction)
    {
        GameObject drone = Instantiate(droneBombPrefab, position, Quaternion.identity);
        DronesBombard bombardScript = drone.GetComponent<DronesBombard>();
        bombardScript.presetVariables(explosionDamage, explosionRadius, damageLayerMask);
        drone.transform.SetParent(transform, true); // 设置为本对象子物体，保持世界坐标
        if (bombardScript != null)
        {
            bombardScript.isAwakeBomb = false; // 不在 Start() 中自动启动
            bombardScript.StartBombingRun(
                direction,
                20f,    // bombardLength
                0.5f,   // dropInterval
                2.0f,   // bombFuseTime
                5f,     // speed
                10f,    // postBombardDistance
                1.0f    // fadeDuration
            );
        }
        else
        {
            Debug.LogWarning("DronesBombard script not found on prefab!");
        }
    }
}
