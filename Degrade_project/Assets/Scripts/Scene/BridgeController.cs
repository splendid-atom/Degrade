using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BridgeController : MonoBehaviour
{
    public Transform NewPlayerBridge;
    private Transform NewPlayerRiver;
    private float targetZ = -1.943357f;
    private bool isConditionMet = false;  // 控制条件是否满足
    private BoxCollider2D riverCollider;  // 用来表示任务的碰撞体
    private BoxCollider2D[] bridgeContainerColliders;  // 用来表示 BridgeContainer 中的两个碰撞体

    private AudioSource bridgeAudioSource; // 音频源组件
    public bool isBridgeRaised = false; // 用于标记桥是否升起完成

    // Start is called before the first frame update
    void Start()
    {
        // 找到名称为 NewPlayerBridge 的对象
        NewPlayerBridge = GameObject.Find("NewPlayerBridge").transform;
        NewPlayerRiver = GameObject.Find("NewPlayerRiver").transform;

        // 设置初始位置的 z 坐标为 8
        if (NewPlayerBridge != null)
        {
            Vector3 initialPosition = NewPlayerBridge.position;
            initialPosition.z = 8f;
            NewPlayerBridge.position = initialPosition;
        }

        // 获取 NewPlayerRiver 对象上的所有 Collider2D 组件
        Collider2D[] colliders = NewPlayerRiver.GetComponents<Collider2D>();
        
        // 如果第三个 Collider2D 存在，赋值给 riverCollider
        if (colliders.Length >= 3)
        {
            riverCollider = colliders[2] as BoxCollider2D;
        }

        // 获取 NewPlayerBridgeContainer 对象上的所有 Collider2D 组件
        GameObject bridgeContainer = GameObject.Find("NewPlayerBridgeContainer");
        if (bridgeContainer != null)
        {
            bridgeContainerColliders = bridgeContainer.GetComponents<BoxCollider2D>();
        }

        // 获取桥的音频源组件
        bridgeAudioSource = NewPlayerBridge.GetComponent<AudioSource>();

        // 根据任务完成状态来启用或禁用 BoxCollider2D
        UpdateriverCollider();
    }

    // Update is called once per frame
    void Update()
    {
        // 如果桥已经升起，直接设置 z 坐标并退出
        if (InterSceneMemory.instance.isBridgeRised)
        {
            if (NewPlayerBridge.position.z != targetZ)
            {
                NewPlayerBridge.position = new Vector3(NewPlayerBridge.position.x, NewPlayerBridge.position.y, targetZ);
            }
            return;
        }
        TriggerCondition();
        if (NewPlayerBridge != null)
        {
            // 如果条件满足，开始插值减少 z 坐标
            if (isConditionMet)
            {
                float newZ = Mathf.Lerp(NewPlayerBridge.position.z, targetZ, Time.deltaTime);
                Vector3 newPosition = NewPlayerBridge.position;
                newPosition.z = newZ;
                NewPlayerBridge.position = newPosition;

                // 如果音频尚未播放，播放音频
                if (!bridgeAudioSource.isPlaying)
                {
                    bridgeAudioSource.Play();
                }

                // 判断桥是否升起完成
                if (!isBridgeRaised && Mathf.Abs(NewPlayerBridge.position.z - targetZ) < 0.15f)
                {
                    isBridgeRaised = true; // 标记桥已完成升起
                    StartCoroutine(FadeOutAudio()); // 启动音效淡出
                    InterSceneMemory.instance.isBridgeRised = true;
                }
            }
        }

        // 每帧检查并更新碰撞体的状态
        UpdateriverCollider();

        // 如果任务完成，调整 NewPlayerBridgeContainer 中两个碰撞体的 size
        if (isConditionMet && bridgeContainerColliders != null && bridgeContainerColliders.Length >= 2)
        {
            // 将两个碰撞体的 size.x 设置为 0.9
            bridgeContainerColliders[0].size = new Vector2(0.9f, bridgeContainerColliders[0].size.y);
            bridgeContainerColliders[1].size = new Vector2(0.9f, bridgeContainerColliders[1].size.y);
        }
    }

    // 用来控制条件的函数
    public void TriggerCondition()
    {
        if (QuestUIManager.QuestManager.quests[0].isCompleted)
        {
            isConditionMet = true;
        }
    }

    // 根据任务完成状态来启用或禁用 BoxCollider2D
    private void UpdateriverCollider()
    {
        if (riverCollider != null)
        {
            riverCollider.enabled = !QuestUIManager.QuestManager.quests[0].isCompleted;
        }
    }

    // 淡出音频的协程
    private IEnumerator FadeOutAudio()
    {
        float fadeDuration = 0.7f;  // 淡出时长
        float startVolume = bridgeAudioSource.volume;

        // 淡出音量
        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            bridgeAudioSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        bridgeAudioSource.volume = 0f;
        bridgeAudioSource.Stop();
    }
}
