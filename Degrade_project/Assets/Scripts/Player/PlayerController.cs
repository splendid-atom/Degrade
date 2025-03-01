using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    Rigidbody2D rigidbody2d;
    public float speed = 3.0f;
    public InputAction MoveActionWASD;
    Vector2 move;
    Vector2 globalmove;
    Animator animator;
    public Vector2 moveDirection = new Vector2(1, 0);
    public Transform arrowIndicator;  // 引用ArrowIndicator
    public int PlayerHealth = 100;    // 生命值
    public int PlayerShield = 100;    // 护甲值
    public int MaxHealth = 100;      // 最大生命值
    public int MaxShield = 200;      // 最大护甲值
    public int CurrentHoldingItem = 0;
    public string PlayerName;        // 玩家名字

    // 存储所有桥的碰撞体
    private List<Collider2D> bridgeColliders = new List<Collider2D>();

    // 设置一个桥面上起伏的幅度和频率
    public float waveFrequency = 0.5f;  // 起伏频率
    public float waveAmplitude = 0.5f;  // 起伏幅度

    private bool isOnBridge = false;  // 判断玩家是否在桥上
    private Collider2D activeBridge = null;
    private float zVelocity = 0f;  // 用于平滑过渡的速度变量
    // 新增变量：判断玩家是否在移动
    private bool isMoving = false;
    public float veticalFactor = 1.5f;
    private float verticalSpeed;
    public bool isDisableMovement = false;
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        MoveActionWASD.Enable();
        animator = GetComponent<Animator>();

        // 获取所有tag为"Bridge"的碰撞体
        bridgeColliders.Clear();
        GameObject[] bridges = GameObject.FindGameObjectsWithTag("Bridge");

        foreach (var bridge in bridges)
        {
            Collider2D bridgeCollider = bridge.GetComponent<Collider2D>();
            if (bridgeCollider != null)
            {
                bridgeColliders.Add(bridgeCollider);
            }
        }
    }

    private void Awake()
    {
        PlayerName = PlayerPrefs.GetString("PlayerName");
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);  // 确保实例化只发生一次
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(moveDirection);
        // Debug.Log("isDisableMovement:"+isDisableMovement);
        if(VillageNpcController.instance!=null){
            if (VillageNpcController.instance.isTalking)
            {
                return;
            }            
        }
        if(isDisableMovement){
            animator.SetFloat("Look X", 0.0f);
            animator.SetFloat("Look Y", 0.0f);
            animator.SetFloat("Speed", 0.0f);
            return;
        }
        move = MoveActionWASD.ReadValue<Vector2>();
        globalmove = move;
        // 判断是否在移动，更新isMoving
        isMoving = !Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f);

        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            moveDirection.Set(move.x, move.y);
            moveDirection.Normalize();
        }

        animator.SetFloat("Look X", moveDirection.x);
        animator.SetFloat("Look Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        // 打印玩家的移动方向
        // Debug.Log("Move Direction: " + moveDirection);
        // 确保ArrowIndicator的旋转与角色的移动方向一致
        if (arrowIndicator != null)
        {
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            arrowIndicator.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90));
        }
    }

    void FixedUpdate()
    {
        if(VillageNpcController.instance!=null){
            if (VillageNpcController.instance.isTalking)
            {
                return;
            }            
        }
        if(isDisableMovement){
            return;
        }


        // 使用角色自身的坐标系移动
        move = move.x * transform.right + move.y * transform.up;

        // 调整垂直方向的速度，只在按W或S时加速
        
        // Debug.Log($"(move.x,move.y): ({globalmove.x}, {globalmove.y})");

        // 判断是否为上下移动（只要 move.y 不为 0）
        if (Mathf.Abs(globalmove.y) > 0.7)  // 如果 move.y 非零，则认为是上下移动
        {
            verticalSpeed = speed * veticalFactor; // 提高垂直移动速度，例如增加50%
        }
        else
        {
            verticalSpeed = speed; // 保持水平移动的速度
        }

        // 计算玩家新位置
        Vector2 position = (Vector2)rigidbody2d.position + move * verticalSpeed * Time.deltaTime;

        if (isOnBridge && activeBridge != null && QuestUIManager.QuestManager.quests[0].isCompleted)
        {
            // 获取玩家当前位置的 X 值
            float playerX = transform.position.x;

            // 根据玩家的 X 值计算 Z 轴的位置，使用线性插值映射到 waveAmplitude 范围
            float bridgeStartX = activeBridge.bounds.min.x;
            float bridgeEndX = activeBridge.bounds.max.x;

            float normalizedX = Mathf.InverseLerp(bridgeStartX, bridgeEndX, playerX);  // 将玩家的 X 映射到 [0, 1] 范围
            float targetZ = Mathf.Sin(normalizedX * Mathf.PI * waveFrequency) * waveAmplitude;  // 使用更大的幅度增加起伏范围

            // 平滑过渡到目标 Z 位置，使用更高平滑因子
            float smoothZ = Mathf.SmoothDamp(transform.position.z, targetZ, ref zVelocity, 0.1f);  // 使用 SmoothDamp 确保平滑过渡

            position = new Vector2(position.x, position.y);  // 保持 XY 不变
            rigidbody2d.MovePosition(position);  // 移动到新的位置
            transform.position = new Vector3(position.x, position.y, smoothZ);  // 调整 Z 轴
        }
        else
        {
            // 普通移动
            rigidbody2d.MovePosition(position);

            // Z 轴回归到原始位置
            float smoothZ = Mathf.SmoothDamp(transform.position.z, 0f, ref zVelocity, 0.2f);  // 回到 Z = 0，使用平滑过渡
            transform.position = new Vector3(position.x, position.y, smoothZ);
        }
    }




    // 当玩家进入桥的碰撞体时
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (bridgeColliders.Contains(other))
        {
            isOnBridge = true;  // 玩家进入桥上，开始上下起伏
            activeBridge = other;
            // Debug.Log("玩家进入桥上");
        }
    }

    // 当玩家离开桥的碰撞体时
    private void OnTriggerExit2D(Collider2D other)
    {
        if (bridgeColliders.Contains(other))
        {
            isOnBridge = false;  // 玩家离开桥，停止上下起伏
            activeBridge = null;
            // Debug.Log("玩家离开桥");
        }
    }
    // 获取玩家是否在移动
    public bool IsMoving()
    {
        return isMoving;
    }
}
