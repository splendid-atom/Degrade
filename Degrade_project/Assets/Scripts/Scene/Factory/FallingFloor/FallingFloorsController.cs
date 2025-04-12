using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingFloorsController : MonoBehaviour
{
    public static  FallingFloorsController Instance;
    private List<Transform> FallingFloors = new List<Transform>(); // 地板列表
    private List<bool> isFallingList = new List<bool>(); // 记录每个地板是否在下降
    private Transform FallingFloorsContainer;
    public bool isFalling = false;
    public float fallInterval = 2f;  // 默认的地板下降时间间隔
    public float crazyFallInterval = 0.5f;  // 疯狂模式下的地板下降时间间隔
    private void Awake()
    {
        Instance = this;
    }
    void Start()
    {
        FallingFloorsContainer = transform;
        foreach (Transform child in FallingFloorsContainer) // 遍历子对象
        {
            FallingFloors.Add(child);
            isFallingList.Add(false); // 初始状态，所有地板都未下降
        }

        // 初始化间隔并开始调用 RandomFalling
        InvokeRepeating(nameof(RandomFalling), 1f, fallInterval);
    }

    void Update()
    {
        // 检查是否有需要更新的条件，比如是否需要改变掉落时间间隔
        CheckFallingFloorSpeed();
        isPlayerFalling();
    }

    void CheckFallingFloorSpeed()
    {
        if (Factory2Controller.Instance.isFallingFloorsCrazy)  // 检查是否进入疯狂模式
        {
            if (fallInterval != crazyFallInterval)
            {
                // 如果当前的间隔不是疯狂模式的间隔，取消旧的调用并重新设置
                CancelInvoke(nameof(RandomFalling));
                InvokeRepeating(nameof(RandomFalling), 1f, crazyFallInterval);
                fallInterval = crazyFallInterval;  // 更新当前间隔
            }
        }
        else
        {
            if (fallInterval != 2f)
            {
                // 如果不在疯狂模式，恢复正常间隔
                CancelInvoke(nameof(RandomFalling));
                InvokeRepeating(nameof(RandomFalling), 1f, 2f);
                fallInterval = 2f;  // 恢复默认间隔
            }
        }
    }

    void RandomFalling()
    {
        if (FallingFloors.Count == 0) return;

        // 过滤未下降的地板
        List<int> availableFloors = new List<int>();
        for (int i = 0; i < FallingFloors.Count; i++)
        {
            if (!isFallingList[i]) availableFloors.Add(i);
        }

        // 如果没有可用的地板，直接返回
        if (availableFloors.Count == 0) return;

        // 随机选择一个未下降的地板
        int randomIndex = availableFloors[Random.Range(0, availableFloors.Count)];
        Transform selectedFloor = FallingFloors[randomIndex];

        // 标记该地板为下降状态
        isFallingList[randomIndex] = true;

        // 开启协程
        StartCoroutine(FallAndReset(selectedFloor, randomIndex));
    }

    public void isPlayerFalling()
    {
        for (int i = 0; i < FallingFloors.Count; i++)
        {
            FallingFloorCollider collider = FallingFloors[i].GetComponentInChildren<FallingFloorCollider>();
            if (collider != null && collider.isPlayerInside && isFallingList[i])
            {
                if(!isFalling){
                    isFalling = true;
                    PlayerFallingDamage();
                }
            }
        }
    }

    void PlayerFallingDamage()
    {
        if (isFalling&&PlayerController.Instance.PlayerHealth>0)
        {
            Debug.Log("player falls");
            StartCoroutine(PlayerRiseAndDie());
        }
    }

    IEnumerator PlayerRiseAndDie()
    {
        if(PlayerController.Instance.PlayerHealth>0){}
        Transform playerTransform = PlayerController.Instance.transform;
        Vector3 originalPosition = playerTransform.position;
        Vector3 targetPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z + 30f); // 目标 Z 轴位置

        float elapsedTime = 0f;
        float duration = 0.5f; // 0.5秒内完成上升
        while (elapsedTime < duration)
        {
            playerTransform.position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = targetPosition; // 确保最终到达目标位置

        // 玩家达到目标位置后立即死亡
        PlayerController.Instance.InstantFallingDie();

    }

    IEnumerator FallAndReset(Transform floor, int index)
    {
        Vector3 originalPosition = floor.position;
        Vector3 targetPosition = new Vector3(originalPosition.x, originalPosition.y, 30f);

        // Z 轴快速上升
        float elapsedTime = 0f;
        float duration = 0.5f; // 0.5秒内完成上升
        while (elapsedTime < duration)
        {
            floor.position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        floor.position = targetPosition; // 确保最终到达目标位置

        // 停留 5 秒
        yield return new WaitForSeconds(5f);

        // Z 轴返回原位置
        elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            floor.position = Vector3.Lerp(targetPosition, originalPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        floor.position = originalPosition; // 确保最终回到原位置

        // 重置状态
        isFallingList[index] = false;
    }
}
