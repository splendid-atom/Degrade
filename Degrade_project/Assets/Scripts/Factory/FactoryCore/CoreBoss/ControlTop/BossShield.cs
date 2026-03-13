using UnityEngine;
using System.Collections;

public class BossShield : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private float maxShieldHP = 200f; // 护盾的最大生命值
    [SerializeField] private GameObject shieldVisual; // 护盾的视觉效果对象 (需要拖拽赋值)
    [SerializeField] private float regenerationDelay = 10.0f; // 护盾被打破后，多少秒开始再生
    [SerializeField] private float regenerationRate = 50f; // 每秒恢复多少护盾值 (如果需要缓慢再生)
    // 或者 [SerializeField] private bool instantlyRegenerate = true; // 是否瞬间完全恢复
    [SerializeField] public float DamagePerhit = 10f;
    private float currentShieldHP;
    private bool shieldActive = true; // 护盾当前是否激活
    private bool isRegenerating = false; // 是否正在再生中
    private BossController bossController; // 对Boss主控制器的引用
     private ParticleSystem shieldParticleSystem;

    void Start()
    {
        bossController = GetComponentInParent<BossController>();
        if (bossController == null) Debug.LogError("找不到父对象的 BossController 脚本!", this);

        if (shieldVisual == null)
        {
            //  Debug.LogError("未分配护盾视觉对象 (Shield Visual)!", this);
            //  enabled = false; // 没有视觉效果，禁用脚本
            //  return;
        }

        // ▼▼▼ 获取 ParticleSystem 组件 ▼▼▼
        shieldParticleSystem = shieldVisual.GetComponent<ParticleSystem>();
        if (shieldParticleSystem == null)
        {
            // 尝试在子对象中查找
            shieldParticleSystem = shieldVisual.GetComponentInChildren<ParticleSystem>(true);
            if (shieldParticleSystem == null)
            {
                Debug.LogError("在护盾视觉对象及其子对象中都找不到 ParticleSystem 组件!", this);
                // 虽然不影响核心逻辑，但视觉效果会缺失
            }
        }

       //shieldVisual.SetActive(true);
       currentShieldHP = maxShieldHP;
    }

    void Update()
    {
        // 如果需要缓慢再生护盾
        /*
        if (isRegenerating && shieldActive)
        {
            currentShieldHP += regenerationRate * Time.deltaTime;
            currentShieldHP = Mathf.Clamp(currentShieldHP, 0, maxShieldHP);
            // TODO: 更新护盾视觉效果 (比如根据血量百分比改变颜色或透明度)
            if (currentShieldHP >= maxShieldHP)
            {
                currentShieldHP = maxShieldHP;
                isRegenerating = false;
                Debug.Log("护盾完全恢复!");
            }
        }
        */
    }


    // 激活护盾 (由BossController在进入阶段3时调用)
    public void ActivateShield()
    {
        if (shieldActive) return;

        Debug.Log("Boss 护盾激活!");
        currentShieldHP = maxShieldHP;
        shieldActive = true;
        isRegenerating = false; // 停止再生状态
        StopAllCoroutines(); // 停止可能正在运行的再生协程

        // ▼▼▼ 激活视觉并播放粒子效果 ▼▼▼
        if (shieldVisual != null)
        {
            shieldVisual.SetActive(true); // 1. 确保 GameObject 是激活的
            if (shieldParticleSystem != null)
            {
                shieldParticleSystem.Play(); // 2. 显式播放粒子系统
            }
        }
        // ▲▲▲ 激活视觉并播放粒子效果 ▲▲▲

        // TODO: 播放护盾开启音效/特效
    }

    // 停用护盾 (被打破或BossController强制禁用时调用)
    public void DeactivateShield()
    {
        if (!shieldActive) return; // 只有激活状态才能停用

        Debug.Log("Boss 护盾停用。");
        shieldActive = false;
        isRegenerating = false; // 重置再生状态

        // ▼▼▼ 停止粒子效果并隐藏视觉 ▼▼▼
        if (shieldParticleSystem != null)
        {
            // 停止发射新粒子，并清除已存在的粒子 (如果希望瞬间消失)
            shieldParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            // 或者，如果希望已有的粒子慢慢消失:
            // shieldParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            // (但下面紧接着 SetActive(false) 会让渐隐效果看不到)
        }
        if (shieldVisual != null)
        {
            // 隐藏 GameObject
            gameObject.SetActive(false);
            shieldVisual.SetActive(false);
        }
        // ▲▲▲ 停止粒子效果并隐藏视觉 ▲▲▲

        // 确保再生协程已停止
        StopAllCoroutines();
    }

    // 护盾承受伤害
    public void TakeDamage(float amount)
    {
        //if (!shieldActive || isRegenerating) return; // 护盾未激活或正在再生时不承受伤害

        currentShieldHP -= amount;
        Debug.Log($"护盾受到 {amount} 伤害, 剩余: {currentShieldHP}/{maxShieldHP}");
        // TODO: 播放护盾受击特效/音效

        if (currentShieldHP <= 0)
        {
            BreakShield();
        }
        else
        {
             // TODO: 更新护盾视觉效果 (可选，比如受击闪烁)
        }
    }

    // 护盾被打破
    private void BreakShield()
    {
        Debug.Log("护盾被打破!");
        DeactivateShield(); // 停用护盾视觉和功能

        // 触发Boss眩晕
        if (bossController != null)
        {
            bossController.TriggerStun();
        }

        // 开始再生计时
        StartCoroutine(RegenerationTimer());
    }

    // 再生计时器
private IEnumerator RegenerationTimer()
    {
        isRegenerating = true; // 标记为再生中 (防止在延迟期间再次受伤)
        Debug.Log($"护盾将在 {regenerationDelay} 秒后再生...");
        yield return new WaitForSeconds(regenerationDelay);
        isRegenerating = false; // 延迟结束

        // 检查 Boss 是否还处于允许护盾再生的状态
        if (bossController != null && bossController.currentPhase == 3 && !bossController.isStunned && this.enabled)
        {
             Debug.Log("护盾尝试再生...");
            //  if (instantlyRegenerate) // 如果是瞬间恢复
            //  {
            //     ActivateShield(); // 直接重新激活
            //  }
             // else { /* 处理缓慢恢复逻辑 */ }
        } else {
             Debug.Log("条件不满足，护盾未能再生。");
        }
    }
    // 提供给BossController查询护盾状态的接口
    public bool IsActive()
    {
        return shieldActive;
    }
}