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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
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