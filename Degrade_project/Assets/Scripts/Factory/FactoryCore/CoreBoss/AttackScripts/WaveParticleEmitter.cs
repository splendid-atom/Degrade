using UnityEngine;
using System.Collections.Generic;

public class WaveParticleEmitter : MonoBehaviour
{
    // 波形设置
    public float waveSpeed = 5f;
    public float particleLifetime = 5f;
    public float particleSize = 0.2f; // 粒子大小参数
    
    // 发射设置
    public float waveInterval = 2f; // 波之间的时间间隔
    public int particlesPerWave = 180; // 每个波的粒子数量
    
    // 缝隙设置
    public int gapsPerWave = 2; // 每波中的缝隙数量
    public float gapAngle = 40f; // 缝隙的角度宽度

    // Prefab设置
    public GameObject particlePrefab; // 拖拽你的粒子prefab到这里

    // 内部变量
    private float timer = 0f;
    private List<float> currentGapAngles = new List<float>();

    void Start()
    {
        if (particlePrefab == null)
        {
            Debug.LogError("Please assign a particle prefab!");
        }
    }

    void Update()
    {
        // 计时并发射新波
        timer += Time.deltaTime;
        if (timer >= waveInterval)
        {
            timer = 0f;
            EmitWave();
        }
    }

    
    void EmitWave()
    {
        if (particlePrefab == null) return;

        // 生成随机缝隙角度
        GenerateRandomGaps();
        
        // 围绕圆周发射粒子，但在缝隙处不发射
        float angleStep = 360f / particlesPerWave;
        for (int i = 0; i < particlesPerWave; i++)
        {
            float angle = i * angleStep;
            
            // 检查是否在缝隙内
            if (!IsInGap(angle))
            {
                // 计算方向和初始位置（从中心稍微偏移一点）
                float radians = angle * Mathf.Deg2Rad;
                Vector3 direction = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0);
                
                // 从发射器位置稍微偏移一点作为起始点
                Vector3 startPosition = transform.position + direction * 0.1f;
                
                // 实例化粒子预制体
                GameObject particle = Instantiate(particlePrefab, startPosition, Quaternion.identity);
                
                // 设置粒子的移动方向和速度
                SetupParticle(particle, direction);
                
                // 调整粒子系统的大小
                AdjustParticleSystemSize(particle, particleSize);
                
                // 设置粒子的生命周期
                Destroy(particle, particleLifetime);
            }
        }
    }

    public void ForceEmitWave()
    {
        EmitWave();
    }


    void SetupParticle(GameObject particle, Vector3 direction)
    {
        // 获取粒子移动脚本
        csParticleMove moveScript = particle.GetComponent<csParticleMove>();
        if (moveScript != null)
        {
            // 设置粒子的速度
            moveScript.speed = waveSpeed;
            
            // 旋转粒子朝向移动方向
            particle.transform.rotation = Quaternion.LookRotation(direction);
        }
        else
        {
            Debug.LogWarning("Particle prefab doesn't have csParticleMove component!");
        }
    }

    void AdjustParticleSystemSize(GameObject particleObject, float size)
    {
        // 调整所有子对象中的粒子系统大小
        ParticleSystem[] particleSystems = particleObject.GetComponentsInChildren<ParticleSystem>(true);
        
        foreach (ParticleSystem ps in particleSystems)
        {
            var main = ps.main;
            main.startSize = size;
        }
    }

    void GenerateRandomGaps()
    {
        currentGapAngles.Clear();
        
        // 生成随机缝隙位置
        for (int i = 0; i < gapsPerWave; i++)
        {
            float randomAngle = Random.Range(0f, 360f);
            currentGapAngles.Add(randomAngle);
        }
    }

    bool IsInGap(float angle)
    {
        // 检查角度是否在任何缝隙内
        foreach (float gapCenter in currentGapAngles)
        {
            float angleDiff = Mathf.Abs(Mathf.DeltaAngle(angle, gapCenter));
            if (angleDiff < gapAngle / 2)
            {
                return true;
            }
        }
        return false;
    }
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        Gizmos.color = Color.yellow;
        foreach (float gapCenter in currentGapAngles)
        {
            for (float r = 0.5f; r < waveSpeed * particleLifetime; r += 0.5f)
            {
                float startAngle = gapCenter - gapAngle / 2;
                float endAngle = gapCenter + gapAngle / 2;
                
                Vector3 start = transform.position + new Vector3(
                    Mathf.Cos(startAngle * Mathf.Deg2Rad),
                    Mathf.Sin(startAngle * Mathf.Deg2Rad),
                    0) * r;
                
                Vector3 end = transform.position + new Vector3(
                    Mathf.Cos(endAngle * Mathf.Deg2Rad),
                    Mathf.Sin(endAngle * Mathf.Deg2Rad),
                    0) * r;
                
                Gizmos.DrawLine(start, end);
            }
        }
    }
}