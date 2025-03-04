using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dataManager : MonoBehaviour
{
    public Save tempSave;
    public bool newFile = false;
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
