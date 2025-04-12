using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairController : MonoBehaviour
{
    public StairEnterTrigger StairEnterTrigger;
    public StairExitTrigger StairExitTrigger;
    public float initialZ = -0.7f;
    public float targetZ = 6.26f;
    public Camera mainCamera;
    public Transform stairCameraTransform;
    public Transform OnstairCameraTransform;
    public Transform initialCameraTransform;
    public Transform PlayerTransform;
    private BoxCollider2D stairCollider; // 从当前对象获取
    public bool isPlayerOnStair = false;
    private bool isInStairView = false;
    private bool wasEnterTriggeredLastFrame = false;
    private bool wasExitTriggeredLastFrame = false;
    private bool wasOnStairLastFrame = false; // 用于检测离开 stairCollider
    private float zVelocity = 0f; // 用于 SmoothDamp 的速度引用
    private Rigidbody2D playerRigidbody; // 玩家刚体
    private Transform currentCameraTarget; // 当前相机目标
    private bool isTransitioning = false; // 是否正在过渡

    void Start()
    {
        if (PlayerTransform == null)
        {
            PlayerTransform = GameObject.FindGameObjectWithTag("PlayerCharacter").transform;
        }
        if (mainCamera != null && initialCameraTransform != null)
        {
            mainCamera.transform.localPosition = initialCameraTransform.localPosition;
            mainCamera.transform.localRotation = initialCameraTransform.localRotation;
            currentCameraTarget = initialCameraTransform; // 初始化相机目标
        }
        // 获取玩家的刚体组件
        playerRigidbody = PlayerTransform.GetComponent<Rigidbody2D>();
        // 获取当前对象的 BoxCollider2D
        stairCollider = GetComponent<BoxCollider2D>();
        // 确保 stairCollider 是触发器
        if (stairCollider != null && !stairCollider.isTrigger)
        {
            Debug.LogWarning("StairController: stairCollider 应该是一个触发器 (Is Trigger = true)");
            stairCollider.isTrigger = true;
        }
    }

    void FixedUpdate()
    {
        bool enterTriggered = StairEnterTrigger != null && StairEnterTrigger.isPlayerInside();
        bool exitTriggered = StairExitTrigger != null && StairExitTrigger.isPlayerInside();
        bool onStairThisFrame = isPlayerOnStair;

        // 相机过渡逻辑
        if (enterTriggered && !wasEnterTriggeredLastFrame && !isInStairView)
        {
            TransitionTo(stairCameraTransform, () => isInStairView = true);
        }
        else if (!enterTriggered && wasEnterTriggeredLastFrame && isInStairView)
        {
            TransitionTo(OnstairCameraTransform, null);
        }
        else if (!exitTriggered && wasExitTriggeredLastFrame && isInStairView)
        {
            TransitionTo(OnstairCameraTransform, () => isInStairView = false);
        }
        else if (onStairThisFrame && !wasOnStairLastFrame && isInStairView)
        {
            TransitionTo(OnstairCameraTransform, null);
        }
        else if (!onStairThisFrame && wasOnStairLastFrame && isInStairView)
        {
            TransitionTo(stairCameraTransform, null);
        }

        // 玩家在楼梯上的 Z 轴插值
        if (isPlayerOnStair && PlayerTransform != null && stairCollider != null && playerRigidbody != null)
        {
            Vector3 playerPos = PlayerTransform.position;

            // 计算楼梯的范围（假设沿X轴或Y轴）
            Bounds bounds = stairCollider.bounds;
            float progress;
            if (bounds.size.x > bounds.size.y) // 水平楼梯
            {
                progress = Mathf.InverseLerp(bounds.min.x, bounds.max.x, playerPos.x);
            }
            else // 垂直楼梯
            {
                progress = Mathf.InverseLerp(bounds.min.y, bounds.max.y, playerPos.y);
            }
            // 计算目标Z值
            float targetZValue = Mathf.Lerp(targetZ, initialZ, progress);

            // 使用SmoothDamp平滑调整Z轴
            float smoothZ = Mathf.SmoothDamp(PlayerTransform.position.z, targetZValue, ref zVelocity, 0.05f);

            // 更新玩家位置（仅修改Z轴）
            PlayerTransform.position = new Vector3(playerPos.x, playerPos.y, smoothZ);
        }

        wasEnterTriggeredLastFrame = enterTriggered;
        wasExitTriggeredLastFrame = exitTriggered;
        wasOnStairLastFrame = onStairThisFrame; // 更新楼梯状态
    }

    private void TransitionTo(Transform target, System.Action onComplete)
    {
        // 如果已经是目标位置或正在过渡中，则不重复触发
        if (currentCameraTarget == target || isTransitioning) return;

        currentCameraTarget = target;
        StartCoroutine(SmoothTransition(target, onComplete));
    }

    IEnumerator SmoothTransition(Transform target, System.Action onComplete)
    {
        isTransitioning = true;
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = mainCamera.transform.localPosition;
        Quaternion startRot = mainCamera.transform.localRotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            mainCamera.transform.localPosition = Vector3.Lerp(startPos, target.localPosition, t);
            mainCamera.transform.localRotation = Quaternion.Lerp(startRot, target.localRotation, t);
            yield return null;
        }
        
        mainCamera.transform.localPosition = target.localPosition;
        mainCamera.transform.localRotation = target.localRotation;
        isTransitioning = false;
        onComplete?.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnStair = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerOnStair = false;
        }
    }
}