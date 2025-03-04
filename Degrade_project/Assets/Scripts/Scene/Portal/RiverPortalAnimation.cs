using System.Collections;
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
    
    public bool isPlayerApproach = false;
    private bool thirdAudioPlayed = false; // Flag to ensure thirdAudioSource plays only once
    
    private Coroutine fadeInCoroutine; // Store the fade coroutine reference

    public GameObject timePiecePrefab;    // Reference to the TimePiece prefab
    public GameObject timePieceContainer; // Reference to the TimePieceContainer

    public Vector3 timePieceOffset = Vector3.zero; // 用于设定 TimePiece 的生成位置偏移量
    public float timePieceFadeInDuration = 1.0f; // 渐变持续时间

    void Awake()
    {
        instance = this;
        riverPortal = gameObject;
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
        animator = GetComponent<Animator>();
        gameObject.SetActive(false);
    }

    void Update()
    {
        // Debug.Log("isSwitchingCamera:"+PollutedRiverCamera.instance.isSwitchingCamera);

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

        // 计算最终生成位置（带偏移量）
        Vector3 spawnPosition = transform.position + timePieceOffset;

        // Instantiate the timepiece
        GameObject spawnedTimePiece = Instantiate(
            timePiecePrefab,
            spawnPosition, // 生成时应用偏移量
            Quaternion.identity,
            timePieceContainer.transform // Optional: Set parent
        );

        // 获取 SpriteRenderer 组件
        SpriteRenderer spriteRenderer = spawnedTimePiece.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // 开始渐变显示
            StartCoroutine(FadeInTimePiece(spriteRenderer));
        }
        else
        {
            Debug.LogError("TimePiece prefab is missing a SpriteRenderer component!");
        }

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

    private IEnumerator FadeInTimePiece(SpriteRenderer spriteRenderer)
    {
        Color color = spriteRenderer.color;
        float elapsedTime = 0f;

        // 初始状态全透明
        color.a = 0;
        spriteRenderer.color = color;

        while (elapsedTime < timePieceFadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / timePieceFadeInDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        // 最终设为完全不透明
        color.a = 1;
        spriteRenderer.color = color;
    }
}