using UnityEngine;
using System.Collections;
using UnityEngine.Splines;
using UnityFactorySceneHDRP;
public class TopArmController : MonoBehaviour
{
    [Header("References")]
    public Transform WholeBody;
    [SerializeField] private Transform InitialPosition;
    public Camera mainCamera;
    public BoxCollider2D roomCollider2D;
    public TopArmContainerCollider TopArmContainerCollider;
    public TopArmCatcherCollider TopArmCatcherCollider;
    public Transform CatcherTransform;
    public Transform TopArmContainer;
    public AudioSource audioSource;      // 挂载的 AudioSource
    public AudioClip clipDown;               // 第一个音效
    public AudioClip clipUp;                 // 第二个音效
    [SerializeField] private SplineContainer _spline;
    [SerializeField] private SplineContainer target_spline;
    [SerializeField] private GameObject caughtObject; // 存储被抓取的对象

    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("FOV Settings")]
    public float fovTransitionSpeed = 3f;
    public float targetEnterFOV = 56f;
    // public float TrashDuration = 10f;
    public float TrashDurationEnterPortal = 10f;
    private float originalFOV;
    private float targetFOV;
    private bool isPlayerInsideCollider = false;

    [Header("State Flags")]
    [SerializeField] private bool isCatached = false;
    [SerializeField] private bool isCataching = false;
    private bool isMoving = false;

    private Vector3 targetPosition;
    private Animator animator;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        originalFOV = mainCamera.fieldOfView;
        targetFOV = originalFOV;
        animator = GetComponent<Animator>();
        targetPosition = WholeBody.position;

        isPlayerInsideCollider = TopArmContainerCollider.IsPlayerInside();
        UpdateTargetFOV();
    }

    void Update()
    {
        bool currentInsideState = TopArmContainerCollider.IsPlayerInside();
        if (currentInsideState != isPlayerInsideCollider)
        {
            isPlayerInsideCollider = currentInsideState;
            UpdateTargetFOV();
        }

        HandleInput();

        if (isCatached)
        {
            MoveTo(InitialPosition.position);
        }

        if (isMoving)
        {
            MoveTowardsTarget();
        }

        SmoothFOVTransition();

        // 如果有被抓取的对象，让它跟随 CatcherTransform
        if (caughtObject != null)
        {
            caughtObject.transform.position = CatcherTransform.position;
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0) && !isCatached && 
        !isMoving && TopArmContainerCollider.IsPlayerInside())
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                MoveTo(hit.point);
            }
        }
    }

    private void MoveTo(Vector3 position)
    {
        targetPosition = position;
        targetPosition.z = WholeBody.position.z;
        isMoving = true;
    }

    private void MoveTowardsTarget()
    {
        Vector3 direction = targetPosition - WholeBody.position;
        direction.z = 0;

        float distance = direction.magnitude;

        if (distance > 0.5f)
        {
            WholeBody.position += direction.normalized * moveSpeed * Time.deltaTime;
        }
        else
        {
            isMoving = false;
            WholeBody.position = targetPosition;

            if (targetPosition != InitialPosition.position)
            {
                animator.SetBool("isCatching", true);
            }

            if (isCatached)
            {
                isCatached = false;
                animator.speed = 1;
            }
        }
    }

    private void SmoothFOVTransition()
    {
        if (Mathf.Abs(mainCamera.fieldOfView - targetFOV) > 0.1f)
        {
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, targetFOV, Time.deltaTime * fovTransitionSpeed);
        }
        else
        {
            mainCamera.fieldOfView = targetFOV;
        }
    }

    private void UpdateTargetFOV()
    {
        targetFOV = isPlayerInsideCollider ? targetEnterFOV : originalFOV;
    }

    public void OnSetCataching()
    {
        isCataching = true;
        Debug.Log("OnSetCataching");
    }
    public void OnPlayAudioSourceDown()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(clipDown);
        Debug.Log("OnPlayAudioSourceDown");
    }
    public void OnPlayAudioSourceUp()
    {
        audioSource.Stop();
        audioSource.PlayOneShot(clipUp);
        Debug.Log("OnPlayAudioSourceUp");
    }
    public void OnDisableCataching()
    {
        isCataching = false;
        GameObject lastObject = TopArmCatcherCollider.GetLastObjectInside();
        Debug.Log("lastObjectInside:" + (lastObject != null ? lastObject.name : "null"));

        if (lastObject != null)
        {
            // 1. Get the CustomSplineAnimate component from the GameObject
            CustomSplineAnimate splineAnimateComponent = lastObject.GetComponent<CustomSplineAnimate>();

            // 2. Check if the component actually exists on the object
            if (splineAnimateComponent != null)
            {
                Debug.Log("splineAnimateComponent:" + splineAnimateComponent.name);
                // 3. Now use the component reference to access its members]
                // splineAnimateComponent.SetSpline(target_spline);
                splineAnimateComponent.isOnBelt = false; // Disable movement on the old spline

                caughtObject = lastObject; // Store the GameObject reference
                caughtObject.transform.SetParent(null); // Unparent before moving smoothly
                StartCoroutine(CatchingObject());
            }
            else
            {
                // Optional: Log a warning if the object doesn't have the required script
                Debug.LogWarning($"The object '{lastObject.name}' being caught does not have a CustomSplineAnimate component.", lastObject);
                // Decide how to handle this case - maybe you don't want to catch objects without this script?
            }
        }
    }

    private IEnumerator CatchingObject()
    {
        if (caughtObject == null) yield break;

        float duration = 0.5f; // 移动时间
        float elapsed = 0f;
        TopArmCatcherCollider.EnableCatachingEffect();
        Vector3 startPos = caughtObject.transform.position;
        Vector3 endPos = CatcherTransform.position;
        while (elapsed < duration)
        {
            if (caughtObject == null) yield break; // 中途被释放了
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            caughtObject.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        // 到达后设置位置并成为子物体
        caughtObject.transform.position = CatcherTransform.position;
        caughtObject.transform.SetParent(CatcherTransform);
    }
    public void OnSetCatached()
    {
        animator.SetBool("isCatching", false);
        isCatached = true;
        animator.speed = 0;
    }

    public void OnSetReleased()
    {
        Debug.Log("OnSetReleased");
        if (caughtObject != null)
        {
            // 1. 解除父子关系，让物体可以自由移动
            //    可以选择父对象为 null 或者一个场景中的容器，取决于你的结构
            caughtObject.transform.SetParent(TopArmContainer); // 或者 SetParent(null);

            // 2. 获取或添加 CustomSplineAnimate 组件
            CustomSplineAnimate customSplineScript = caughtObject.GetComponent<CustomSplineAnimate>();
            if (customSplineScript == null)
            {
                customSplineScript = caughtObject.AddComponent<CustomSplineAnimate>();
                // 可能需要设置默认的 _duration 等参数
                // customSplineScript._duration = 5f; // 例如
            }

            // 3. 设置要跟随的新 Spline (target_spline)
            customSplineScript.SetSpline(target_spline); // 或者直接 customSplineScript._spline = target_spline;
            customSplineScript.SetDuration(TrashDurationEnterPortal);
            // 4. ⭐ 重置 Spline 的时间到起点 ⭐
            customSplineScript.ResetTime();
            Debug.Log($"Called ResetTime() for {caughtObject.name}.");

            // 4. 确保脚本启用，并让它开始在新的 Spline 上移动
            customSplineScript.enabled = true; // 确保脚本是激活的
            customSplineScript.isOnBelt = true; // ★ 明确告诉脚本开始循迹
            customSplineScript.IsMoving = true; // ★ 确保物体开始移动 (如果需要)
            // 你可能还需要重置 _time 到 0，或者根据 target_spline 的情况决定
            // customSplineScript.ResetTime(); // 你需要在 CustomSplineAnimate 中添加一个 public ResetTime() 方法

            // 5. 清除抓取记录
            caughtObject = null;

             // 6. (可选) 禁用抓取特效
            // TopArmCatcherCollider.DisableCatachingEffect(); // 如果有这个方法的话
        }
    }
}