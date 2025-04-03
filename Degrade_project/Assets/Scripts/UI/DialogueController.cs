using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue
{
    public bool isSpeakerPlayer;
    public string dialogueText;

    public Dialogue(bool isSpeakerPlayer, string dialogueText)
    {
        this.isSpeakerPlayer = isSpeakerPlayer;
        this.dialogueText = dialogueText;
    }
}

public class DialogueController : MonoBehaviour
{
    public static DialogueController instance;
    private TextMeshProUGUI playerDialogue;
    private TextMeshProUGUI speakerName;
    private string currentNpcName;
    private GameObject DialogueContainer;
    private List<Dialogue> currentDialogues;
    private List<Dialogue> currentVisitedDialogues;
    private Button dialogueButton;
    private int currentDialogueIndex = 0;
    public List<GameObject> uiElementsToHide;
    private RectMask2D mask;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerDialogue = GameObject.Find("PlayerDialogue")?.GetComponent<TextMeshProUGUI>();
        speakerName = GameObject.Find("SpeakerName")?.GetComponent<TextMeshProUGUI>();
        DialogueContainer = GameObject.Find("DialogueContainer");
        if (VillageNpcController.instance != null)
        {
            currentDialogues = VillageNpcController.instance.villageNpcDialogues;
            currentVisitedDialogues = VillageNpcController.instance.villageNpcVisitedDialogues;
        }
        mask = GameObject.Find("DialogueContainerMask")?.GetComponent<RectMask2D>();
        if (DialogueContainer != null)
        {
            dialogueButton = DialogueContainer.GetComponent<Button>();
        }

        if (dialogueButton != null)
        {
            dialogueButton.onClick.AddListener(OnDialogueContainerClick);
        }

        if (mask != null)
        {
            mask.padding = new Vector4(1300, 0, 0, 0);
        }
    }

    void Update()
    {
        switchingCurrentDialogues();
        ShowDialogueContainer();

        if (isHavingDialogue())
        {
            HideUIElements();
        }

        if (currentDialogues != null && currentDialogues.Count > 0 && 
            playerDialogue != null && speakerName != null)
        {
            playerDialogue.text = currentDialogues[currentDialogueIndex].dialogueText;
            speakerName.text = currentDialogues[currentDialogueIndex].isSpeakerPlayer 
                ? (PlayerController.Instance != null ? PlayerController.Instance.PlayerName : "") 
                : currentNpcName;
        }

        if (VillageNpcController.instance != null && 
            VillageNpcController.instance.isVisited && 
            VillageNpcController.instance.canTriggerNpc)
        {
            currentDialogues = currentVisitedDialogues;
        }
    }

    public bool isHavingDialogue()
    {
        if ((VillageNpcController.instance != null && VillageNpcController.instance.isTalking) ||
            (NewPlayerGuide.instance != null && NewPlayerGuide.instance.isGuiding) ||
            (SwitchToMazeSceneTrigger.instance != null && SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger) ||
            (HeepAnimation.instance != null && HeepAnimation.instance.isHeepDialogue) ||
            (RiverPortalAnimation.instance != null && (RiverPortalAnimation.instance.isTalking || 
                                                      RiverPortalAnimation.instance.isSecondTalking)) ||
            (PortalAnimation.instance != null && (PortalAnimation.instance.isTalking || 
                                                 PortalAnimation.instance.isSecondTalking)) ||
            (FetchItemController.instance != null && FetchItemController.instance.isTalking))
        {
            if (VillageNpcController.instance != null && VillageNpcController.instance.isTalking)
            {
                currentNpcName = VillageNpcController.instance.npcName;
            }
            else if (NewPlayerGuide.instance != null)
            {
                currentNpcName = NewPlayerGuide.instance.npcName;
            }
            return true;
        }
        return false;
    }

    void switchingCurrentDialogues()
    {
        if (VillageNpcController.instance != null && VillageNpcController.instance.isVisited)
        {
            currentDialogues = VillageNpcController.instance.villageNpcVisitedDialogues;
        }
        if (NewPlayerGuide.instance != null && NewPlayerGuide.instance.isGuiding)
        {
            currentDialogues = NewPlayerGuide.instance.NewPlayerDialogues;
        }
        if (SwitchToMazeSceneTrigger.instance != null && SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger)
        {
            currentDialogues = SwitchToMazeSceneTrigger.instance.bambooMazeDialogues;
        }
        if (HeepAnimation.instance != null && HeepAnimation.instance.isHeepDialogue)
        {
            currentDialogues = HeepAnimation.instance.HeepDialogues;
        }
        if (RiverPortalAnimation.instance != null && RiverPortalAnimation.instance.isTalking)
        {
            currentDialogues = RiverPortalAnimation.instance.PollutedRiverDialogues;
        }
        if (RiverPortalAnimation.instance != null && RiverPortalAnimation.instance.isSecondTalking)
        {
            currentDialogues = RiverPortalAnimation.instance.PollutedRiverReturnDialogues;
        }
        if (PortalAnimation.instance != null && PortalAnimation.instance.isTalking)
        {
            currentDialogues = PortalAnimation.instance.DegradeBambooDialogues;
        }
        if (PortalAnimation.instance != null && PortalAnimation.instance.isSecondTalking)
        {
            currentDialogues = PortalAnimation.instance.SecondDegradeBambooDialogues;
        }
        if (FetchItemController.instance != null && FetchItemController.instance.isTalking)
        {
            currentDialogues = FetchItemController.instance.CollectedTimePieceDialogues;
        }
    }

    void OnDialogueContainerClick()
    {
        if (currentDialogues != null)
        {
            Debug.Log("currentDialogue.Count" + currentDialogues.Count);

            if (currentDialogueIndex < currentDialogues.Count - 1)
            {
                currentDialogueIndex++;
            }
            else
            {
                if (VillageNpcController.instance != null && VillageNpcController.instance.isTalking)
                {
                    VillageNpcController.instance.isTalking = false;
                    VillageNpcController.instance.FadeNpc();
                    if (!VillageNpcController.instance.isVisited)
                    {
                        if (QuestUIManager.QuestManager != null)
                        {
                            QuestUIManager.QuestManager.CompleteTask("", 3);
                        }
                        if (ItemManager.itemManager != null)
                        {
                            ItemManager.itemManager.AddItem(5, 5);
                        }
                    }
                    VillageNpcController.instance.isVisited = true;
                }
                if (NewPlayerGuide.instance != null && NewPlayerGuide.instance.isGuiding)
                {
                    NewPlayerGuide.instance.isGuiding = false;
                    NewPlayerGuide.instance.isNewPlayerGuiding = true;
                    if (HintUI.instance != null)
                    {
                        HintUI.instance.isTalkingOver = true;
                    }
                }
                if (SwitchToMazeSceneTrigger.instance != null && SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger)
                {
                    SwitchToMazeSceneTrigger.instance.isInMazeSwitchTrigger = false;
                    SwitchToMazeSceneTrigger.instance.isStartSwitchScene = true;
                }
                if (HeepAnimation.instance != null && HeepAnimation.instance.isHeepDialogue)
                {
                    HeepAnimation.instance.isHeepDialogue = false;
                    if (QuestUIManager.QuestManager != null)
                    {
                        QuestUIManager.QuestManager.CompleteTask("", 4);
                    }
                    HeepAnimation.instance.OnDialogueEnd();
                }
                if (RiverPortalAnimation.instance != null && RiverPortalAnimation.instance.isTalking)
                {
                    RiverPortalAnimation.instance.isTalking = false;
                    RiverPortalAnimation.instance.OnDialogueEnd();
                }
                if (RiverPortalAnimation.instance != null && RiverPortalAnimation.instance.isSecondTalking)
                {
                    RiverPortalAnimation.instance.isSecondTalking = false;
                    RiverPortalAnimation.instance.OnSecondDialogueEnd();
                }
                if (PortalAnimation.instance != null && PortalAnimation.instance.isTalking)
                {
                    PortalAnimation.instance.isTalking = false;
                    PortalAnimation.instance.OnDialogueEnd();
                }
                if (PortalAnimation.instance != null && PortalAnimation.instance.isSecondTalking)
                {
                    PortalAnimation.instance.isSecondTalking = false;
                    PortalAnimation.instance.OnSecondDialogueEnd();
                }
                if (FetchItemController.instance != null && FetchItemController.instance.isTalking)
                {
                    FetchItemController.instance.isTalking = false;
                    if (QuestUIManager.QuestManager != null)
                    {
                        QuestUIManager.QuestManager.CompleteTask("", 7);
                    }
                }

                currentDialogueIndex = 0;
                ShowUIElements();
                if (mask != null)
                {
                    mask.padding = new Vector4(1300, 0, 0, 0);
                }
            }
        }
    }

    void HideUIElements()
    {
        if (uiElementsToHide != null)
        {
            foreach (GameObject uiElement in uiElementsToHide)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(false);
                }
            }
        }
        if (BigMapController.instance != null)
        {
            BigMapController.instance.CloseMap();
        }
    }

    void ShowUIElements()
    {
        if (uiElementsToHide != null)
        {
            foreach (GameObject uiElement in uiElementsToHide)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(true);
                }
            }
        }
    }

    void ShowDialogueContainer()
    {
        if (VillageNpcController.instance != null && VillageNpcController.instance.isTalking)
        {
            StartCoroutine(DelayedDialogueContainer());
        }
        else if (mask != null)
        {
            mask.padding = new Vector4(isHavingDialogue() ? 0 : 1300, 0, 0, 0);
        }
    }

    IEnumerator DelayedDialogueContainer()
    {
        yield return new WaitForSeconds(1.5f);
        if (mask != null)
        {
            mask.padding = new Vector4(isHavingDialogue() ? 0 : 1300, 0, 0, 0);
        }
    }
}