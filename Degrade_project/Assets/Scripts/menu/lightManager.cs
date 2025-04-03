using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class lightManager : MonoBehaviour
{
    public static lightManager lightmanager;

    public Material red_Light;
    public Material green_Light;
    public Material normal_Light;
    public GameObject lamp;
    public List<GameObject> lampList = new List<GameObject>();
    public GameObject cube;
    public List<GameObject> cubeList = new List<GameObject>();
    public int wrongTime;//判断是否有两灯同行同列
    void Awake()
    {
        if (lightmanager == null)
        {
            lightmanager = this;
        }
    }
    void Start()
    {
        wrongTime = 0;
        foreach (Transform child in lamp.transform)
        {
            lampList.Add(child.gameObject);
        }
        foreach (Transform child in cube.transform)
        {
            cubeList.Add(child.gameObject);
            child.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (doorTrigger.doortrigger.isTrickRaised)
        {
            foreach (GameObject light in lampList)
            {
                //将每个light的隐藏子物体Spotlight设置为可见
                Transform child = light.transform.Find("Spotlight");
                child.gameObject.SetActive(true);
            }
            foreach (Transform child in cube.transform)
            {
                child.gameObject.GetComponent<BoxCollider2D>().isTrigger = false;
            }
        }
    }
    public void lightLamp(Transform lamp)
    {
        //关闭灯
        if (lamp.GetComponent<lampAxis>().isSetLamp)
        {
            lamp.GetComponent<lampAxis>().isSetLamp = false;
            foreach (GameObject light in lampList)
            {
                if (light.GetComponent<lampAxis>().x == lamp.gameObject.GetComponent<lampAxis>().x || light.GetComponent<lampAxis>().y == lamp.gameObject.GetComponent<lampAxis>().y)
                {
                    if (isBlocked(lamp.gameObject.GetComponent<lampAxis>(), light.GetComponent<lampAxis>()))
                    {
                        continue;
                    }
                    light.GetComponent<lampAxis>().lightCount--;
                    //对该点本身的操作
                    if (light.GetComponent<lampAxis>().x == lamp.gameObject.GetComponent<lampAxis>().x && light.GetComponent<lampAxis>().y == lamp.gameObject.GetComponent<lampAxis>().y)
                    {
                        if (light.GetComponent<lampAxis>().lightCount == 0)
                        {
                            light.transform.Find("Light_01").GetComponent<MeshRenderer>().material = normal_Light;
                            light.transform.Find("Spotlight").GetComponent<Light>().color = Color.blue;
                        }
                        else
                        {
                            light.transform.Find("Light_01").GetComponent<MeshRenderer>().material = green_Light;
                            light.transform.Find("Spotlight").GetComponent<Light>().color = Color.green;
                        }
                    }
                    else
                    {
                        if (light.GetComponent<lampAxis>().lightCount == 0 && light.gameObject.GetComponent<lampAxis>().isSetLamp == false)
                        {
                            light.transform.Find("Light_01").GetComponent<MeshRenderer>().material = normal_Light;
                            light.transform.Find("Spotlight").GetComponent<Light>().color = Color.blue;
                        }
                        else if (light.GetComponent<lampAxis>().lightCount > 0 && light.gameObject.GetComponent<lampAxis>().isSetLamp == false)
                        {
                            light.transform.Find("Light_01").GetComponent<MeshRenderer>().material = green_Light;
                            light.transform.Find("Spotlight").GetComponent<Light>().color = Color.green;
                        }
                        else if (light.GetComponent<lampAxis>().isSetLamp)
                        {
                            wrongTime--;
                        }
                    }
                }
            }
        }
        //点亮灯
        else if (!lamp.GetComponent<lampAxis>().isSetLamp)
        {
            foreach (GameObject light in lampList)
            {
                if (light.GetComponent<lampAxis>().x == lamp.gameObject.GetComponent<lampAxis>().x || light.GetComponent<lampAxis>().y == lamp.gameObject.GetComponent<lampAxis>().y)
                {
                    //障碍判断
                    if (isBlocked(lamp.gameObject.GetComponent<lampAxis>(), light.GetComponent<lampAxis>()))
                    {
                        continue;
                    }
                    if (light.GetComponent<lampAxis>().x == lamp.gameObject.GetComponent<lampAxis>().x && light.GetComponent<lampAxis>().y == lamp.gameObject.GetComponent<lampAxis>().y)
                    {
                        light.transform.Find("Light_01").GetComponent<MeshRenderer>().material = red_Light;
                        light.transform.Find("Spotlight").GetComponent<Light>().color = Color.red;
                    }
                    else
                    {
                        if (!light.gameObject.GetComponent<lampAxis>().isSetLamp)
                        {
                            light.transform.Find("Light_01").GetComponent<MeshRenderer>().material = green_Light;
                            light.transform.Find("Spotlight").GetComponent<Light>().color = Color.green;
                        }
                        else
                        {
                            wrongTime++;
                        }
                    }
                    light.GetComponent<lampAxis>().lightCount++;
                }
            }
            lamp.GetComponent<lampAxis>().isSetLamp = true;
        }

        if (passJudge())
        {
            Debug.Log("mission success!");
            doorTrigger.doortrigger.trickControl = true;
        }
        Debug.Log(wrongTime);
    }
    public bool isBlocked(lampAxis lampS, lampAxis lampT)
    {
        if (lampS.x == lampT.x) // 同一列，检查垂直路径
        {
            int minY = Mathf.Min(lampS.y, lampT.y);
            int maxY = Mathf.Max(lampS.y, lampT.y);
            for (int y = minY + 1; y < maxY; y++)
            {
                if (cubeList.Exists(cube => cube.GetComponent<lampAxis>().x == lampS.x && cube.GetComponent<lampAxis>().y == y))
                {
                    return true; // 被黑格阻挡
                }
            }
        }
        else if (lampS.y == lampT.y) // 同一行，检查水平路径
        {
            int minX = Mathf.Min(lampS.x, lampT.x);
            int maxX = Mathf.Max(lampS.x, lampT.x);
            for (int x = minX + 1; x < maxX; x++)
            {
                if (cubeList.Exists(cube => cube.GetComponent<lampAxis>().x == x && cube.GetComponent<lampAxis>().y == lampS.y))
                {
                    return true; // 被黑格阻挡
                }
            }
        }
        return false;
    }
    public bool passJudge()
    {
        if (wrongTime == 0)
        {
            foreach (GameObject light in lampList)
            {
                if (light.GetComponent<lampAxis>().lightCount == 0) return false;
            }
            bool lamp1 = GameObject.Find("Light12").GetComponent<lampAxis>().isSetLamp;
            bool lamp2 = GameObject.Find("Light14").GetComponent<lampAxis>().isSetLamp;
            bool lamp3 = GameObject.Find("Light23").GetComponent<lampAxis>().isSetLamp;
            if (lamp1 && lamp2 && !lamp3) return false;
            else if (lamp1 && !lamp2 && lamp3) return false;
            else if (!lamp1 && lamp2 && lamp3) return false;
            else if (lamp1 && lamp2 && lamp3) return false;
            else if (!lamp1 && !lamp2 && !lamp3) return false;
            lamp2 = GameObject.Find("Light21").GetComponent<lampAxis>().isSetLamp;
            if (!lamp1 && !lamp2 && !lamp3)
            {
                //均未放灯则继续执行判断
            }
            else return false;
            lamp1 = GameObject.Find("Light15").GetComponent<lampAxis>().isSetLamp;
            lamp2 = GameObject.Find("Light24").GetComponent<lampAxis>().isSetLamp;
            lamp3 = GameObject.Find("Light35").GetComponent<lampAxis>().isSetLamp;
            if (lamp1 && lamp2 && !lamp3) return false;
            else if (lamp1 && !lamp2 && lamp3) return false;
            else if (!lamp1 && lamp2 && lamp3) return false;
            else if (lamp1 && lamp2 && lamp3) return false;
            else if (!lamp1 && !lamp2 && !lamp3) return false;
            lamp1 = GameObject.Find("Light41").GetComponent<lampAxis>().isSetLamp;
            lamp2 = GameObject.Find("Light52").GetComponent<lampAxis>().isSetLamp;
            lamp3 = GameObject.Find("Light61").GetComponent<lampAxis>().isSetLamp;
            if (!lamp1 && !lamp2 && !lamp3) return false;
            else if (lamp1 && lamp2 && lamp3) return false;
            else if (lamp1 && !lamp2 && !lamp3) return false;
            else if (!lamp1 && !lamp2 && lamp3) return false;
            else if (!lamp1 && lamp2 && !lamp3) return false;
            lamp1 = GameObject.Find("Light73").GetComponent<lampAxis>().isSetLamp;
            lamp2 = GameObject.Find("Light53").GetComponent<lampAxis>().isSetLamp;
            lamp3 = GameObject.Find("Light64").GetComponent<lampAxis>().isSetLamp;
            if (!lamp1 && !lamp2 && !lamp3)
            {
                //均未放灯则继续执行判断
            }
            else return false;
            lamp1 = GameObject.Find("Light76").GetComponent<lampAxis>().isSetLamp;
            lamp2 = GameObject.Find("Light67").GetComponent<lampAxis>().isSetLamp;
            lamp3 = GameObject.Find("Light65").GetComponent<lampAxis>().isSetLamp;
            if (!lamp1 && !lamp2 && !lamp3) return false;
            else if (lamp1 && lamp2 && lamp3) return false;
            else if (lamp1 && !lamp2 && !lamp3) return false;
            else if (!lamp1 && !lamp2 && lamp3) return false;
            else if (!lamp1 && lamp2 && !lamp3) return false;
        }
        else return false;
        return true;
    }
}
