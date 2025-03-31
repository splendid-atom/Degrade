using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRunTrashGenerator : MonoBehaviour
{
    public GameObject WoodenCablePrefab;
    public List<GameObject> SmallTrashPrefabs; // 小垃圾预制体
    public List<Transform> CableGeneratePosList;
    public List<Transform> SmallTrashGeneratePosList;
    public GameObject TrashContainer;
    
    public float CableSpawnInterval = 5f; // 线缆生成间隔
    public float SmallTrashSpawnInterval = 3f; // 小垃圾生成间隔

    void Start()
    {
        // 每 CableSpawnInterval 秒调用一次 CabelGenerator 方法
        InvokeRepeating(nameof(CabelGenerator), 0f, CableSpawnInterval);

        // 每 SmallTrashSpawnInterval 秒调用一次 SmallTrashGenerator 方法
        InvokeRepeating(nameof(SmallTrashGenerator), 1f, SmallTrashSpawnInterval);
    }

    private void CabelGenerator()
    {
        if (CableGeneratePosList.Count < 2)
        {
            Debug.LogWarning("CableGeneratePosList 需要至少有 2 个位置");
            return;
        }

        // 随机选择两个不同的索引
        List<int> indices = new List<int> { 0, 1, 2 };
        int firstIndex = indices[Random.Range(0, indices.Count)];
        indices.Remove(firstIndex); // 防止重复
        int secondIndex = indices[Random.Range(0, indices.Count)];

        // 计算生成位置，仅使用 x 和 y，z 设为 0.2329698
        Vector3 pos1 = new Vector3(CableGeneratePosList[firstIndex].position.x, CableGeneratePosList[firstIndex].position.y, 0.2329698f);
        Vector3 pos2 = new Vector3(CableGeneratePosList[secondIndex].position.x, CableGeneratePosList[secondIndex].position.y, 0.2329698f);

        // 生成并设置父对象
        Instantiate(WoodenCablePrefab, pos1, Quaternion.identity, TrashContainer.transform);
        Instantiate(WoodenCablePrefab, pos2, Quaternion.identity, TrashContainer.transform);
    }

    private void SmallTrashGenerator()
    {
        if (SmallTrashGeneratePosList.Count == 0)
        {
            Debug.LogWarning("SmallTrashGeneratePosList 为空，无法生成小垃圾");
            return;
        }

        if (SmallTrashPrefabs.Count == 0)
        {
            Debug.LogWarning("SmallTrashPrefabs 为空，无法生成小垃圾");
            return;
        }

        // 随机选择一个生成位置
        int randomIndex = Random.Range(0, SmallTrashGeneratePosList.Count);
        Vector3 spawnPos = new Vector3(SmallTrashGeneratePosList[randomIndex].position.x, 
                                    SmallTrashGeneratePosList[randomIndex].position.y, 
                                    0.2329698f);

        // 随机选择一个小垃圾预制体
        int prefabIndex = Random.Range(0, SmallTrashPrefabs.Count);
        GameObject selectedPrefab = SmallTrashPrefabs[prefabIndex];

        // 生成小垃圾
        Instantiate(selectedPrefab, spawnPos, Quaternion.identity, TrashContainer.transform);
    }

}
