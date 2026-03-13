using UnityEngine;
using System.Collections; // 需要用到协程
using Minimalist.Quantity;
using UnityEngine.Rendering.PostProcessing;
using System.Collections.Generic;
public class BossController : Enemy3
{
    [Header("核心属性")]
    // [SerializeField] private float maxHP = 1000f; // Boss最大血量
    // [SerializeField] private float currentHP; // Boss当前血量
    public int currentPhase = 0; // 当前阶段 (0: 未开始, 1, 2, 3, 4)
    [SerializeField] private float phase2Threshold = 0.7f; // 进入阶段2的血量百分比
    [SerializeField] private float phase3Threshold = 0.3f; // 进入阶段3的血量百分比
    [SerializeField] private float phase4Threshold = 0.1f; // 进入阶段4的血量百分比
    [SerializeField] public bool isInvincible = false; // Boss是否无敌 (阶段4)
    [SerializeField] public bool isStunned = false; // Boss是否处于眩晕状态 (破盾后)
    [SerializeField] public float shieldStunDuration = 3.0f; // 破盾后的眩晕时间

    [Header("组件引用")]
    [SerializeField] private Animator bossAnimator; // Boss的动画控制器 (如果scientistAnimation内部没有获取，这里可以保留)
    // ▼▼▼ 修改点 ▼▼▼
    [SerializeField] private scientistAnimation bossAnimationController; // Boss的动画控制脚本 (替换 DronesAnimation)

    [SerializeField] private GameObject TrashEnemiesContainerSmall;
    // ▲▲▲ 修改点 ▲▲▲
    [SerializeField] private TrapGenerator trapGenerator; // 陷阱生成器脚本
    [SerializeField] private WaveParticleEmitter waveParticleEmitter; // 火球波发射器脚本
    [SerializeField] private DroneBombController droneBombController; // 无人机轰炸控制器脚本
    [SerializeField] private SummonBots summonBots; // 机器人召唤脚本
    [SerializeField] private FloorCollapseController floorCollapseController; // 地板塌陷控制器脚本
    [SerializeField] private BossMovementController bossMovementController; // Boss移动控制器脚本
    [SerializeField] private BossShield bossShield; // Boss护盾脚本
    [SerializeField] private Transform playerTransform; // 玩家的Transform
    [SerializeField] private GameObject teleportEffectPrefab; // 传送特效 (可选)
    [SerializeField] private Transform portalTransform; // 最终阶段逃跑的传送门Transform
    
    [Tooltip("【重要】存储不同阶段活动范围的 Collider2D 列表")]
    [SerializeField] private List<BoxCollider2D> phasePolygons; // 需要添加这个
    
    [Tooltip("玩家传送点相对于区域中心的【世界X轴】偏移量")]
    [SerializeField] private float playerTeleportXOffset = -3.0f;

    private QuantityBhv hpQuantityComponent;

    private List<int> isphaseSkillEnabled = new List<int>();

    [Header("阶段配置 (示例)")]
    [SerializeField] private float phase1TrapInterval = 2.0f;
    [SerializeField] private float phase1WaveInterval = 6.0f;
    [SerializeField] private float phase2BombFrequency = 1.5f;
    [SerializeField] private float phase2BotInterval = 3.5f;
    [SerializeField] private int phase2FloorCollapseCount = 2;
    [SerializeField] private float phase3BombFrequency = 1.0f;
    [SerializeField] private float phase3BotInterval = 1.5f;
    [SerializeField] private int phase3FloorCollapseCount = 4;
    [SerializeField] private float phase4BotInterval = 0.3f;

    void Start()
    {
        //currentHP = maxHP;
        isphaseSkillEnabled.Add(0);
        isphaseSkillEnabled.Add(0);
        isphaseSkillEnabled.Add(0);
        isphaseSkillEnabled.Add(0);
        
        currentHealth = maxHealth;
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null) Debug.LogError("找不到玩家对象！请确保玩家有'Player'标签。");

        hpQuantityComponent = GetComponentInChildren<QuantityBhv>();
        if (hpQuantityComponent == null)
        {
            Debug.LogError("【错误】在 Boss GameObject 上找不到 QuantityBhv 组件！请确保已添加。", this);
            enabled = false; // 找不到就禁用脚本
            return;
        }


        // 获取动画控制器引用
        // ▼▼▼ 修改点 ▼▼▼
        bossAnimationController = GetComponent<scientistAnimation>(); // 获取新的动画脚本
        if (bossAnimationController == null) Debug.LogError("Boss身上找不到 scientistAnimation 脚本!");
        // 如果 scientistAnimation 脚本内部自己获取 Animator，下面这行可以注释掉
        bossAnimator = GetComponent<Animator>();
        if (bossAnimator == null) Debug.LogError("Boss身上找不到 Animator 组件!");
        // ▲▲▲ 修改点 ▲▲▲

        // 获取其他组件引用 (保持不变)
        trapGenerator = GetComponent<TrapGenerator>();
        waveParticleEmitter = GetComponent<WaveParticleEmitter>();
        droneBombController = GetComponent<DroneBombController>();
        summonBots = GetComponent<SummonBots>();
        floorCollapseController = GetComponent<FloorCollapseController>();
        bossMovementController = GetComponent<BossMovementController>();
        bossShield = GetComponent<BossShield>();
        TrashEnemiesContainerSmall = GameObject.Find("TrashEnemiesContainerSmall");
        // 初始化：禁用所有非阶段1的技能
        DisableAllSkills();

        // 战斗开始，进入阶段1
        //TransitionToPhase(1);
    }

    void Update()
    {   
        //Debug.Log("BossController Update");
        // --- 阶段 0 处理：检查是否开始战斗 ---
        if (currentPhase == 0)
        {
            if (IsPlayerInRoom(1))
            {
                Debug.Log("玩家进入第一房间，准备启动阶段 1！");
                // ▼▼▼【关键修改】调用 TransitionToPhase 来启动阶段 1 并启用技能 ▼▼▼
                SwitchRageAttackCamera_old.instance.SwitchToRageAttackCamera(); // 切换到房间1摄影机
                // yield return new WaitForSeconds(2f); // 等待摄影机切换完成
                //WaitForSecondsRealtime(2f);
                TransitionToPhase(1);
                if (isphaseSkillEnabled[0] == 0)EnableSkill(1);
                // ▲▲▲【关键修改】▲▲▲
                // currentPhase = 1; // 这行不再需要，TransitionToPhase 会设置它
            }
            else
            {
                // 玩家不在房间内，Boss 不做任何事，直接返回
                // Debug.Log("玩家不在第一房间，Boss 等待..."); // 可以取消注释来调试
                return;
            }
        }
            
        
        HealthDisplaySetting();
        if(currentPhase > 0){
            if (isInvincible || isStunned)
            {
                if (isStunned && bossAnimationController != null)
                {
                    // ▼▼▼ 修改点 ▼▼▼
                    // 播放适合眩晕的动画，比如 Idling 或 FloatDown? 暂时用 Idling
                    bossAnimationController.SendMessage("OnAnimatorIdling", SendMessageOptions.DontRequireReceiver); // 使用SendMessage避免方法不存在报错
                    // ▲▲▲ 修改点 ▲▲▲
                }
                return;
            }

            //float hpPercent = currentHP / maxHP;
            float hpPercent = currentHealth / maxHealth;
            if (currentPhase == 1 && hpPercent <= phase2Threshold )
            {
                
                TransitionToPhase(2);
                
                
            }
            else if (currentPhase == 2 && hpPercent <= phase3Threshold )
            {
                
                TransitionToPhase(3);
                
            }
            else if (currentPhase == 3 && hpPercent <=phase4Threshold ) 
            {   
                
                TransitionToPhase(4);
                
            }
            
            if (IsPlayerInRoom(2) && isphaseSkillEnabled[1] == 0) EnableSkill(2);
            if (IsPlayerInRoom(3) && isphaseSkillEnabled[2] == 0) EnableSkill(3); 
            if (IsPlayerInRoom(3) && currentPhase == 4 && isphaseSkillEnabled[3] == 0) EnableSkill(4);
            // 更新Boss动画状态 (放到这里或者需要更精细控制的地方)
            UpdateBossAnimationState();
        }
    }
    
bool IsPlayerInRoom(int roomIndex)
    {
        // 1. 检查引用是否有效
        if (playerTransform == null)
        {
            Debug.LogError("玩家 Transform (playerTransform) 未赋值，无法检测房间！");
            return false; // 无法判断，返回 false
        }
        if (phasePolygons == null || phasePolygons.Count == 0 || phasePolygons[roomIndex - 1] == null)
        {
            Debug.LogError("第一个房间的 PolygonCollider (phasePolygons[0]) 未设置或无效，无法检测房间！");
            return false; // 无法判断，返回 false
        }

        // 2. 获取第一个房间的 PolygonCollider2D
        BoxCollider2D firstRoomPolygon = phasePolygons[roomIndex - 1];

        // 3. 获取玩家当前的位置 (只需要 X, Y 坐标进行 2D 检测)
        Vector2 playerPosXY = new Vector2(playerTransform.position.x, playerTransform.position.y);

        // 4. 使用 PolygonCollider2D.OverlapPoint() 进行检测
        //    这个方法会检查一个点是否在该 Collider 的内部。
        bool isPlayerInFirstRoom = firstRoomPolygon.OverlapPoint(playerPosXY);

        // 5. (可选) 打印调试日志
        // Debug.Log($"玩家位置: {playerPosXY}, 是否在第一房间 ({firstRoomPolygon.name})? {isPlayerInFirstRoom}");

        // 6. 返回检测结果
        return isPlayerInFirstRoom;
    }
    

    void TransitionToPhase(int phase)
    {
        if (phase <= currentPhase) return;
        if (phase > 4) return;

        Debug.Log($"Boss 进入阶段 {phase}");
        currentPhase = phase;
        DisableAllSkills();

        int polygonIndex = phase - 1;
        if (polygonIndex < 0 || polygonIndex >= phasePolygons.Count || phasePolygons[polygonIndex] == null || phase == 4)
        {
             Debug.LogError($"阶段 {phase} 找不到有效的 PolygonCollider!", this);
             // 可能需要回退或采取默认行为
             return;
        }
        BoxCollider2D targetPolygon = phasePolygons[polygonIndex];
        Vector3 polygonCenter = targetPolygon.bounds.center;

        // Boss 目标位置
        Vector3 bossTargetPos = polygonCenter;
        bossTargetPos.z = transform.position.z; // 保持当前 Z
        
        // Player 目标位置
        Vector3 playerTargetPos = polygonCenter;
        playerTargetPos.x += playerTeleportXOffset;
        // if (playerTransform != null) playerTargetPos.z = playerTransform.position.z; // 保持玩家当前 Z

        // --- 2. 传送 Boss 和 Player (先传送，再触发相机) ---
        // (可选: 在这里播放出发特效)
        PlayTeleportEffect(teleportEffectPrefab, transform.position);
        //if (playerTransform != null) PlayTeleportEffect(teleportEffectPrefab, playerTransform.position);

        transform.position = bossTargetPos; // 传送 Boss
        //if (playerTransform != null) playerTransform.position = playerTargetPos; // 传送 Player


        // (可选: 在这里播放到达特效)
        //PlayTeleportEffect(teleportEffectPrefab, bossTargetPos);
        //if (playerTransform != null) PlayTeleportEffect(teleportEffectPrefab, playerTargetPos);
        //Debug.Log($"Boss 传送到 {bossTargetPos}, Player 传送到 {playerTargetPos}");


        // --- 3. 触发相机转换 ---
        // 检查 SwitchRageAttackCamera 实例是否存在，并且只在阶段 2 或 3 时触发相机转换
        if (SwitchRageAttackCamera_new.instance != null && phase >= 2 && phase <= 3) // 修改了条件和实例检查
        {
            // 调用 SwitchRageAttackCamera 的方法，传递 Boss 和 Player 的最终位置
            //GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerController.Instance.isInvincible = true; // 玩家对象不存在，禁用玩家控制器
            PlayerController.Instance.enabled = false; // 禁用玩家控制器
            SwitchRageAttackCamera_new.instance.TriggerBossPhaseTransition(bossTargetPos, playerTargetPos); // 调用了正确的方法
        }
        else if (phase >= 2 && phase <= 3) // 如果实例不存在但应该触发
        {
            Debug.LogWarning("找不到 SwitchRageAttackCamera 实例，无法触发相机转换！");
        }

        
        
        // 阶段切换后，更新Boss动画状态
    }




    private void EnableSkill(int phase)
    {
        DisableAllSkills();
        switch (phase)
        {
            case 1:
                Debug.Log("Boss 进入阶段1！");
                isphaseSkillEnabled[0] = 1;
                if (trapGenerator != null) { trapGenerator.enabled = true; trapGenerator.spawnInterval = phase1TrapInterval; }
                if (waveParticleEmitter != null) { waveParticleEmitter.enabled = true; waveParticleEmitter.waveInterval = phase1WaveInterval; }
                //if (bossShield != null) { bossShield.enabled = true; bossShield.ActivateShield(); } // 护盾脚本要先启用才能调用方法
                if (bossMovementController != null) bossMovementController.enabled = true;
                // ▼▼▼ 修改点 ▼▼▼
                // 阶段1开始时，播放 Idling 或 FloatUp? 假设是 Idling
                if (bossAnimationController != null) bossAnimationController.SendMessage("OnAnimatorIdling", SendMessageOptions.DontRequireReceiver);
                // ▲▲▲ 修改点 ▲▲▲
                break;//陷阱+波+移动

            case 2:
                Debug.Log("Boss 进入阶段2！");
                isphaseSkillEnabled[1] = 1;
                //if(!IsPlayerInRoom(2)) return; // 玩家不在房间2，禁用所有技能
                if (droneBombController != null) { droneBombController.enabled = true; droneBombController.bombFrequency = phase2BombFrequency; }
                if (summonBots != null) { summonBots.enabled = true; summonBots.spawnInterval = phase2BotInterval; }
                if (floorCollapseController != null) { floorCollapseController.enabled = true; floorCollapseController.SetCollapseParameters(phase2FloorCollapseCount, 3.0f); }
                if (trapGenerator != null) trapGenerator.enabled = true;
                if (waveParticleEmitter != null) waveParticleEmitter.enabled = true;
                if (bossMovementController != null) bossMovementController.enabled = true;
                // ▼▼▼ 修改点 ▼▼▼
                // 阶段2，Boss更活跃，可能播放 Floating 状态
                if (bossAnimationController != null) bossAnimationController.SendMessage("OnAnimatorFloating", SendMessageOptions.DontRequireReceiver);
                // ▲▲▲ 修改点 ▲▲▲
                break; 

            case 3:
                Debug.Log("Boss 进入阶段3！");
                isphaseSkillEnabled[2] = 1;
                //if(!IsPlayerInRoom(3)) return;
                if (droneBombController != null) { droneBombController.enabled = true; droneBombController.bombFrequency = phase3BombFrequency; }
                //if (TrashEnemiesContainerSmall)
                if (summonBots != null) { summonBots.enabled = true; summonBots.spawnInterval = phase3BotInterval; }
                //if (floorCollapseController != null) { floorCollapseController.enabled = true; floorCollapseController.SetCollapseParameters(phase3FloorCollapseCount, 2.0f); }
                //if (bossShield != null) { bossShield.enabled = true; bossShield.ActivateShield(); } // 护盾脚本要先启用才能调用方法
                if (trapGenerator != null) trapGenerator.enabled = true;
                if (waveParticleEmitter != null) waveParticleEmitter.enabled = true;
                if (TrashEnemiesContainerSmall!= null) TrashEnemiesContainerSmall.SetActive(true);
                 //if (bossMovementController != null) bossMovementController.enabled = true;
                // ▼▼▼ 修改点 ▼▼▼
                // 阶段3，攻击性强，可能播放 Floating 或 Summoning 状态
                if (bossAnimationController != null) bossAnimationController.SendMessage("OnAnimatorFloating", SendMessageOptions.DontRequireReceiver); // 或者 Summoning?
                // ▲▲▲ 修改点 ▲▲▲
                break;

            case 4:
                isInvincible = true;
                Debug.Log("Boss 无敌了！快跑！");
                isphaseSkillEnabled[3] = 1;
                if (droneBombController != null) droneBombController.enabled = false;
                if (trapGenerator != null) trapGenerator.enabled = false;
                if (waveParticleEmitter != null) waveParticleEmitter.enabled = false;
                if (floorCollapseController != null) floorCollapseController.enabled = false;
                if (bossShield != null) bossShield.DeactivateShield(); // 确保护盾关闭
                //if (bossMovementController != null) bossMovementController.enabled = false;
                //if (summonBots != null) { summonBots.enabled = true; summonBots.spawnInterval = phase4BotInterval; }
                // ▼▼▼ 修改点 ▼▼▼
                // 阶段4，疯狂召唤，播放 Summoning 状态
                StartCoroutine(TrashEnemiesController.instance.RebornTrashEnemiesProcess());
                if (bossAnimationController != null) bossAnimationController.SendMessage("OnAnimatorSummoning", SendMessageOptions.DontRequireReceiver);
                // ▲▲▲ 修改点 ▲▲▲
                ShowEscapeObjective();
                break;
        }
        
    }
    private void DisableAllSkills()
    {
        if (trapGenerator != null) trapGenerator.enabled = false;
        if (waveParticleEmitter != null) waveParticleEmitter.enabled = false;
        if (droneBombController != null) droneBombController.enabled = false;
        if (summonBots != null) summonBots.enabled = false;

        // FloorCollapseController 可能需要保持启用以恢复地板，但可以禁用自动触发
        if (floorCollapseController != null) floorCollapseController.enabled = false; // 或者 floorCollapseController.autoTrigger = false;
        //if (bossShield != null) bossShield.DeactivateShield();
        // 移动通常不需要禁用，除非特定阶段
         if (bossMovementController != null) bossMovementController.enabled = false;
    }

    private void PlayTeleportEffect(GameObject effectPrefab, Vector3 position)
    {
        if (effectPrefab != null) Instantiate(effectPrefab, position, Quaternion.identity);
    }

    // private void Die()
    // {
    //     Debug.Log("Boss 被击败了!");
    //     isInvincible = true;
    //     DisableAllSkills();
    //     // ▼▼▼ 修改点 ▼▼▼
    //     // 播放死亡动画，可能对应 FloatDown? 或者需要专门的死亡动画状态
    //     if (bossAnimationController != null) bossAnimationController.SendMessage("OnAnimatorFloatDown", SendMessageOptions.DontRequireReceiver); // 假设FloatDown是死亡
    //     else if (bossAnimator != null) bossAnimator.SetTrigger("Die"); // 或者用触发器
    //     // ▲▲▲ 修改点 ▲▲▲
    //     // Destroy(gameObject, 5f); // 延迟销毁
    // }

    public void TriggerStun()
    {
        if (isStunned || isInvincible) return;
        Debug.Log($"Boss 护盾被打破，眩晕 {shieldStunDuration} 秒!");
        StartCoroutine(StunSequence());
    }

    private IEnumerator StunSequence()
    {
        isStunned = true;
        PauseAttacks(true);

        // ▼▼▼ 修改点 ▼▼▼
        // 播放眩晕动画，用 Idling 或 FloatDown?
        if (bossAnimationController != null) bossAnimationController.SendMessage("OnAnimatorIdling", SendMessageOptions.DontRequireReceiver);
        // ▲▲▲ 修改点 ▲▲▲

        yield return new WaitForSeconds(shieldStunDuration);

        isStunned = false;
        PauseAttacks(false);
        Debug.Log("Boss 眩晕结束!");
        // ▼▼▼ 修改点 ▼▼▼
        // 恢复动画状态
        UpdateBossAnimationState();
        // ▲▲▲ 修改点 ▲▲▲
    }

    private void PauseAttacks(bool pause)
    {
        // 暂停/恢复攻击逻辑 (保持不变)
        Debug.Log($"Boss 攻击行为已 {(pause ? "暂停" : "恢复")}");
    }

    // 更新Boss动画状态的核心逻辑
    private void UpdateBossAnimationState()
    {
        if (bossAnimationController == null || isStunned) return; // 眩晕时由StunSequence控制

        if (isInvincible && currentPhase == 4)
        {
            // 阶段4保持召唤动画
            bossAnimationController.SendMessage("OnAnimatorSummoning", SendMessageOptions.DontRequireReceiver);
            return;
        }

        // --- 判断当前动作优先级 ---
        // 1. 是否正在使用某个需要特定动画的技能？ (例如，如果火球波有施法动画)
        //    if (isCastingWave) { bossAnimationController.OnAnimatorSummonUp(); return; } // 假设SummonUp是施法准备

        // 2. 是否正在移动？
        bool moving = (bossMovementController != null && bossMovementController.IsMoving());
        if (moving)
        {
             // 移动时播放什么动画？Floating 比较合适
            bossAnimationController.SendMessage("OnAnimatorFloating", SendMessageOptions.DontRequireReceiver);
        }
        // 3. 如果不移动也不在特殊动作中，则根据阶段决定默认状态
        else
        {
            switch (currentPhase)
            {
                case 1:
                    bossAnimationController.SendMessage("OnAnimatorIdling", SendMessageOptions.DontRequireReceiver);
                    break;
                case 2:
                case 3:
                    // 阶段2和3更活跃，默认 Floating?
                    bossAnimationController.SendMessage("OnAnimatorFloating", SendMessageOptions.DontRequireReceiver);
                    break;
                default:
                     // 其他情况（比如刚开始或阶段过渡）用Idling
                    bossAnimationController.SendMessage("OnAnimatorIdling", SendMessageOptions.DontRequireReceiver);
                    break;
            }
        }

        // 注意：这里没有处理攻击动画，因为你的技能脚本（如SummonBots, WaveParticleEmitter）
        // 是独立运行的。如果希望Boss在“执行”这些技能时播放特定动画（如Summoning），
        // 需要让这些技能脚本在激活/执行时通知BossController，或者BossController反过来
        // 检查这些脚本的状态来决定是否播放Summoning动画。这会增加复杂度。
        // 一个简单的处理是，在阶段2、3、4，默认就是比较活跃的Floating或Summoning状态。
    }


    private void ShowEscapeObjective()
    {
    //    Debug.Log($"提示玩家: 快逃往传送门! 位置: {targetPosition}");

        // ▼▼▼【关键修改】调用传送门聚焦相机脚本 ▼▼▼
        if (PortalFocusCamera.instance != null)
        {
            PortalFocusCamera.instance.FocusOnPortal(); // 触发聚焦传送门的运镜
        }
        else
        {
            Debug.LogWarning("找不到 PortalFocusCamera 实例，无法触发聚焦传送门效果！");
        }
    }

    public void Teleport(Vector3 newPosition)
    {
        // ▼▼▼ 修改点 ▼▼▼
        // 传送前可以播放 FloatDown，传送后播放 FloatUp
        if (bossAnimationController != null) bossAnimationController.SendMessage("OnAnimatorFloatDown", SendMessageOptions.DontRequireReceiver);
        // 播放特效等
        if (teleportEffectPrefab != null) 
            {
                GameObject temp = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity, transform);
                //
                Destroy(temp, 1f); // 3秒后销毁特效对象
            }
        //3秒后特效消失

        
        // ▲▲▲ 修改点 ▲▲▲

        transform.position = newPosition; // 实际传送

        // ▼▼▼ 修改点 ▼▼▼
            if (teleportEffectPrefab != null) 
            {
                GameObject temp2 = Instantiate(teleportEffectPrefab, transform.position, Quaternion.identity,transform);
                Destroy(temp2, 1f); // 3秒后销毁特效对象
            }
        if(waveParticleEmitter != null && waveParticleEmitter.enabled) waveParticleEmitter.ForceEmitWave(); // 波动特效
        // 传送后恢复动画，比如 FloatUp 接 Floating
        if (bossAnimationController != null)
        {
            bossAnimationController.SendMessage("OnAnimatorFloatUp", SendMessageOptions.DontRequireReceiver);
            // 可以延迟一小会再切换回 Floating
            // StartCoroutine(ResetToFloatingAfterTeleport(0.5f));
            // 或者直接由 UpdateBossAnimationState 在下一帧更新
        }
        // ▲▲▲ 修改点 ▲▲▲
        Debug.Log($"Boss 传送到 {newPosition}");
    }

    private void HealthDisplaySetting(){
        direction = transform.localScale.x > 0 ? 1 : -1;
        // 确保 HealthDisplay 的 x 轴缩放方向与 direction 一致
        HealthDisplay.localScale = new Vector3(
            Mathf.Abs(HealthDisplay.localScale.x) * direction, // 让 x 方向匹配 direction
            HealthDisplay.localScale.y,
            HealthDisplay.localScale.z
        );
        if (quantityBhv != null)
        {
            quantityBhv.Amount = currentHealth;
        }
    }
    // (可选) 辅助协程，用于传送动画后恢复
    /*
    private IEnumerator ResetToFloatingAfterTeleport(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!isStunned && bossAnimationController != null) // 确保状态合适
        {
             bossAnimationController.SendMessage("OnAnimatorFloating", SendMessageOptions.DontRequireReceiver);
        }
    }
    */
}