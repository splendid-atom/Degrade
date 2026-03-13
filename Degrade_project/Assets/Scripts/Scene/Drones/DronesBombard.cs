using UnityEngine;
using System.Collections;

public class DronesBombard : MonoBehaviour
{
    [Header("Bomb Settings")]
    [Tooltip("The prefab of the bomb to drop.")]
    public GameObject bombPrefab;
    [Tooltip("The prefab for the visual explosion effect (Particle System, etc.).")]
    public GameObject explosionEffectPrefab;

    [SerializeField] float defaultBombFuseTime = 2.0f;
    [SerializeField] float defaultDropInterval = 0.5f;
    [SerializeField] float defaultBombardLength = 20f;
    [SerializeField] float defaultBombardSpeed = 5f;
    [SerializeField] float defaultPostBombardDistance = 10f;
    [SerializeField] float defaultFadeDuration = 1.0f;
    [SerializeField] float explosionDamage = 50f;
    [SerializeField] float explosionRadius = 3f;
    [SerializeField] LayerMask damageLayerMask;   // 伤害图层掩码
    private Vector3 direction;
    private float bombardLength;
    private float bombardSpeed;
    private float dropInterval;
    private float bombFuseTime;
    private float postBombardDistance;
    private float fadeDuration;
    private float distanceTraveled = 0f;
    private float timeSinceLastDrop = 0f;
    private bool isBombing = false;
    private bool isPostBombing = false;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    public bool isAwakeBomb = true;
    // Inside DronesBombard.cs
    public void presetVariables(float damage, float radius, LayerMask mask) // Renamed parameters for clarity (optional but good practice)
    {
        // Assign the parameter values to the class member fields
        this.explosionDamage = damage;
        this.explosionRadius = radius;
        this.damageLayerMask = mask;

        // --- OR ---
        // Keep original parameter names and use 'this.' explicitly
        // this.explosionDamage = explosionDamage;
        // this.explosionRadius = explosionRadius;
        // this.damageLayerMask = damageLayerMask;
    }
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator component not found on the drone!", this);
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the drone! Fade-out will not work.", this);
        }

        if(isAwakeBomb){
            // 使用默认值开始轰炸
            StartBombingRun();            
        }

    }

    void Update()
    {
        if (isBombing)
        {
            PerformBombingRun();
        }
        else if (isPostBombing)
        {
            PerformPostBombingFlight();
        }
    }

    void UpdateAnimatorDirection(Vector2 moveDirection)
    {
        if (animator == null) return;

        moveDirection.Normalize();
        float moveX = (moveDirection.x + 1) / 2f;
        animator.SetFloat("Move X", moveX);
        animator.SetFloat("Move Y", moveDirection.y);
    }

    // ✅ 重载方法，使用默认值启动
    public void StartBombingRun()
    {
        StartBombingRun(Vector3.right, defaultBombardLength, defaultDropInterval,
            defaultBombFuseTime, defaultBombardSpeed, defaultPostBombardDistance,
            defaultFadeDuration);
    }

    // ✅ 主方法，接收所有参数
    public void StartBombingRun(Vector3 moveDirection, float bombardLength,
        float dropInterval, float bombFuseTime, float bombardSpeed,
        float postBombardDistance, float fadeDuration)
    {
        if (bombPrefab == null)
        {
            Debug.LogError("Bomb Prefab is not assigned in DronesBombard script!", this);
            return;
        }
        if (explosionEffectPrefab == null)
        {
            Debug.LogWarning("Explosion Effect Prefab is not assigned. Bombs will explode without visuals.", this);
        }

        // Debug.Log("Starting bombing run!");
        isBombing = true;
        distanceTraveled = 0f;
        timeSinceLastDrop = 0f;

        // 设置参数
        this.direction = moveDirection.normalized;
        this.bombardLength = bombardLength;
        this.dropInterval = dropInterval;
        this.bombFuseTime = bombFuseTime;
        this.bombardSpeed = bombardSpeed;
        this.postBombardDistance = postBombardDistance;
        this.fadeDuration = fadeDuration;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle = (angle == 90f ||angle == -90f) ? 0f : angle;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
        Vector3 scale = transform.localScale;

        // X轴处理：让朝向对应翻转
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x != 0 ? direction.x : 1f);

        // Y轴处理：如果从右往左飞则翻转y轴
        scale.y = Mathf.Abs(scale.y) * (direction.x < 0 ? -1f : 1f);

        transform.localScale = scale;


        UpdateAnimatorDirection(new Vector2(direction.x, direction.y));
    }

    private void PerformBombingRun()
    {
        if (distanceTraveled >= bombardLength)
        {
            // Debug.Log("Bombing run finished. Starting post-bombing flight with fade-out.");
            isBombing = false;
            isPostBombing = true;
            distanceTraveled = 0f;
            return;
        }

        float moveDistanceThisFrame = bombardSpeed * Time.deltaTime;
        transform.position += direction * moveDistanceThisFrame;
        distanceTraveled += moveDistanceThisFrame;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x != 0 ? direction.x : 1f);
        transform.localScale = scale;

        UpdateAnimatorDirection(new Vector2(direction.x, direction.y));

        timeSinceLastDrop += Time.deltaTime;
        if (timeSinceLastDrop >= dropInterval)
        {
            DropBomb();
            timeSinceLastDrop = 0f;
        }
    }

    private void PerformPostBombingFlight()
    {
        if (distanceTraveled >= postBombardDistance)
        {
            // Debug.Log("Post-bombing flight finished. Destroying drone.");
            Destroy(gameObject);
            return;
        }

        float moveDistanceThisFrame = bombardSpeed * Time.deltaTime;
        transform.position += direction * moveDistanceThisFrame;
        distanceTraveled += moveDistanceThisFrame;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x != 0 ? direction.x : 1f);
        transform.localScale = scale;

        UpdateAnimatorDirection(new Vector2(direction.x, direction.y));

        if (spriteRenderer != null)
        {
            float progress = distanceTraveled / postBombardDistance;
            float alpha = Mathf.Lerp(1f, 0f, progress * (postBombardDistance / (bombardSpeed * fadeDuration)));
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
        }
    }

    private void DropBomb()
    {
        if (bombPrefab != null)
        {
            GameObject bombInstance = Instantiate(bombPrefab, transform.position, Quaternion.identity);
            // 设为 DroneBombController（= 父对象）下的子对象
            bombInstance.transform.SetParent(transform.parent, true); // drone.parent 就是 DroneBombController
            BombController bombController = bombInstance.GetComponent<BombController>();
            if (bombController != null)
            {
                bombController.Initialize(bombFuseTime, explosionEffectPrefab
                ,explosionDamage,explosionRadius,damageLayerMask);
            }
            else
            {
                Debug.LogWarning("Instantiated bomb prefab does not have a BombController script attached!", bombInstance);
            }
            // Debug.Log($"Bomb dropped at {transform.position}");
        }
    }
}
