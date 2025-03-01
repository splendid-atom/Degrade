using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;  // 导入SceneManager


//用于跨scene的变量传递
public class InterSceneMemory : MonoBehaviour
{
    public static InterSceneMemory instance;
    public string currentSceneName;
    public string lastSceneName;
    public bool setPlayerPos = false;
    public bool isBeenToBambooMaze = false;
    void Awake()
    {
        if (instance == null)
        {
            currentSceneName = SceneManager.GetActiveScene().name;
            lastSceneName = currentSceneName;
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentSceneName == "BambooMazeScene"&&!isBeenToBambooMaze){
            isBeenToBambooMaze = true;  
        }
        if(SceneManager.GetActiveScene().name != currentSceneName){
            lastSceneName = currentSceneName;
            currentSceneName = SceneManager.GetActiveScene().name;
        }
        if(!setPlayerPos&&lastSceneName == "BambooMazeScene"&& currentSceneName == "SampleScene"){
            PlayerController.Instance.transform.position = GameObject.Find("PlayerStartPos").transform.position;
            setPlayerPos = true;
        }

    }
}
