using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory2Controller : MonoBehaviour
{
    public static Factory2Controller Instance;
    public List<float> shieldGeneratorHealths = new List<float>();
    public List<GameObject> shieldGenerators = new List<GameObject>();
    private GameObject shieldGeneratorContainer;
    public bool isFallingFloorsCrazy = false;
    public bool isCanonRageMode = false;//炮台是否在 rage mode
    public GameObject BigRobot;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if(BigRobot==null){
            BigRobot = GameObject.Find("BigRobot");
        }
        shieldGeneratorContainer = GameObject.Find("ShieldGeneratorContainer");

        if (shieldGeneratorContainer != null)
        {
            foreach (Transform child in shieldGeneratorContainer.transform)
            {
                if (child != null)
                {
                    ShieldGeneratorController shieldGen = child.GetComponent<ShieldGeneratorController>();
                    if (shieldGen != null)
                    {
                        shieldGenerators.Add(child.gameObject);
                        shieldGeneratorHealths.Add(shieldGen.Health);
                    }
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
        if (shieldGenerators != null)
        {
            for (int i = 0; i < shieldGenerators.Count; i++)
            {
                if (shieldGenerators[i] != null)
                {
                    ShieldGeneratorController shieldGen = shieldGenerators[i].GetComponent<ShieldGeneratorController>();
                    if (shieldGen != null)
                    {
                        shieldGeneratorHealths[i] = shieldGen.Health;
                    }
                }
            }
        }
        CheckAndSetFallingFloorsCrazy();
        // 添加bigrobot的rigidbody2D
        if (isCanonRageMode&&BigRobot != null)
        {
            // 检查是否已有 Rigidbody2D，若没有则添加
            Rigidbody2D rb = BigRobot.GetComponent<Rigidbody2D>();
            if (rb == null)
            {
                rb = BigRobot.AddComponent<Rigidbody2D>();
                Debug.Log("Rigidbody2D added to " + BigRobot.name);
            }
        }
    }

    public bool isCanonInRageMode(){
        return isCanonRageMode;
    }
    public void SetCanonRageMode()
    {
        isCanonRageMode = true;
    }
    public void CheckAndSetFallingFloorsCrazy()
    {
        if (shieldGeneratorHealths != null && shieldGenerators != null &&
            shieldGeneratorHealths.Count > 1 && shieldGenerators.Count > 1)
        {
            if (shieldGeneratorHealths[1] <= 0.5f || shieldGenerators[1] == null)
            {
                isFallingFloorsCrazy = true;
            }
        }
    }
}