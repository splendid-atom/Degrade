using UnityEngine;
using System.Collections; // 导入协程所需的命名空间

public class DronesAttack : MonoBehaviour
{
    public Transform laserStartPoint;  // 激光发射点
    public float laserRange = 15f;     // 激光射程
    public LineRenderer lineRenderer; // 线渲染器
    private Transform player;          // 玩家对象的 Transform
    private float laserDuration = 0.4f;  // 激光逐渐消失的时间
    private float laserFadeDuration = 0.2f;  // 激光逐渐消失的时间
    private float timer = 0f;          // 计时器
    public bool isFiring = false;     // 激光是否正在发射
    private Vector3 laserDirection;    // 激光的方向
    public bool isHit = false;        // 激光是否击中目标
    private AudioSource[] audioSources;  // 存储所有的 AudioSource

    void Start()
    {
        // 获取玩家对象
        player = GameObject.FindGameObjectWithTag("Player").transform;
        // 获取对象上的所有 AudioSource 组件
        audioSources = GetComponents<AudioSource>();
        // 设置 LineRenderer 初始透明度
        SetLaserAlpha(1f);
        lineRenderer.SetPosition(0, laserStartPoint.position);  // 激光起点
        lineRenderer.SetPosition(1, laserStartPoint.position);  // 激光终点逐渐向目标位置延伸

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            setFiring();
        }
        // if(!isFiring){
        //     audioSources[1].Stop();
        // }
    }

    public void setFiring()
    {
        isFiring = true;
        timer = 0f;  // 重置计时器
        isHit = false;  // 重置击中状态

        // 计算并存储激光发射时的方向（不再改变）
        laserDirection = (player.position - laserStartPoint.position).normalized;

        // 启动协程来处理激光发射
        StartCoroutine(isFiringAttack());
    }

    // 将 isFiringAttack 方法改为协程
    private IEnumerator isFiringAttack()
    {
        while (isFiring)
        {
            // 只在音效没有播放时才播放音效
            if (audioSources.Length > 1 && !audioSources[1].isPlaying)
            {
                audioSources[1].PlayOneShot(audioSources[1].clip);  // 播放第二个 AudioSource 的音效
            }
            timer += Time.deltaTime;  // 增加计时器

            // 激光射程
            Vector3 laserEndPoint = laserStartPoint.position + laserDirection * laserRange;

            RaycastHit2D hit;
            int layerMask = 1 << LayerMask.NameToLayer("Player");  // 只检测玩家层
            // 使用 Physics2D.Raycast 检测激光射线
            hit = Physics2D.Raycast(laserStartPoint.position, laserDirection, laserRange, layerMask);


            if (hit.collider != null)
            {
                laserEndPoint = hit.point;  // 如果碰到物体，激光停止在碰撞点
                // 检测击中的目标名称
                string hitObjectName = hit.collider.gameObject.name;
                if(hitObjectName == "PlayerCharacter"&& !isHit){
                    Debug.Log("Hit target: " + hitObjectName);  // 输出目标名称
                    PlayerController.Instance.PlayerHealth -= 0.5f;                    
                }


                isHit = true;  // 设置击中目标状态



                // 您可以根据需要在这里进行其他操作
            }

            // 计算激光的显示比例，根据计时器逐渐增加激光的长度
            float lerpFactor = Mathf.Clamp01(timer / laserDuration);

            // 设置 LineRenderer 的起点和终点
            lineRenderer.SetPosition(0, laserStartPoint.position);  // 激光起点
            lineRenderer.SetPosition(1, Vector3.Lerp(laserStartPoint.position, laserEndPoint, lerpFactor));  // 激光终点逐渐向目标位置延伸

            // 当激光击中目标或达到射程时，开始渐变消失
            if (isHit || timer >= laserDuration)
            {
                float fadeOutFactor = Mathf.Clamp01((timer - laserFadeDuration) / laserDuration);
                SetLaserAlpha(1f - fadeOutFactor);  // 渐变消失，透明度逐渐减少
            }

            // 当计时器到达最大时，停止发射
            if (timer >= laserDuration * 2)  // 激光总持续时间是两倍的laserDuration
            {
                isFiring = false;
            }

            // 使用 yield return null 等待下一帧
            yield return null;
        }
    }

    void SetLaserAlpha(float alpha)
    {
        // 获取 LineRenderer 的起始和结束颜色
        Color startColor = lineRenderer.startColor;
        Color endColor = lineRenderer.endColor;

        // 设置透明度
        startColor.a = alpha;
        endColor.a = alpha;

        // 更新 LineRenderer 的颜色
        lineRenderer.startColor = startColor;
        lineRenderer.endColor = endColor;
    }
}
