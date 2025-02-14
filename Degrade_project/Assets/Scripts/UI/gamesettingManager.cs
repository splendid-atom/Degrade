using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gamesettingManager : MonoBehaviour
{
    public GameObject settingboard;
    public void openSet()
    {
        settingboard.SetActive(true);
    }
    public void closeSet()
    {
        settingboard.SetActive(false);
    }
}
