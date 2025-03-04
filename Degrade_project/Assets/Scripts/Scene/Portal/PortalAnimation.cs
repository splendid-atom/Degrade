using System.Collections;
using UnityEngine;

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

        // Instantiate the timepiece
        GameObject spawnedTimePiece = Instantiate(
            timePiecePrefab,
            transform.position, // Spawn at portal’s position
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
}