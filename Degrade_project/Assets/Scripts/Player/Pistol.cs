// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;
// using Minimalist.Quantity;
// public class Pistol : MonoBehaviour
// {
//     [Header("枪械属性")]
//     [SerializeField] private GameObject projectile; // 子弹预制体
//     [SerializeField] private Transform muzzle; // 枪口位置
//     [SerializeField] private float damage = 10f; // 伤害值
//     [SerializeField] private float fireRate = 1f; // 射速
    
//     [Header("屏息系统")]
//     [SerializeField] private float holdBreathTime = 1.5f; // 屏息持续时间
//     [SerializeField] private GameObject crosshairPrefab; // 准心预制体(UI预制体)
//     [SerializeField] private AudioClip breathHoldSound; // 屏息音效
//     [SerializeField] private AudioClip shootSound; // 射击音效
    
//     [Header("UI设置")]
//     [SerializeField] private Canvas uiCanvas; // UI画布引用
    
//     [Header("调试")]
//     [SerializeField] private bool debugMode = false; // 调试模式
    
//     // 私有变量
//     private Vector2 direction; // 射击方向
//     private float nextFireTime = 0f; // 下次射击时间
//     private bool isHoldingBreath = false; // 是否正在屏息
//     private GameObject currentCrosshair; // 当前准心对象
//     private AudioSource audioSource; // 音频源
//     private Coroutine breathCoroutine; // 屏息协程
//     private Camera mainCamera; // 主摄像机
//     private RotatingCamera rotatingCamera; // 旋转摄像机引用
//     private float targetAngle; // 目标旋转角度
//     private Vector3 worldCrosshairPosition; // 准心在世界中的位置
//     public QuantityBhv quantityBhv; 
//     public Transform ChargeDisplay;
//     private float remainingBreathTime; // 剩余屏息时间
//     public TextMeshProUGUI ChargeText;
//     // 组件引用和初始化
//     void Start()
//     {
//         // 禁用并启用gun_attack.cs脚本
//         GetComponent<Gun_attack>().enabled = false;
//         GetComponent<Gun_attack>().enabled = true;
//         // 获取或添加音频源
//         audioSource = GetComponent<AudioSource>();
//         if (audioSource == null)
//             audioSource = gameObject.AddComponent<AudioSource>();
            
//         // 获取主摄像机
//         mainCamera = Camera.main;
        
//         // 获取旋转摄像机组件
//         rotatingCamera = mainCamera.GetComponent<RotatingCamera>();
        
//         // 获取或创建UI画布
//         if (uiCanvas == null)
//         {
//             // 尝试查找现有的Canvas
//             uiCanvas = FindObjectOfType<Canvas>();
            
//             // 如果场景中没有Canvas，创建一个
//             if (uiCanvas == null)
//             {
//                 GameObject canvasObj = new GameObject("UICanvas");
//                 uiCanvas = canvasObj.AddComponent<Canvas>();
//                 uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
//                 canvasObj.AddComponent<CanvasScaler>();
//                 canvasObj.AddComponent<GraphicRaycaster>();
//             }
//         }
        
//         if (debugMode)
//         {
//             Debug.Log("Pistol初始化完成");
//             if (projectile == null) Debug.LogError("子弹预制体未设置!");
//             if (muzzle == null) Debug.LogError("枪口位置未设置!");
//             if (crosshairPrefab == null) Debug.LogError("准心预制体未设置!");
//         }
//     }
    
//     // 每帧更新
//     private void Update()
//     {
//         // 特殊条件检查，如果需要禁用枪械功能
//         // if(QuestUIManager.QuestManager.quests[0].isCompleted && 
//         //     !SwitchBridgeCamera.instance.isBridgeCameraSwitched){
//         //         return;
//         // }
        
//         // if(VillageNpcController.instance != null && VillageNpcController.instance.isTalking){
//         //     return;
//         // }
        
//         // 检查 ChargeText 是否已赋值
//         if (ChargeText != null)
//         {
//             // 获取当前的颜色
//             Color textColor = ChargeText.color;
            
//             // 根据 remainingBreathTime 设置透明度
//             if (remainingBreathTime > 0f)
//             {
//                 textColor.a = 1f; // 完全不透明
//             }
//             else
//             {
//                 textColor.a = 0f; // 完全透明
//             }
            
//             // 将修改后的颜色重新赋值给 ChargeText
//             ChargeText.color = textColor;
//         }

//         // 获取准确的鼠标世界坐标，考虑摄像机倾斜
//         Vector3 mouseWorldPosition = GetMouseWorldPosition();
        
//         // 保存世界坐标用于射击
//         worldCrosshairPosition = mouseWorldPosition;
        
//         // 计算从枪到鼠标的方向向量
//         direction = (mouseWorldPosition - transform.position).normalized;
        
//         // 计算角度（注意y在前，x在后，符合Atan2的参数顺序）
//         float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
//         // 应用旋转
//         if(isHoldingBreath) transform.rotation = Quaternion.Euler(0, 0, angle);
        
//         if (debugMode)
//             Debug.Log($"鼠标世界位置: {mouseWorldPosition}, 方向: {direction}, 角度: {angle}");
        
//         // 更新准心位置 - 关键修复：每帧更新准心位置为当前鼠标位置
//         UpdateCrosshairPosition();
        
//         // 保存当前的目标角度供射击使用
//         if(isHoldingBreath)
//             targetAngle = angle;
        
//         // 空格键控制   
//         if (Input.GetKeyDown(KeyCode.Space) && CanShoot())
//         {
            
//             StartHoldBreath();
//         }
        
//         if (Input.GetKeyUp(KeyCode.Space) && isHoldingBreath)
//         {
//             StopHoldBreath(true); // 结束屏息并射击

//         }
        
//         // 在Scene视图中绘制方向线
//         if (debugMode)
//             Debug.DrawRay(transform.position, direction * 5f, Color.red);
//     }
    
//     // 更新准心位置到当前鼠标位置
//     private void UpdateCrosshairPosition()
//     {
//         if (isHoldingBreath && currentCrosshair != null)
//         {
//             // 直接使用鼠标屏幕坐标
//             currentCrosshair.GetComponent<RectTransform>().position = Input.mousePosition;
            
//             if (debugMode)
//                 Debug.Log($"更新准心位置: {Input.mousePosition}");
//         }
//     }
    
//     // 获取准确的鼠标世界坐标，考虑摄像机倾斜
//     private Vector3 GetMouseWorldPosition()
//     {
//         // 创建射线，从摄像机发射到鼠标位置
//         Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        
//         // 考虑游戏平面的正确方向（由于摄像机倾斜45度）
//         // 创建一个与地面平行的平面，而不是与摄像机方向垂直
//         Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);
        
//         // 计算射线与游戏平面的交点
//         if (gamePlane.Raycast(ray, out float distance))
//         {
//             Vector3 worldPosition = ray.GetPoint(distance);
            
//             // 确保z坐标为0（2D游戏）
//             worldPosition.z = 0;
            
//             if (debugMode)
//                 Debug.Log($"原始鼠标位置: {Input.mousePosition}, 计算得到的世界位置: {worldPosition}");
                
//             return worldPosition;
//         }
        
//         // 如果计算失败，使用替代方法（但这种情况应该不会发生，因为我们使用的是固定平面）
//         Debug.LogWarning("射线投射到平面失败，使用备用方法");
//         Vector3 fallbackPosition = mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
//         fallbackPosition.z = 0;
        
//         return fallbackPosition;
//     }
    
//     // 检查是否可以射击
//     private bool CanShoot()
//     {
//         return Time.time >= nextFireTime && !isHoldingBreath;
//     }
    
//     // 开始屏息
//     // private void StartHoldBreath()
//     // {
//     //     isHoldingBreath = true;
        
//     //     // 播放屏息音效
//     //     if (breathHoldSound != null)
//     //         audioSource.PlayOneShot(breathHoldSound);
        
//     //     // 生成准心
//     //     SpawnCrosshair();
        
//     //     // 开始屏息计时
//     //     breathCoroutine = StartCoroutine(HoldBreathRoutine());
        
//     //     if (debugMode)
//     //         Debug.Log("开始屏息");
//     // }
//     // 修改 StartHoldBreath 方法
//     private void StartHoldBreath()
//     {
//         isHoldingBreath = true;
        
//         // 初始化剩余时间
//         remainingBreathTime = holdBreathTime;
        
//         // 更新 QuantityBhv 显示
//         if (quantityBhv != null)
//             quantityBhv.Amount = remainingBreathTime;
        
//         // 播放屏息音效
//         if (breathHoldSound != null)
//             audioSource.PlayOneShot(breathHoldSound);
        
//         // 生成准心
//         SpawnCrosshair();

//         // 开始屏息计时
//         breathCoroutine = StartCoroutine(HoldBreathRoutine());
        
//         if (debugMode)
//             Debug.Log("开始屏息");
//     }
//     // 结束屏息
//     // 修改 StopHoldBreath 方法
//     // private void StopHoldBreath(bool shouldShoot)
//     // {
//     //     if (!isHoldingBreath) return;
        
//     //     isHoldingBreath = false;
        
//     //     // 停止屏息协程
//     //     if (breathCoroutine != null)
//     //         StopCoroutine(breathCoroutine);
        
//     //     // 如果是手动松开空格键，只射击一次
//     //     if (shouldShoot)
//     //     {
//     //         Shoot();
//     //         if (debugMode)
//     //             Debug.Log("手动结束屏息，单发射击");
//     //     }
        
//     //     // 销毁准心
//     //     DestroyCrosshair();
        
//     //     if (debugMode)
//     //         Debug.Log($"结束屏息，手动射击: {shouldShoot}");
//     // }

//     // 修改 StopHoldBreath 方法
//     private void StopHoldBreath(bool shouldShoot)
//     {
//         if (!isHoldingBreath) return;
        
//         isHoldingBreath = false;
//         ChargeText.gameObject.SetActive(false);
//         // 停止屏息协程
//         if (breathCoroutine != null)
//             StopCoroutine(breathCoroutine);
        
//         // 如果是手动松开空格键，只射击一次
//         if (shouldShoot)
//         {
//             Shoot();
//             if (debugMode)
//                 Debug.Log("手动结束屏息，单发射击");
//         }
        
//         // 重置 QuantityBhv 显示
//         if (quantityBhv != null)
//             quantityBhv.Amount = 0f; // 或其他默认值，根据你的需求
        
//         // 销毁准心
//         DestroyCrosshair();
        
//         if (debugMode)
//             Debug.Log($"结束屏息，手动射击: {shouldShoot}");
//     }

    
//     // 屏息协程
//     // 修改 HoldBreathRoutine 协程
//     // private IEnumerator HoldBreathRoutine()
//     // {
//     //     yield return new WaitForSeconds(holdBreathTime);
        
//     //     // 时间结束，自动连发三发子弹
//     //     if (isHoldingBreath)
//     //     {
//     //         if (debugMode)
//     //             Debug.Log("屏息时间结束，自动连发三发子弹");
                
//     //         // 连发三发子弹
//     //         for (int i = 0; i < 3; i++)
//     //         {
//     //             Shoot();
//     //             // 在连发之间添加小间隔（可选）
//     //             yield return new WaitForSeconds(0.1f);
//     //         }
            
//     //         StopHoldBreath(false); // 注意这里传入 false，因为我们已经手动处理了射击
//     //     }
//     // }
//     // 修改 HoldBreathRoutine 协程
//     private IEnumerator HoldBreathRoutine()
//     {
//         float elapsedTime = 0f;
//         ChargeText.gameObject.SetActive(true);  
//         while (elapsedTime < holdBreathTime && isHoldingBreath)
//         {
//             elapsedTime += Time.deltaTime;
//             remainingBreathTime = holdBreathTime - elapsedTime;
            
//             // 更新 QuantityBhv 显示已经屏息的时间 x/100
//             if (quantityBhv != null)
//                 quantityBhv.Amount = elapsedTime/holdBreathTime*100;
            
//             yield return null; // 每帧更新
//         }
        
//         // 时间结束，自动连发三发子弹
//         if (isHoldingBreath)
//         {
//             if (debugMode)
//                 Debug.Log("屏息时间结束，自动连发三发子弹");
                
//             // 连发三发子弹
//             for (int i = 0; i < 3; i++)
//             {
//                 Shoot();
//                 yield return new WaitForSeconds(0.1f);
//             }
            
//             StopHoldBreath(false);
//         }
//         ChargeText.gameObject.SetActive(false);
//     }
//     // 生成准心 - 现在作为UI元素
//     private void SpawnCrosshair()
//     {
//         if (crosshairPrefab == null || uiCanvas == null) return;
        
//         // 直接使用当前鼠标位置
//         Vector3 mousePosition = Input.mousePosition;
        
//         // 实例化准心作为UI元素
//         currentCrosshair = Instantiate(crosshairPrefab, mousePosition, Quaternion.identity, uiCanvas.transform);
        
//         // 确保准心层级最高
//         currentCrosshair.transform.SetAsLastSibling();
        
//         if (debugMode)
//             Debug.Log($"生成UI准心在屏幕位置: {mousePosition}");
//     }
    
//     // 销毁准心
//     private void DestroyCrosshair()
//     {
//         if (currentCrosshair != null)
//         {
//             Destroy(currentCrosshair);
//             currentCrosshair = null;
            
//             if (debugMode)
//                 Debug.Log("销毁准心");
//         }
//     }
    
//     // 射击
//     private void Shoot()
//     {
//         if (projectile == null || muzzle == null) return;
        
//         // 更新下次射击时间
//         nextFireTime = Time.time + 1f / fireRate;
        
//         // 实例化子弹
//         GameObject bullet = Instantiate(projectile, muzzle.position, Quaternion.identity);
        
//         // 设置子弹方向
//         Bullet bulletScript = bullet.GetComponent<Bullet>();
//         if (bulletScript != null)
//         {
//             // 设置子弹方向和伤害
//             bulletScript.SetDirection(targetAngle);
//             bulletScript.damage = damage;
            
//             if (debugMode)
//                 Debug.Log($"发射子弹，角度: {targetAngle}, 位置: {muzzle.position}, 方向: {direction}");
//         }
//         else if (debugMode)
//         {
//             Debug.LogError("子弹预制体缺少Bullet组件!");
//         }
        
//         // 播放射击音效
//         if (shootSound != null)
//             audioSource.PlayOneShot(shootSound);
//     }
    
//     // 当脚本被禁用时
//     private void OnDisable()
//     {
//         // 确保清理状态
//         if (isHoldingBreath)
//             StopHoldBreath(false);
//     }
    
//     // 在编辑器中绘制辅助图形
//     // private void OnDrawGizmos()
//     // {
//     //     if (!debugMode || !Application.isPlaying) return;
        
//     //     // 绘制枪口位置
//     //     if (muzzle != null)
//     //     {
//     //         Gizmos.color = Color.red;
//     //         Gizmos.DrawWireSphere(muzzle.position, 0.1f);
//     //     }
        
//     //     // 绘制鼠标位置和准心位置
//     //     Vector3 mousePos = GetMouseWorldPosition();
//     //     Gizmos.color = Color.green;
//     //     Gizmos.DrawWireSphere(mousePos, 0.15f);
        
//     //     if (currentCrosshair != null)
//     //     {
//     //         Gizmos.color = Color.yellow;
//     //         Gizmos.DrawLine(transform.position, currentCrosshair.transform.position);
//     //     }
//     // }
// }

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Minimalist.Quantity;

public class Pistol : MonoBehaviour
{
    [Header("枪械属性")]
    [SerializeField] private GameObject projectile; // 子弹预制体
    [SerializeField] private Transform muzzle; // 枪口位置
    [SerializeField] private float damage = 10f; // 伤害值
    [SerializeField] private float fireRate = 1f; // 射速
    
    [Header("屏息系统")]
    [SerializeField] private float holdBreathTime = 1.5f; // 屏息持续时间
    [SerializeField] private GameObject crosshairPrefab; // 准心预制体(UI预制体)
    [SerializeField] private AudioClip breathHoldSound; // 屏息音效
    [SerializeField] private AudioClip shootSound; // 射击音效
    
    [Header("UI设置")]
    [SerializeField] private Canvas uiCanvas; // UI画布引用
    
    [Header("调试")]
    [SerializeField] private bool debugMode = false; // 调试模式
    
    // 私有变量
    private Vector2 direction; // 射击方向
    private float nextFireTime = 0f; // 下次射击时间
    private bool isHoldingBreath = false; // 是否正在屏息
    private GameObject currentCrosshair; // 当前准心对象
    private AudioSource audioSource; // 音频源
    private Coroutine breathCoroutine; // 屏息协程
    private Camera mainCamera; // 主摄像机
    private RotatingCamera rotatingCamera; // 旋转摄像机引用
    private float targetAngle; // 目标旋转角度
    private Vector3 worldCrosshairPosition; // 准心在世界中的位置
    public QuantityBhv quantityBhv; 
    public Transform ChargeDisplay;
    private float remainingBreathTime; // 剩余屏息时间
    public TextMeshProUGUI ChargeText;
    private float elapsedTime = 0f;

    void Start()
    {
        GetComponent<Gun_attack>().enabled = false;
        GetComponent<Gun_attack>().enabled = true;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        mainCamera = Camera.main;
        rotatingCamera = mainCamera != null ? mainCamera.GetComponent<RotatingCamera>() : null;
        
        if (uiCanvas == null)
        {
            uiCanvas = FindObjectOfType<Canvas>();
            if (uiCanvas == null)
            {
                GameObject canvasObj = new GameObject("UICanvas");
                uiCanvas = canvasObj.AddComponent<Canvas>();
                uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObj.AddComponent<CanvasScaler>();
                canvasObj.AddComponent<GraphicRaycaster>();
            }
        }
        
        if (debugMode)
        {
            Debug.Log("Pistol初始化完成");
            if (projectile == null) Debug.LogError("子弹预制体未设置!");
            if (muzzle == null) Debug.LogError("枪口位置未设置!");
            if (crosshairPrefab == null) Debug.LogError("准心预制体未设置!");
        }
    }

    private void Update()
    {
        if (ChargeText != null)
        {
            Color textColor = ChargeText.color;
            textColor.a = remainingBreathTime > 0f ? 1f : 0f;
            ChargeText.color = textColor;
        }

        Vector3 mouseWorldPosition = GetMouseWorldPosition();
        worldCrosshairPosition = mouseWorldPosition;
        direction = (mouseWorldPosition - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        if (isHoldingBreath) transform.rotation = Quaternion.Euler(0, 0, angle);
        
        if (debugMode)
            Debug.Log($"鼠标世界位置: {mouseWorldPosition}, 方向: {direction}, 角度: {angle}");
        
        UpdateCrosshairPosition();
        
        if (isHoldingBreath)
            targetAngle = angle;
        
        if (Input.GetKeyDown(KeyCode.Space) && CanShoot())
        {
            StartHoldBreath();
        }
        
        if (Input.GetKeyUp(KeyCode.Space) && isHoldingBreath)
        {
            StopHoldBreath(true);
        }
        
        if (debugMode)
            Debug.DrawRay(transform.position, direction * 5f, Color.red);
    }

    private void UpdateCrosshairPosition()
    {
        if (isHoldingBreath && currentCrosshair != null)
        {
            currentCrosshair.GetComponent<RectTransform>().position = Input.mousePosition;
            if (debugMode)
                Debug.Log($"更新准心位置: {Input.mousePosition}");
        }
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Plane gamePlane = new Plane(Vector3.forward, Vector3.zero);
            if (gamePlane.Raycast(ray, out float distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                worldPosition.z = 0;
                if (debugMode)
                    Debug.Log($"原始鼠标位置: {Input.mousePosition}, 计算得到的世界位置: {worldPosition}");
                return worldPosition;
            }
        }
        
        Debug.LogWarning("射线投射到平面失败，使用备用方法");
        Vector3 fallbackPosition = mainCamera != null ? mainCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10)) : Vector3.zero;
        fallbackPosition.z = 0;
        return fallbackPosition;
    }

    private bool CanShoot()
    {
        return Time.time >= nextFireTime && !isHoldingBreath;
    }

    private void StartHoldBreath()
    {
        isHoldingBreath = true;
        remainingBreathTime = holdBreathTime;
        
        if (quantityBhv != null)
            quantityBhv.Amount = remainingBreathTime;
        
        if (breathHoldSound != null && audioSource != null)
            audioSource.PlayOneShot(breathHoldSound);
        
        SpawnCrosshair();
        breathCoroutine = StartCoroutine(HoldBreathRoutine());
        
        if (debugMode)
            Debug.Log("开始屏息");
    }

    private void StopHoldBreath(bool shouldShoot)
    {
        if (!isHoldingBreath) return;
        
        isHoldingBreath = false;
        if (ChargeText != null)
            ChargeText.gameObject.SetActive(false);
        
        if (breathCoroutine != null)
            StopCoroutine(breathCoroutine);
        
        if (shouldShoot)
        {
            if (debugMode)
                Debug.Log($"屏息时间结束，elapsedTime: {elapsedTime}");
                
            int bulletCount = CalculateBulletCount(elapsedTime);
            if (debugMode)
                Debug.Log($"计算得到的子弹数量: {bulletCount}");
                
            // 启动协程来发射子弹，带有时间间隔
            StartCoroutine(ShootWithInterval(bulletCount));
        }
        
        if (quantityBhv != null)
            quantityBhv.Amount = 0f;
        
        DestroyCrosshair();
        
        if (debugMode)
            Debug.Log($"结束屏息，手动射击: {shouldShoot}");
    }

    // 新增协程：发射子弹并添加时间间隔
    private IEnumerator ShootWithInterval(int bulletCount)
    {
        for (int i = 0; i < bulletCount; i++)
        {
            Shoot();
            if (i < bulletCount - 1) // 如果不是最后一颗子弹，添加间隔
            {
                yield return new WaitForSeconds(0.1f); // 每颗子弹间隔0.1秒
            }
        }
    }

    private IEnumerator HoldBreathRoutine()
    {
        elapsedTime = 0f;
        if (ChargeText != null)
            ChargeText.gameObject.SetActive(true);
            
        while (elapsedTime < holdBreathTime && isHoldingBreath)
        {
            elapsedTime += Time.deltaTime;
            remainingBreathTime = holdBreathTime - elapsedTime;
            
            if (quantityBhv != null)
                quantityBhv.Amount = (elapsedTime / holdBreathTime) * 100;
            
            yield return null;
        }
        
        if (isHoldingBreath)
        {
            if (debugMode)
                Debug.Log($"屏息时间结束，elapsedTime: {elapsedTime}");
            StopHoldBreath(true);
        }
        if (ChargeText != null)
            ChargeText.gameObject.SetActive(false);
    }

    private int CalculateBulletCount(float elapsedTime)
    {
        float timePerBullet = 0.3f; // 每颗子弹所需时间，可以调整
        int bulletCount = Mathf.Max(1, Mathf.FloorToInt(elapsedTime / timePerBullet));
        return bulletCount;
    }

    private void SpawnCrosshair()
    {
        if (crosshairPrefab == null || uiCanvas == null) return;
        
        Vector3 mousePosition = Input.mousePosition;
        currentCrosshair = Instantiate(crosshairPrefab, mousePosition, Quaternion.identity, uiCanvas.transform);
        currentCrosshair.transform.SetAsLastSibling();
        
        if (debugMode)
            Debug.Log($"生成UI准心在屏幕位置: {mousePosition}");
    }

    private void DestroyCrosshair()
    {
        if (currentCrosshair != null)
        {
            Destroy(currentCrosshair);
            currentCrosshair = null;
            if (debugMode)
                Debug.Log("销毁准心");
        }
    }

    private void Shoot()
    {
        if (projectile == null || muzzle == null) return;
        
        nextFireTime = Time.time + 1f / fireRate;
        GameObject bullet = Instantiate(projectile, muzzle.position, Quaternion.identity);
        
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(targetAngle);
            bulletScript.damage = damage;
            if (debugMode)
                Debug.Log($"发射子弹，角度: {targetAngle}, 位置: {muzzle.position}, 方向: {direction}");
        }
        else if (debugMode)
        {
            Debug.LogError("子弹预制体缺少Bullet组件!");
        }
        
        if (shootSound != null && audioSource != null)
            audioSource.PlayOneShot(shootSound);
    }

    private void OnDisable()
    {
        if (isHoldingBreath)
            StopHoldBreath(false);
    }
}