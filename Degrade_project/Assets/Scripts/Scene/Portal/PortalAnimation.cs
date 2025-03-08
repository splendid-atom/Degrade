using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class PortalAnimation : MonoBehaviour
{
    public static PortalAnimation instance;
    private Animator animator;
    private AudioSource firstAudioSource;
    private AudioSource secondAudioSource;
    private AudioSource thirdAudioSource;
    public float fadeDuration = 0.05f;
    // private GameObject DegradePortal;
    public bool isPlayerApproach = false;
    private bool thirdAudioPlayed = false; // Flag to ensure thirdAudioSource plays only once
    
    private Coroutine fadeInCoroutine; // Store the fade coroutine reference

    public GameObject timePiecePrefab;    // Reference to the TimePiece prefab
    public GameObject timePieceContainer; // Reference to the TimePieceContainer
    private GameObject DegradeBambooTimePiecePosition;
    public List<Dialogue> DegradeBambooDialogues;  // 管理每个对话的说话者和内容
    public List<Dialogue> SecondDegradeBambooDialogues;  // 管理每个对话的说话者和内容
    public bool isTalking = false;
    public bool isSecondTalking = false;
    public bool hasDegradeBambooDialogueShown = false; // 新增标志，追踪对话是否已显示
    public bool hasSecondDegradeBambooDialogueShown = false; // 新增标志，追踪对话是否已显示

    void Awake()
    {
        instance = this;

        // Get AudioSource components
        AudioSource[] audioSources = GetComponents<AudioSource>();
        if (audioSources.Length >= 3)
        {
            firstAudioSource = audioSources[0];
            secondAudioSource = audioSources[1];
            thirdAudioSource = audioSources[2];
        }
        else
        {
            Debug.LogError("Not enough AudioSource components attached!");
        }

        secondAudioSource.loop = true; // Set second audio to loop
    }

    void Start()
    {
        DegradeBambooTimePiecePosition = GameObject.Find("DegradeBambooTimePiecePosition");
        // DegradePortal = GameObject.Find("degradePortal");
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Start fade-in when first audio ends and second hasn’t started
        if (!firstAudioSource.isPlaying && !secondAudioSource.isPlaying && fadeInCoroutine == null && !thirdAudioPlayed)
        {
            fadeInCoroutine = StartCoroutine(FadeIn(secondAudioSource));
        }
    }

    public void OnAnimationComplete()
    {
        animator.SetBool("IsOpening", false);
        animator.SetBool("IsLooping", true);
    }

    public void OnAnimationClosing()
    {
        if (isPlayerApproach && !thirdAudioPlayed)
        {
            Debug.Log("Portal Closed");

            // Stop fade coroutine if running
            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }

            // Stop second audio and reset volume
            secondAudioSource.Stop();
            secondAudioSource.volume = 0;

            // Play third audio
            thirdAudioSource.PlayOneShot(thirdAudioSource.clip);

            thirdAudioPlayed = true; // Prevent replay

            animator.SetBool("isClosing", true);
            animator.SetBool("IsOpening", false);
            animator.SetBool("IsLooping", false);
            SpawnTimePiece();
            QuestUIManager.QuestManager.CompleteTask("",6);
            isSecondTalking = true;
        }
    }

    private IEnumerator FadeIn(AudioSource fadeInSource)
    {
        float targetVolume = fadeInSource.volume;
        float startVolume = 0f;
        fadeInSource.volume = startVolume;
        fadeInSource.Play();

        while (fadeInSource.volume < targetVolume)
        {
            if (thirdAudioPlayed)
            {
                fadeInSource.Stop();
                fadeInSource.volume = 0;
                yield break;
            }

            fadeInSource.volume += Time.deltaTime / fadeDuration;
            yield return null;
        }

        fadeInSource.volume = targetVolume;
        fadeInCoroutine = null; // Clear coroutine reference
        FetchItemController.instance.UpdateChilds();
    }

    public void SpawnTimePiece()
    {
        if (timePiecePrefab == null)
        {
            Debug.LogError("TimePiece prefab is not assigned!");
            return;
        }
        // 获取 TimePiecePosition 上的 PolygonCollider2D 组件
        PolygonCollider2D polygonCollider = DegradeBambooTimePiecePosition.GetComponent<PolygonCollider2D>();
        if (polygonCollider == null)
        {
            Debug.LogError("PolygonCollider2D not found on DegradeBambooTimePiecePosition!");
            return;
        }

        // 获取碰撞体的顶点
        Vector2[] colliderPoints = polygonCollider.points;

        // 在多边形内部生成一个随机位置
        Vector2 randomPoint = GetRandomPointInsidePolygon(colliderPoints);
        // 生成位置
        Vector3 spawnPosition = DegradeBambooTimePiecePosition.transform.position + new Vector3(randomPoint.x, randomPoint.y, 0);

        // Instantiate the timepiece
        GameObject spawnedTimePiece = Instantiate(
            timePiecePrefab,
            spawnPosition, // Spawn at portal’s position
            Quaternion.identity,
            timePieceContainer.transform // Optional: Set parent
        );

        // Get the TimePieceTrigger component and assign the controller
        TimePieceTrigger trigger = spawnedTimePiece.GetComponent<TimePieceTrigger>();
        if (trigger != null)
        {
            trigger.controller = FetchItemController.instance; // Assign the controller
        }
        else
        {
            Debug.LogError("TimePieceTrigger component missing on spawned TimePiece!");
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
    // 在对话结束时保存
    public void OnDialogueEnd() // 假设在对话结束时调用
    {
        hasDegradeBambooDialogueShown = true;
        PlayerPrefs.SetInt("hasDegradeBambooDialogueShown", 1); // 保存为 1（true）
        PlayerPrefs.Save(); // 确保立即写入磁盘
    }
    // 在对话结束时保存
    public void OnSecondDialogueEnd() // 假设在对话结束时调用
    {
        hasSecondDegradeBambooDialogueShown = true;
        PlayerPrefs.SetInt("hasSecondDegradeBambooDialogueShown", 1); // 保存为 1（true）
        PlayerPrefs.Save(); // 确保立即写入磁盘
    }
}