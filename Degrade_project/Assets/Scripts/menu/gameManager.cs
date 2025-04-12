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
    public bool isDead = false;
    public string currentSceneName;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (isDead != null && isDead)
        {
            Time.timeScale = 1;
            if (PlayerController.Instance != null)
            {
                PlayerController.Instance.TeleportTo(new Vector3(18.7f, -26.2f, -0.7f));
            }
            GameObject playerChar = GameObject.Find("PlayerCharacter");
            if (playerChar != null && playerChar.GetComponent<PlayerController>() != null)
            {
                playerChar.GetComponent<PlayerController>().PlayerHealth = 100;
                playerChar.GetComponent<PlayerController>().PlayerShield = 100;
            }
            isDead = false;
            return;
        }

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

        GameObject gameData = GameObject.Find("GameData");
        if (gameData != null && gameData.GetComponent<dataManager>() != null && 
            gameData.GetComponent<dataManager>().newFile != null && 
            gameData.GetComponent<dataManager>().newFile)
        {
            if (gameData != null)
            {
                int i = 0, j = 0;
                Save dataSave = gameData.GetComponent<dataManager>().tempSave;

                if (QuestUIManager.QuestManager != null && dataSave != null)
                {
                    foreach (QuestUIManager.Quest quest in QuestUIManager.QuestManager.quests)
                    {
                        if (i < dataSave.questsList.Count)
                        {
                            quest.isCompleted = dataSave.questsList[i].iscompleted;
                            i++;
                        }
                    }
                }

                if (ItemManager.itemManager != null && dataSave != null)
                {
                    foreach (InventoryItem itemStatus in ItemManager.itemManager.inventoryItems)
                    {
                        if (j < dataSave.itemList.Count)
                        {
                            itemStatus.item.itemID = dataSave.itemList[j].id;
                            itemStatus.amount = dataSave.itemList[j].amount;
                            itemStatus.isObtained = dataSave.itemList[j].isObtained;
                            j++;
                        }
                    }
                }

                GameObject playerCharacter = GameObject.Find("PlayerCharacter");
                if (playerCharacter != null && dataSave != null)
                {
                    playerCharacter.transform.localPosition = new Vector3(dataSave.x, dataSave.y, 0);
                    if (playerCharacter.GetComponent<PlayerController>() != null)
                    {
                        playerCharacter.GetComponent<PlayerController>().PlayerHealth = dataSave.PlayerHealth;
                        playerCharacter.GetComponent<PlayerController>().PlayerShield = dataSave.PlayerShield;
                    }
                }

                if (QuestUIManager.QuestManager != null && 
                    QuestUIManager.QuestManager.quests != null && 
                    QuestUIManager.QuestManager.quests.Count > 0)
                {
                    if (QuestUIManager.QuestManager.quests[0].isCompleted)
                    {
                        GameObject cameraContainer = GameObject.Find("CameraContainer");
                        if (cameraContainer != null && cameraContainer.GetComponent<SwitchBridgeCamera>() != null)
                        {
                            cameraContainer.GetComponent<SwitchBridgeCamera>().isBridgeCameraSwitched = true;
                        }

                        GameObject newPlayerBridge = GameObject.Find("NewPlayerBridge");
                        if (newPlayerBridge != null && newPlayerBridge.GetComponent<BridgeController>() != null)
                        {
                            Vector3 newPosition = newPlayerBridge.GetComponent<BridgeController>().NewPlayerBridge.position;
                            newPosition.z = -1.943357f;
                            newPlayerBridge.GetComponent<BridgeController>().NewPlayerBridge.position = newPosition;
                            newPlayerBridge.GetComponent<BridgeController>().isBridgeRaised = true;
                        }
                    }
                }
            }
        }
    }

    void Update()
    {
        GameObject playerChar = GameObject.Find("PlayerCharacter");
        if (playerChar != null && playerChar.GetComponent<PlayerController>() != null)
        {
            if (playerChar.GetComponent<PlayerController>().PlayerHealth <= 0)
            {
                if (loadScreen != null)
                {
                    loadScreen.SetActive(true);
                }
                Time.timeScale = 0;
                Debug.Log("dead!!!");
                return;
            }
        }
    }

    public void loadNextLevel()
    {
        timer = 0;
        GameObject gameData = GameObject.Find("GameData");
        if (gameData != null && gameData.GetComponent<dataManager>() != null)
        {
            gameData.GetComponent<dataManager>().newFile = false;
        }
        StartCoroutine(loadLevel(0));
    }

    public void openSet()
    {
        if (settingboard != null)
        {
            settingboard.SetActive(true);
        }
    }

    public void closeSet()
    {
        if (settingboard != null)
        {
            settingboard.SetActive(false);
        }
    }

    public void quit()
    {
        SceneManager.LoadScene("Menu");
    }

    public void exit()
    {
        Application.Quit();
    }

    public void openFileScene()
    {
        SceneManager.LoadScene("loadScene");
    }

    public void enterName()
    {
        if (nameinput != null && namepanel != null)
        {
            PlayerPrefs.SetString("PlayerName", nameinput.text);
            namepanel.SetActive(false);
        }
    }

    public void changeName()
    {
        if (namepanel != null)
        {
            namepanel.SetActive(true);
        }
    }

    public Save createSaveQuestandItem()
    {
        Save saveData = new Save();
        if (QuestUIManager.QuestManager != null && QuestUIManager.QuestManager.quests != null)
        {
            foreach (QuestUIManager.Quest quest in QuestUIManager.QuestManager.quests)
            {
                saveQuest temp = new saveQuest();
                temp.id = quest.id;
                temp.iscompleted = quest.isCompleted;
                saveData.questsList.Add(temp);
            }
        }

        if (ItemManager.itemManager != null && ItemManager.itemManager.inventoryItems != null)
        {
            foreach (InventoryItem item in ItemManager.itemManager.inventoryItems)
            {
                saveItem temp = new saveItem();
                temp.id = item.item.itemID;
                temp.amount = item.amount;
                temp.isObtained = item.isObtained;
                saveData.itemList.Add(temp);
            }
        }
        return saveData;
    }

    public void saveGame()
    {
        string savePath = Application.dataPath + "data.txt";
        Save save = createSaveQuestandItem();
        Scene scene = SceneManager.GetActiveScene();
        save.sceneIndex = scene.buildIndex;
        
        GameObject playerChar = GameObject.Find("PlayerCharacter");
        if (playerChar != null)
        {
            save.x = playerChar.transform.position.x;
            save.y = playerChar.transform.position.y;
            if (playerChar.GetComponent<PlayerController>() != null)
            {
                save.PlayerHealth = (int)playerChar.GetComponent<PlayerController>().PlayerHealth;
                save.PlayerShield = playerChar.GetComponent<PlayerController>().PlayerShield;
            }
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fileStream = File.Create(savePath);
        bf.Serialize(fileStream, save);
        fileStream.Close();
        Debug.Log("save complete");
    }

    public void loadGame()
    {
        string savePath = Application.dataPath + "data.txt";
        if (!File.Exists(savePath))
        {
            return;
        }

        BinaryFormatter bf = new BinaryFormatter();
        FileStream fileStream = File.Open(savePath, FileMode.Open);
        Save save = (Save)bf.Deserialize(fileStream);
        fileStream.Close();

        GameObject gameData = GameObject.Find("GameData");
        if (gameData != null && gameData.GetComponent<dataManager>() != null)
        {
            gameData.GetComponent<dataManager>().tempSave = save;
            gameData.GetComponent<dataManager>().newFile = true;
        }
    }

    public void loadLevel()
    {
        string savePath = Application.dataPath + "data.txt";
        if (!File.Exists(savePath))
        {
            Debug.Log("文件不存在！");
            return;
        }

        GameObject gameData = GameObject.Find("GameData");
        if (gameData != null && gameData.GetComponent<dataManager>() != null && 
            gameData.GetComponent<dataManager>().tempSave != null)
        {
            int id = gameData.GetComponent<dataManager>().tempSave.sceneIndex;
            timer = 0;
            StartCoroutine(loadLevel(id));
        }
    }

    public void deadRetry()
    {
        isDead = true;
        if (loadScreen != null)
        {
            loadScreen.SetActive(false);
        }
        Start();
    }

    IEnumerator loadLevel(int index)
    {
        if (loadScreen != null && slider != null && text != null)
        {
            loadScreen.SetActive(true);
            AsyncOperation operation = SceneManager.LoadSceneAsync(index);
            operation.allowSceneActivation = false;
            while (timer <= 100)
            {
                slider.value = timer / 100f;
                text.text = timer + "%";
                timer += 1;
                yield return new WaitForSeconds(0.02f);
            }
            operation.allowSceneActivation = true;
            isLoaded = true;
        }
    }
}

[System.Serializable]
public class Save
{
    public List<saveQuest> questsList = new List<saveQuest>();
    public List<saveItem> itemList = new List<saveItem>();
    public int sceneIndex;
    public float x;
    public float y;
    public int PlayerHealth;
    public int PlayerShield;
}

[System.Serializable]
public class saveQuest
{
    public int id;
    public bool iscompleted;
}

[System.Serializable]
public class saveItem
{
    public int id;
    public int amount;
    public bool isObtained;
}