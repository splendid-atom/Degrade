using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class HintUI : MonoBehaviour
{
    public static HintUI instance;
    private RawImage hintImage;
    public Sprite callingPhoneSprite;
    public Sprite talkingPhoneSprite;
    private Button hintButton; // 用来存储Button组件
    private bool isTalkingPhone = false; // 是否正在打电话
    public bool isTalkingOver = false; // 电话是否结束
    private TextMeshProUGUI HintTitle; // 通话栏标题
    private TextMeshProUGUI HintButtonContent; // 通话栏内容

    // 闪烁相关的变量
    private float blinkTimer = 0f;
    private bool isButtonVisible = true; // 按钮是否可见
    private float blinkInterval = 0.5f; // 闪烁间隔

    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        hintImage = GameObject.Find("HintImage").GetComponent<RawImage>();
        hintButton = GameObject.Find("HintButton").GetComponent<Button>();
        HintTitle = GameObject.Find("HintTitle").GetComponent<TextMeshProUGUI>();
        HintButtonContent = GameObject.Find("HintButtonContent").GetComponent<TextMeshProUGUI>();

        // 给Button添加点击事件监听器
        if (hintButton != null)
        {
            hintButton.onClick.AddListener(OnHintButtonClick);
        }
    }

    // Update is called once per frame
    void Update()
    {
        hintButton.interactable = isTalkingPhone ? false : true;

        if (isTalkingOver)
        {
            HintButtonContent.text = "通话结束";
            isTalkingOver = false;
        }

        // 如果还没有接听电话并且按钮正在闪烁
        if (!isTalkingPhone)
        {
            BlinkButton();
        }
    }

    void OnHintButtonClick()
    {
        isTalkingPhone = !isTalkingPhone;
        hintImage.texture = isTalkingPhone ?
            talkingPhoneSprite.texture : callingPhoneSprite.texture;
        HintButtonContent.text = isTalkingPhone ? "通话中..." : "接听";
        NewPlayerGuide.instance.isGuiding = isTalkingPhone;
    }

    // 控制按钮的闪烁效果
    private void BlinkButton()
    {
        blinkTimer += Time.deltaTime; // 增加时间

        // 如果时间超过了闪烁间隔
        if (blinkTimer >= blinkInterval)
        {
            // 每次闪烁切换按钮的透明度
            isButtonVisible = !isButtonVisible;

            // 设置按钮颜色
            ColorBlock colorBlock = hintButton.colors;
            colorBlock.normalColor = new Color(colorBlock.normalColor.r, colorBlock.normalColor.g, colorBlock.normalColor.b, isButtonVisible ? 1f : 0.5f);
            hintButton.colors = colorBlock;

            // 重置计时器
            blinkTimer = 0f;
        }
    }
}
