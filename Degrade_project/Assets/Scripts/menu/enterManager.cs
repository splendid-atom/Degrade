using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class enterManager : MonoBehaviour
{
    public GameObject namepanel;
    public InputField nameinput;
    void Start()
    {
        if (!PlayerPrefs.HasKey("PlayerName"))
        {
            namepanel.SetActive(true);
        }
        else Debug.Log(PlayerPrefs.GetString("PlayerName"));
    }

    // Update is called once per frame
    public void enterName()
    {
        PlayerPrefs.SetString("PlayerName", nameinput.text);
        namepanel.SetActive(false);
    }
    public void changeName()
    {
        namepanel.SetActive(true);
    }
}
