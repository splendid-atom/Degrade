using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // 引入TextMeshPro命名空间
public class VillageNpcController : MonoBehaviour
{
    public static VillageNpcController instance;
    private GameObject VillageNpc;
    private Renderer npcRenderer;
    private BoxCollider2D npcCollider; // 用来引用BoxCollider2D
    private float targetAlpha = 1f; // 渐变的目标透明度（完全显示）
    public float fadeSpeed = 0.5f; // 渐变速度
    private bool isFadingIn = false; // 当前是否在渐变显示
    private bool isFadingOut = false; // 当前是否在渐变消失
    private bool canTriggerNpc = false; // 玩家是否可以触发NPC的渐变显示

    private BoxCollider2D triggerCollider; // 触发器的BoxCollider2D
    private GameObject player; // 玩家对象
    public bool isTalking = false;// 是否正在与玩家对话
    private TextMeshProUGUI PlayerInteractHint;
    // 新增List，用于存储对话内容
    public List<Dialogue> villageNpcDialogues;  // 管理每个对话的说话者和内容
    public List<Dialogue> villageNpcVisitedDialogues;  //第二次对话
    public bool isVisited = false;
    public string npcName;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogError("More than one instance of VillageNpcController found!");
        }
    }
    void Start()
    {
        PlayerInteractHint = GameObject.Find("PlayerInteractHint").GetComponent<TextMeshProUGUI>();
        PlayerInteractHint.gameObject.SetActive(false);
        player = GameObject.Find("PlayerCharacter");
        VillageNpc = GameObject.Find("VillageNpc");

        if (VillageNpc == null)
        {
            Debug.LogError("VillageNpc object not found!");
        }

        npcRenderer = VillageNpc.GetComponent<Renderer>();

        if (npcRenderer == null)
        {
            Debug.LogError("Renderer component not found on VillageNpc!");
        }

        npcCollider = VillageNpc.GetComponent<BoxCollider2D>(); // 获取 BoxCollider2D 组件

        if (npcCollider == null)
        {
            Debug.LogError("BoxCollider2D component not found on VillageNpc!");
        }

        // 获取触发器Collider
        triggerCollider = GameObject.Find("VillageHouseTrigger").GetComponent<BoxCollider2D>();

        if (triggerCollider == null)
        {
            Debug.LogError("VillageHouseTrigger object or BoxCollider2D not found!");
        }

        // 设置初始透明度为0（完全透明）
        Color startColor = npcRenderer.material.color;
        startColor.a = 0f;
        npcRenderer.material.color = startColor;

        // 禁用碰撞器，因为NPC一开始是透明的，不需要碰撞
        if (npcCollider != null)
        {
            npcCollider.enabled = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log("isTalking:"+isTalking);
        // 检查玩家是否进入触发区并允许提示显示
        if (canTriggerNpc&&!isTalking)
        {
            // 显示提示文本：启用父对象
            PlayerInteractHint.gameObject.SetActive(true);
        }
        if(!canTriggerNpc&&!isTalking){
            PlayerInteractHint.gameObject.SetActive(false);
        }
        // 如果玩家在触发器范围内，才允许按F触发渐变
        if (canTriggerNpc && Input.GetKeyDown(KeyCode.F))
        {
            
            // 隐藏提示文本：禁用父对象
            PlayerInteractHint.gameObject.SetActive(false);
            if (!isFadingIn && !isFadingOut)  // 确保没有正在进行的渐变
            {
                // 检查当前透明度，决定是渐变显示还是渐变消失
                Color currentColor = npcRenderer.material.color;
                if (currentColor.a <= 0f+0.2f) // 当前是透明，执行FadeIn
                {
                    StartCoroutine(FadeIn());
                    isTalking = true; // 标记为正在对话
                    // 完成任务
                    QuestUIManager.QuestManager.CompleteTask("", 2);
                    // ShowDialogue();
                }
                if(currentColor.a >= 1f-0.2f) // 当前是显示，执行FadeOut
                {
                    StartCoroutine(FadeOut());
                    isTalking = false; // 标记为不再对话
           
                }
            }
        }
    }
    // 显示对话内容

    public void FadeNpc(){
        if (!isFadingIn && !isFadingOut)  // 确保没有正在进行的渐变
        {
            // 检查当前透明度，决定是渐变显示还是渐变消失
            Color currentColor = npcRenderer.material.color;
            if(currentColor.a >= 1f-0.2f) // 当前是显示，执行FadeOut
            {
                StartCoroutine(FadeOut());
                isTalking = false; // 标记为不再对话
            }
        }
    }
    // 渐变显示（协程）
    IEnumerator FadeIn()
    {
        isFadingIn = true;

        Color currentColor = npcRenderer.material.color;
        float currentAlpha = currentColor.a;

        // 逐步增加透明度直到达到目标透明度
        while (currentAlpha < targetAlpha)
        {
            currentAlpha += fadeSpeed * Time.deltaTime; // 使用Time.deltaTime使渐变效果帧率独立
            currentAlpha = Mathf.Clamp(currentAlpha, 0f, targetAlpha); // 限制透明度范围

            currentColor.a = currentAlpha;
            npcRenderer.material.color = currentColor;

            // 调试：查看透明度的变化
            // Debug.Log("Current Alpha (FadeIn): " + currentAlpha);

            // 等待下一帧继续执行
            yield return null;
        }

        // 确保透明度最终为目标值
        currentColor.a = targetAlpha;
        npcRenderer.material.color = currentColor;

        // NPC显示完全后，启用碰撞器
        if (npcCollider != null)
        {
            npcCollider.enabled = true;
        }

        isFadingIn = false;
    }

    // 渐变消失（协程）
    IEnumerator FadeOut()
    {
        isFadingOut = true;

        Color currentColor = npcRenderer.material.color;
        float currentAlpha = currentColor.a;

        // 逐步减少透明度直到完全透明
        while (currentAlpha > 0f)
        {
            currentAlpha -= fadeSpeed * Time.deltaTime; // 使用Time.deltaTime使渐变效果帧率独立
            currentAlpha = Mathf.Clamp(currentAlpha, 0f, targetAlpha); // 限制透明度范围

            currentColor.a = currentAlpha;
            npcRenderer.material.color = currentColor;

            // 调试：查看透明度的变化
            // Debug.Log("Current Alpha (FadeOut): " + currentAlpha);

            // 等待下一帧继续执行
            yield return null;
        }

        // 确保透明度最终为0
        currentColor.a = 0f;
        npcRenderer.material.color = currentColor;

        // NPC完全透明后，禁用碰撞器
        if (npcCollider != null)
        {
            npcCollider.enabled = false;
        }

        isFadingOut = false;
    }

    // 玩家进入触发器
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 判断触发器内是否是玩家
        if (other.gameObject == player)
        {
            canTriggerNpc = true;
            Debug.Log("Player entered the trigger zone. V can now trigger NPC fade.");
        }
    }

    // 玩家离开触发器
    private void OnTriggerExit2D(Collider2D other)
    {
        // 判断触发器内是否是玩家
        if (other.gameObject == player)
        {
            canTriggerNpc = false;
            Debug.Log("Player left the trigger zone. V can no longer trigger NPC fade.");
        }
    }
}
