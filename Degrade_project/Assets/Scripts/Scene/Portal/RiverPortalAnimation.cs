using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RiverPortalAnimation : MonoBehaviour
{
    public static RiverPortalAnimation instance;
    public GameObject riverPortal;
    private Animator animator;
    private AudioSource firstAudioSource;
    private AudioSource secondAudioSource;
    private AudioSource thirdAudioSource;
    public float fadeDuration = 0.05f;
    public Transform TrashContainer;
    public bool isPlayerApproach = false;
    private bool thirdAudioPlayed = false;

    private Coroutine fadeInCoroutine;

    public GameObject[] trashPrefabs; // 九个垃圾Prefab
    public GameObject timePiecePrefab;
    public GameObject timePieceContainer;
    
    public Vector3 timePieceOffset = Vector3.zero;
    public float timePieceFadeInDuration = 1.0f;
    
    public float spawnInterval = 1.5f; // 垃圾生成间隔
    private bool isSpawningTrash = false; // 控制垃圾生成
    public List<Dialogue> PollutedRiverDialogues;  // 管理发现污染河流时的旁白
    public List<Dialogue> PollutedRiverReturnDialogues;  // 管理发现回到原时空时的旁白
    public bool isTalking = false;// 是否正在对话
    public bool isSecondTalking = false;// 是否正在对话
    public bool hasRiverDialogueShown = false; // 新增标志，追踪对话是否已显示
    public bool hasSecondRiverDialogueShown = false; // 新增标志，追踪对话是否已显示
    private GameObject TimePiecePosition;

    void Awake()
    {
        instance = this;
        riverPortal = gameObject;
        
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

        secondAudioSource.loop = true;
    }
    // 在对话结束时保存
    public void OnDialogueEnd() // 假设在对话结束时调用
    {
        hasRiverDialogueShown = true;
        PlayerPrefs.SetInt("hasRiverDialogueShown", 1); // 保存为 1（true）
        PlayerPrefs.Save(); // 确保立即写入磁盘
    }
    // 在对话结束时保存
    public void OnSecondDialogueEnd() // 假设在对话结束时调用
    {
        hasSecondRiverDialogueShown = true;
        PlayerPrefs.SetInt("hasSecondRiverDialogueShown", 1); // 保存为 1（true）
        PlayerPrefs.Save(); // 确保立即写入磁盘
    }
    // 在对话结束时保存
    public void OnDialogueStart() // 假设在对话开始时调用
    {
        if(!isTalking&&!hasRiverDialogueShown){
            isTalking = true;
        }
    }

    void Start()
    {
        TimePiecePosition = GameObject.Find("TimePiecePosition");
        // TrashContainer = GameObject.Find("TrashContainer").transform;
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        if (!firstAudioSource.isPlaying && !secondAudioSource.isPlaying && fadeInCoroutine == null && !thirdAudioPlayed)
        {
            fadeInCoroutine = StartCoroutine(FadeIn(secondAudioSource));
        }
        if(hasRiverDialogueShown&&!hasSecondRiverDialogueShown&&!isSecondTalking){
            if(ItemManager.itemManager.GetItemAmount("时空碎片") == 2){
                isSecondTalking = true;
            }
        }
    }

    public void OnAnimationComplete()
    {
        animator.SetBool("IsOpening", false);
        animator.SetBool("IsLooping", true);
        // 开始生成垃圾
        if (!isSpawningTrash)
        {
            Debug.Log("Start Spawning Trash");
            StartCoroutine(SpawnTrashRoutine());
        }
    }

    public void OnAnimationClosing()
    {
        if (isPlayerApproach && !thirdAudioPlayed)
        {
            Debug.Log("Portal Closed");

            if (fadeInCoroutine != null)
            {
                StopCoroutine(fadeInCoroutine);
                fadeInCoroutine = null;
            }

            secondAudioSource.Stop();
            secondAudioSource.volume = 0;
            thirdAudioSource.PlayOneShot(thirdAudioSource.clip);
            thirdAudioPlayed = true;

            animator.SetBool("isClosing", true);
            animator.SetBool("IsOpening", false);
            animator.SetBool("IsLooping", false);
            // 生成TimePiece
            SpawnTimePiece();
            if(hasRiverDialogueShown&&!hasSecondRiverDialogueShown&&!isSecondTalking){
                if(ItemManager.itemManager.GetItemAmount("时空碎片") == 1){
                    isSecondTalking = true;
                }
            }
            QuestUIManager.QuestManager.CompleteTask("",5);
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
        fadeInCoroutine = null;
        FetchItemController.instance.UpdateChilds();
    }

    private IEnumerator SpawnTrashRoutine()
    {
        isSpawningTrash = true;

        while (!thirdAudioPlayed) // 只要传送门未完全关闭，持续生成垃圾
        {
            SpawnTrash();
            yield return new WaitForSeconds(spawnInterval);
        }

        isSpawningTrash = false;
    }

    private void SpawnTrash()
    {
        if (trashPrefabs.Length == 0)
        {
            Debug.LogError("No trash prefabs assigned!");
            return;
        }

        // 随机选择一个垃圾Prefab
        GameObject trashPrefab = trashPrefabs[Random.Range(0, trashPrefabs.Length)];

        // 生成位置（带一点随机偏移）
        Vector3 spawnPosition = transform.position + new Vector3(
            Random.Range(-2f, 2f),  // X 方向随机偏移
            Random.Range(-1f, -5f),   // Y 方向略微浮起
            Random.Range(-1f, 1f)   // Z 方向随机偏移
        );

        // 生成垃圾
        GameObject trash = Instantiate(
            trashPrefab,
            spawnPosition,
            Quaternion.Euler(0, 0, Random.Range(0f, 360f)), // 随机Z轴旋转
            TrashContainer // 设置父对象
        );

        // 让垃圾渐变显现
        StartCoroutine(FadeInTrash(trash));

        // 开始下降动画
        StartCoroutine(MoveTrashDown(trash));
    }

    private IEnumerator FadeInTrash(GameObject trash)
    {
        SpriteRenderer spriteRenderer = trash.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color color = spriteRenderer.color;
        float duration = 1.0f; // 渐变时长
        float elapsedTime = 0f;

        color.a = 0;
        spriteRenderer.color = color;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / duration);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 1;
        spriteRenderer.color = color;
    }

    private IEnumerator MoveTrashDown(GameObject trash)
    {
        float fallSpeed = Random.Range(1f, 3f); // 不同垃圾下降速度不同

        while (trash.transform.position.y > -30f)
        {
            trash.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }

        // 在销毁前让垃圾渐变消失
        StartCoroutine(FadeOutTrash(trash));
    }

    private IEnumerator FadeOutTrash(GameObject trash)
    {
        SpriteRenderer spriteRenderer = trash.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Destroy(trash);
            yield break;
        }

        Color color = spriteRenderer.color;
        float duration = 1.0f; // 渐变时长
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1 - (elapsedTime / duration));
            spriteRenderer.color = color;
            yield return null;
        }

        Destroy(trash);
    }


    public void SpawnTimePiece()
    {
        if (timePiecePrefab == null)
        {
            Debug.LogError("TimePiece prefab is not assigned!");
            return;
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

        // Vector3 spawnPosition = transform.position + timePieceOffset;
        GameObject spawnedTimePiece = Instantiate(
            timePiecePrefab, 
            spawnPosition,
            Quaternion.identity, 
            timePieceContainer.transform);
        
        SpriteRenderer spriteRenderer = spawnedTimePiece.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeInTimePiece(spriteRenderer));
        }
        else
        {
            Debug.LogError("TimePiece prefab is missing a SpriteRenderer component!");
        }

        TimePieceTrigger trigger = spawnedTimePiece.GetComponent<TimePieceTrigger>();
        if (trigger != null)
        {
            trigger.controller = FetchItemController.instance;
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

    private IEnumerator FadeInTimePiece(SpriteRenderer spriteRenderer)
    {
        Color color = spriteRenderer.color;
        float elapsedTime = 0f;

        color.a = 0;
        spriteRenderer.color = color;

        while (elapsedTime < timePieceFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / timePieceFadeInDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 1;
        spriteRenderer.color = color;
    }
}
