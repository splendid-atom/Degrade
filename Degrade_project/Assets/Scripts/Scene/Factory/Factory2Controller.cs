using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory2Controller : MonoBehaviour
{
    public static Factory2Controller Instance;
    public List<float> shieldGeneratorHealths = new List<float>(); // 存储生命值
    public List<GameObject> shieldGenerators = new List<GameObject>(); // 存储护盾对象
    private GameObject shieldGeneratorContainer;
    public bool isFallingFloorsCrazy = false;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // 获取 ShieldGeneratorContainer
        shieldGeneratorContainer = GameObject.Find("ShieldGeneratorContainer");

        if (shieldGeneratorContainer != null)
        {
            // 遍历所有子对象（护盾生成器）
            foreach (Transform child in shieldGeneratorContainer.transform)
            {
                // 确保子对象有 ShieldGeneratorController 组件
                ShieldGeneratorController shieldGen = child.GetComponent<ShieldGeneratorController>();
                if (shieldGen != null)
                {
                    shieldGenerators.Add(child.gameObject); // 添加护盾对象
                    shieldGeneratorHealths.Add(shieldGen.Health); // 记录初始生命值
                }
            }
        }
        else
        {
            Debug.LogError("ShieldGeneratorContainer not found!");
        }
    }

    void Update()
    {
        // 这里可以更新护盾的生命值，以防止外部修改时未同步
        for (int i = 0; i < shieldGenerators.Count; i++)
        {
            if (shieldGenerators[i] != null)
            {
                ShieldGeneratorController shieldGen = shieldGenerators[i].GetComponent<ShieldGeneratorController>();
                shieldGeneratorHealths[i] = shieldGen.Health;
            }
        }
        CheckAndSetFallingFloorsCrazy();
    }
    public void CheckAndSetFallingFloorsCrazy()
    {
        if(shieldGeneratorHealths[1]<=0.5f){
            isFallingFloorsCrazy = true;  
        }

    }
}
