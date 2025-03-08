using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 引入TextMeshPro命名空间
using UnityEngine.UI;

public class HeepAnimation : MonoBehaviour
{
    public static HeepAnimation instance;
    private Animator animator;

    public RuntimeAnimatorController firstAnimatorController; // First Animator Controller
    public RuntimeAnimatorController secondAnimatorController; // Second Animator Controller

    private bool isUsingSecondAnimator = false; // Tracks if switched to second Animator

    private Transform playerTransform;
    public float minimumActiveDistance = 1f;
    public GameObject timePiecePrefab;    // Reference to the TimePiece prefab
    public GameObject timePieceContainer; // Optional: Reference to TimePieceContainer for parenting
    public Vector2 spawnOffset = new Vector2(1f, 0f); // Offset to spawn TimePiece nearby
    public float fadeInDuration = 0.5f;   // Duration for the fade-in effect (adjustable)
    public List<Dialogue> HeepDialogues;  // 管理垃圾堆的旁白
    public bool isHeepDialogue = false;
    public bool hasHeepDialogueShown = false; // 新增标志，追踪对话是否已显示
    private GameObject TimePiecePosition;
    AudioSource audioSource;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        //存档问题
        PlayerPrefs.DeleteKey("HasHeepDialogueShown"); // 重置 Heep 对话状态
        // 加载对话是否已显示的状态，默认值为 0（false）
        // hasHeepDialogueShown = PlayerPrefs.GetInt("HasHeepDialogueShown", 0) == 1;
    }
    // 在对话结束时保存
    public void OnDialogueEnd() // 假设在对话结束时调用
    {
        hasHeepDialogueShown = true;
        PlayerPrefs.SetInt("HasHeepDialogueShown", 1); // 保存为 1（true）
        PlayerPrefs.Save(); // 确保立即写入磁盘
    }
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        TimePiecePosition = GameObject.Find("TimePiecePosition");
        animator = GetComponent<Animator>();

        // Set initial Animator Controller
        if (firstAnimatorController != null)
        {
            animator.runtimeAnimatorController = firstAnimatorController;
        }
        playerTransform = GameObject.Find("PlayerCharacter").transform;
    }

    void Update()
    {
        // Switch to second Animator Controller when condition met
        if (!isUsingSecondAnimator && VillageSceneController.instance.isTimeMachine)
        {
            SwitchToSecondAnimator();

            // VillageSceneController.instance.isTimeMachine = false;
        }

        if (isUsingSecondAnimator && VillageSceneController.instance.isTimeMachineMasked)
        {

            Vector2 playerPosition = playerTransform.position;
            Vector2 heepPosition = transform.position;
            // Debug.Log("distance" + Vector2.Distance(playerPosition, heepPosition));
            if (Vector2.Distance(playerPosition, heepPosition) < minimumActiveDistance)
            {
                animator.SetBool("IsForming", true);
                // 只有在对话未显示过时才触发
                if (!hasHeepDialogueShown && !isHeepDialogue)
                {
                    //垃圾堆振荡音效
                    audioSource.PlayOneShot(audioSource.clip);
                    isHeepDialogue = true;
                }
            }
        }
    }

    void SwitchToSecondAnimator()
    {
        if (secondAnimatorController != null)
        {
            SpawnTimePiece(); // Spawn TimePiece when animation completes
            animator.runtimeAnimatorController = secondAnimatorController;
            isUsingSecondAnimator = true;
        }
    }

    public void OnAnimationComplete()
    {
        animator.SetBool("IsComplete", true);

    }

    public void SpawnTimePiece()
    {
        if (timePiecePrefab != null)
        {
            // Ensure TimePieceContainer is assigned or found
            if (timePieceContainer == null)
            {
                timePieceContainer = GameObject.Find("TimePieceContainer");
                if (timePieceContainer == null)
                {
                    Debug.LogError("TimePieceContainer not found in the scene!");
                    return;
                }
            }

            // 获取 TimePiecePosition 上的 PolygonCollider2D 组件
            PolygonCollider2D polygonCollider = TimePiecePosition.GetComponent<PolygonCollider2D>();
            if (polygonCollider == null)
            {
                Debug.LogError("PolygonCollider2D not found on TimePiecePosition!");
                return;
            }

            // 获取碰撞体的顶点
            Vector2[] colliderPoints = polygonCollider.points;

            // 在多边形内部生成一个随机位置
            Vector2 randomPoint = GetRandomPointInsidePolygon(colliderPoints);

            // 生成位置
            Vector3 spawnPosition = TimePiecePosition.transform.position + new Vector3(randomPoint.x, randomPoint.y, 0);

            // Instantiate TimePiece as a child of TimePieceContainer
            GameObject spawnedTimePiece = Instantiate(
                timePiecePrefab,
                spawnPosition,
                Quaternion.identity,
                timePieceContainer.transform
            );
            
            // Get the SpriteRenderer and set initial transparency
            SpriteRenderer spriteRenderer = spawnedTimePiece.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0f; // Start fully transparent
                spriteRenderer.color = color;
            }

            // Start fade-in coroutine
            StartCoroutine(FadeInTimePiece(spawnedTimePiece));

            // Assign FetchItemController if using singleton
            TimePieceTrigger trigger = spawnedTimePiece.GetComponent<TimePieceTrigger>();
            if (trigger != null && FetchItemController.instance != null)
            {
                Debug.Log("Assigned FetchItemController to TimePieceTrigger.");
                trigger.controller = FetchItemController.instance;
            }

            Debug.Log("TimePiece spawned near HeepAnimation at position: " + spawnPosition);
        }
        else
        {
            Debug.LogError("TimePiece prefab is not assigned in the Inspector!");
        }
    }

    private Vector2 GetRandomPointInsidePolygon(Vector2[] polygonPoints)
    {
        // 计算多边形的包围盒
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector2 point in polygonPoints)
        {
            minX = Mathf.Min(minX, point.x);
            maxX = Mathf.Max(maxX, point.x);
            minY = Mathf.Min(minY, point.y);
            maxY = Mathf.Max(maxY, point.y);
        }

        // 在包围盒内生成随机点
        Vector2 randomPoint;
        do
        {
            randomPoint = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        } while (!IsPointInsidePolygon(randomPoint, polygonPoints));

        return randomPoint;
    }

    private bool IsPointInsidePolygon(Vector2 point, Vector2[] polygonPoints)
    {
        // 使用射线法判断点是否在多边形内
        int intersectionCount = 0;
        for (int i = 0; i < polygonPoints.Length; i++)
        {
            Vector2 p1 = polygonPoints[i];
            Vector2 p2 = polygonPoints[(i + 1) % polygonPoints.Length];

            // 检查射线与边的交点
            if (((p1.y > point.y) != (p2.y > point.y)) &&
                (point.x < (p2.x - p1.x) * (point.y - p1.y) / (p2.y - p1.y) + p1.x))
            {
                intersectionCount++;
            }
        }
        return (intersectionCount % 2 != 0); // 如果交点数为奇数，则点在多边形内
    }


    private IEnumerator FadeInTimePiece(GameObject timePiece)
    {
        SpriteRenderer spriteRenderer = timePiece.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found on TimePiece!");
            yield break;
        }

        float elapsed = 0f;
        Color startColor = spriteRenderer.color;
        Color targetColor = startColor;
        targetColor.a = 1f; // Fully opaque target

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            spriteRenderer.color = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        spriteRenderer.color = targetColor; // Ensure fully opaque at the end
        //生成碎片后让控制器扫描其子对象
        FetchItemController.instance.UpdateChilds();
    }
}