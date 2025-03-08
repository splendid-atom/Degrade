using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.VisualScripting;
using UnityEngine.UIElements;
using UnityEngine.UI;
using JetBrains.Annotations;
using UnityEditor.Rendering;

public class gameManager : MonoBehaviour
{
    public GameObject settingboard;
    public static gameManager instance;
    public GameObject loadScreen;
    public UnityEngine.UI.Slider slider;
    public Text text;
    private int timer;
    public GameObject namepanel;
    public InputField nameinput;
    public bool isLoaded = false;
    void Awake()
    {
        instance = this;
    }
    void Start()
    {
        if (slider != null)
        {
            slider.interactable = false;
        }
        if (namepanel != null)
        {
            if (!PlayerPrefs.HasKey("PlayerName"))
            {
                namepanel.SetActive(true);
            }
            else Debug.Log(PlayerPrefs.GetString("PlayerName"));
        }
        if (GameObject.Find("GameData").GetComponent<dataManager>().newFile)
        {
            if (GameObject.Find("GameData") != null)
            {
                int i = 0, j = 0;
                Save dataSave = GameObject.Find("GameData").GetComponent<dataManager>().tempSave;
                //载入任务状态
                if (QuestUIManager.QuestManager != null)
                {
                    foreach (QuestUIManager.Quest quest in QuestUIManager.QuestManager.quests)
                    {
                        quest.isCompleted = dataSave.questsList[i].iscompleted;
                        i++;
                    }
                }
                //载入物品状态
                if (ItemManager.itemManager != null)
                {
                    foreach (InventoryItem itemStatus in ItemManager.itemManager.inventoryItems)
                    {
                        itemStatus.item.itemID = dataSave.itemList[j].id;
                        itemStatus.amount = dataSave.itemList[j].amount;
                        itemStatus.isObtained = dataSave.itemList[j].isObtained;
                        j++;
                    }
                }
                //修改玩家位置
                if (GameObject.Find("PlayerCharacter") != null)
                {
                    GameObject.Find("PlayerCharacter").transform.localPosition = new Vector3(dataSave.x, dataSave.y, 0);
                    GameObject.Find("PlayerCharacter").GetComponent<PlayerController>().PlayerHealth = dataSave.PlayerHealth;
                    GameObject.Find("PlayerCharacter").GetComponent<PlayerController>().PlayerShield = dataSave.PlayerShield;
                }
                if (QuestUIManager.QuestManager != null)
                {
                    if (QuestUIManager.QuestManager.quests[0].isCompleted)
                    {
                        //阻止镜头移动
                        if (GameObject.Find("CameraContainer") != null && GameObject.Find("CameraContainer").GetComponent<SwitchBridgeCamera>() != null)
                        {
                            GameObject.Find("CameraContainer").GetComponent<SwitchBridgeCamera>().isBridgeCameraSwitched = true;

                        }
                        //阻止镜头移动
                        if (GameObject.Find("NewPlayerBridge") != null)
                        {
                            Vector3 newPosition = GameObject.Find("NewPlayerBridge").GetComponent<BridgeController>().NewPlayerBridge.position;
                            newPosition.z = -1.943357f;
                            GameObject.Find("NewPlayerBridge").GetComponent<BridgeController>().NewPlayerBridge.position = newPosition;
                            GameObject.Find("NewPlayerBridge").GetComponent<BridgeController>().isBridgeRaised = true;
                        }
                    }
                }
            }
        }
    }
    //切换至游戏场景进度条
    public void loadNextLevel()
    {
        timer = 0;
        GameObject.Find("GameData").GetComponent<dataManager>().newFile = false;
        StartCoroutine(loadLevel(0));
    }
    //打开设置界面
    public void openSet()
    {
        settingboard.SetActive(true);
    }
    //关闭设置界面
    public void closeSet()
    {
        settingboard.SetActive(false);
    }
    //退出游戏界面
    public void quit()
    {
        SceneManager.LoadScene("Menu");
    }
    //退出游戏
    public void exit()
    {
        Application.Quit();
    }
    //进入存档界面
    public void openFileScene()
    {
        SceneManager.LoadScene("loadScene");
    }
    //输入玩家名
    public void enterName()
    {
        PlayerPrefs.SetString("PlayerName", nameinput.text);
        namepanel.SetActive(false);
    }
    //更改玩家名
    public void changeName()
    {
        namepanel.SetActive(true);
    }

    public Save createSaveQuestandItem()
    {
        Save saveData = new Save();
        //存入任务信息
        foreach (QuestUIManager.Quest quest in QuestUIManager.QuestManager.quests)
        {
            saveQuest temp = new saveQuest();
            temp.id = quest.id;
            temp.iscompleted = quest.isCompleted;
            saveData.questsList.Add(temp);
        }
        //存入物品信息
        foreach (InventoryItem item in ItemManager.itemManager.inventoryItems)
        {
            saveItem temp = new saveItem();
            temp.id = item.item.itemID;
            temp.amount = item.amount;
            temp.isObtained = item.isObtained;
            saveData.itemList.Add(temp);
        }
        return saveData;
    }


    //存档游戏内容

    public void saveGame()
    {
        string savePath = Path.Combine(Application.dataPath, "data", "data.txt");
        Save save = createSaveQuestandItem();
        Scene scene = SceneManager.GetActiveScene();
        save.sceneIndex = scene.buildIndex;//存入当前场景索引
        save.x = GameObject.Find("PlayerCharacter").transform.position.x;
        save.y = GameObject.Find("PlayerCharacter").transform.position.y;
        //存入人物坐标位置
        save.PlayerHealth = (int)GameObject.Find("PlayerCharacter").GetComponent<PlayerController>().PlayerHealth;
        save.PlayerShield = GameObject.Find("PlayerCharacter").GetComponent<PlayerController>().PlayerShield;
        //存入人物生命护盾值
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fileStream = File.Create(savePath);
        bf.Serialize(fileStream, save);
        fileStream.Close();
        Debug.Log("save complete");
    }

    public void loadGame()
    {
        string savePath = Path.Combine(Application.dataPath, "data", "data.txt");
        if (!File.Exists(savePath))
        {
            return;
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream fileStream = File.Open(savePath, FileMode.Open);
        Save save = (Save)bf.Deserialize(fileStream);
        fileStream.Close();
        GameObject.Find("GameData").GetComponent<dataManager>().tempSave = save;
        GameObject.Find("GameData").GetComponent<dataManager>().newFile = true;
    }

    public void loadLevel()
    {
        string savePath = Path.Combine(Application.dataPath, "data", "data.txt");
        if (!File.Exists(savePath))
        {
            Debug.Log("文件不存在！");
            return;
        }
        if (GameObject.Find("GameData").GetComponent<dataManager>() != null)
        {
            int id = GameObject.Find("GameData").GetComponent<dataManager>().tempSave.sceneIndex;
            timer = 0;
            StartCoroutine(loadLevel(id));
        }
    }

    //进度条展示
    IEnumerator loadLevel(int index)
    {
        loadScreen.SetActive(true);
        AsyncOperation operation = SceneManager.LoadSceneAsync(index);
        operation.allowSceneActivation = false;
        while (timer <= 100)
        {
            slider.value = timer / 100f;
            // Debug.Log(slider.value);
            // Debug.Log(timer);
            text.text = timer + "%";
            timer += 1;
            yield return new WaitForSeconds(0.05f);

        }
        operation.allowSceneActivation = true;
        isLoaded = true;
    }
}
[System.Serializable]
public class Save//存档类
{
    public List<saveQuest> questsList = new List<saveQuest>();//存储任务完成情况
    public List<saveItem> itemList = new List<saveItem>();//存储物品状况
    public int sceneIndex;//存储场景索引
    public float x;//存储人物x坐标
    public float y;//存储人物y坐标
    public int PlayerHealth;    // 生命值
    public int PlayerShield;    // 护甲值
}

[System.Serializable]
public class saveQuest
{
    public int id; //存储任务id
    public bool iscompleted;//存储任务完成情况
}
[System.Serializable]
public class saveItem
{
    public int id;//存储物品id
    public int amount;//存储物品数量
    public bool isObtained;//存储获得状态
}

