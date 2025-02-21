using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class loadManager : MonoBehaviour
{
    public GameObject loadScreen;
    public GameObject loadSetting;
    public Slider slider;
    public Text text;
    private int timer;

    void Start()
    {
        slider.interactable = false;
    }
    public void loadNextLevel()
    {
        timer = 0;
        StartCoroutine(loadLevel());
    }
    public void openSetting()
    {
        loadSetting.SetActive(true);
    }

    public void closeSetting()
    {
        loadSetting.SetActive(false);
    }

    public void exit()
    {
        Application.Quit();
    }

    IEnumerator loadLevel()
    {
        loadScreen.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync("SampleScene");
        operation.allowSceneActivation = false;
        /*while (!operation.isDone)
        {
            slider.value = operation.progress;
            text.text = operation.progress * 100 + "%";
            if (operation.progress >= 0.9f)
            {
                slider.value = 1;
                text.text = "Press any key to continue";
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true;
                }
            }
            yield return null;
        }*/
        while (timer <= 100)
        {
            slider.value = timer / 100f;
            Debug.Log(slider.value);
            Debug.Log(timer);
            text.text = timer + "%";
            timer += 1;
            yield return new WaitForSeconds(0.05f);

        }
        operation.allowSceneActivation = true;
    }
}
