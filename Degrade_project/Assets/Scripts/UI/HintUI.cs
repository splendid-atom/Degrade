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
    private Button hintButton;
    private bool isTalkingPhone = false; // 是否正在打电话
    public bool isTalkingOver = false; // 电话是否结束
    private TextMeshProUGUI HintTitle;
    private TextMeshProUGUI HintButtonContent;
    private AudioSource audioSource;  // 添加音频源组件
    public AudioClip callingSound;    // 来电音效
    public AudioClip pickupSound;    // 拿起老式电话音效
    public AudioClip slamdownSound;    // 放下老式电话音效

    // 闪烁相关的变量
    private float blinkTimer = 0f;
    private bool isButtonVisible = true; // 按钮是否可见
    private float blinkInterval = 0.5f; // 闪烁间隔

    void Awake()
    {
        instance = this;
        // DontDestroyOnLoad(gameObject);
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

        audioSource = GameObject.Find("HintUI").GetComponent<AudioSource>();
        audioSource.loop = true;  // 循环播放音效
    }

    void Update()
    {
        hintButton.interactable = isTalkingPhone ? false : true;

        if (isTalkingOver)
        {
            HintButtonContent.text = "通话结束";
            isTalkingOver = false;
            audioSource.PlayOneShot(slamdownSound);  // 播放接听电话音效
        }

        // 如果还没有接听电话并且按钮正在闪烁+场景切换为游戏画面
        if (!isTalkingPhone&&gameManager.instance.isLoaded)
        {
            BlinkButton();
            
            // 如果还没有接听电话，播放来电音效
            if (!audioSource.isPlaying)
            {
                audioSource.clip = callingSound;
                audioSource.Play();  // 播放来电音效
            }
        }

    }

    void OnHintButtonClick()
    {
        isTalkingPhone = !isTalkingPhone;
        hintImage.texture = isTalkingPhone ? talkingPhoneSprite.texture : callingPhoneSprite.texture;
        HintButtonContent.text = isTalkingPhone ? "通话中..." : "接听";
        NewPlayerGuide.instance.isGuiding = isTalkingPhone;

        // 当玩家接听电话时，停止来电音效，开始通话音效
        if (isTalkingPhone)
        {
            audioSource.Stop();  // 停止当前音效（来电音效）
            audioSource.PlayOneShot(pickupSound);  // 播放接听电话音效
        }
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
