using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum lightCase
{
    red, green, normal
}
public class lampAxis : MonoBehaviour
{
    public lightCase lightcase = lightCase.normal;
    public bool isSetLamp;
    public bool isSetNum = false;
    public int lightAround;
    public int lightCount;
    public int x;
    public int y;
    void Start()
    {
        isSetLamp = false;
        lightCount = 0;
    }
}
