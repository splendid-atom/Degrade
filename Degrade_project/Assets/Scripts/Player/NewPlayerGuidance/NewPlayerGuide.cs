using System.Collections;
using UnityEngine;

public class NewPlayerGuide : MonoBehaviour
{
    public SpriteRenderer[] arrows;  // 箭头数组
    public SpriteRenderer[] keySprites; // 按键数组
    private AudioSource audioSource; // 用来播放音效的音频源
    public float fadeDuration = 2f;  // 渐变时间
    private int currentArrowIndex = 0;
    public int arrows_number = 6;    // 箭头数量
    public float WaitUntilDisappear = 5f; // 持续显示的时间
    public float finalTransparency = 0.4f; // 最终透明度
    private bool[] keyAnimationsRunning;  // 用来记录每个按键的动画是否正在进行

    void Start()
    {
        if (arrows.Length != arrows_number || keySprites.Length != arrows_number)
        {
            return;
        }

        keyAnimationsRunning = new bool[arrows_number]; // 初始化动画状态数组

        // 添加 AudioSource 组件
        audioSource = gameObject.GetComponent<AudioSource>();

        // 初始化箭头和按键为透明
        for (int i = 0; i < arrows.Length; i++)
        {
            arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, 0);
            keySprites[i].color = new Color(keySprites[i].color.r, keySprites[i].color.g, keySprites[i].color.b, 0);
        }

        // 开始显示第一个箭头和按键
        StartCoroutine(ShowArrowAndKey(currentArrowIndex));
    }

    IEnumerator ShowArrowAndKey(int index)
    {
        // 显示当前箭头和按键并等待其淡入动画完成
        yield return StartCoroutine(FadeInArrowAndKey(index));

        // 如果按键动画没有进行过，则启动动画
        if (!keyAnimationsRunning[index])
        {
            if (keySprites[index] != null)  // 检查对象是否已销毁
            {
                StartCoroutine(KeyAnimation(keySprites[index], index));  // 启动按键动画
                keyAnimationsRunning[index] = true;  // 设置动画已开始
            }
        }

        // 等待玩家按下对应的按键
        KeyCode keyToPress = GetKeyForArrow(index);  // 获取当前箭头对应的按键
        bool keyPressed = false;

        // 等待玩家按下指定的按键
        while (!keyPressed)
        {
            if (Input.GetKeyDown(keyToPress))  // 检查玩家是否按下了对应按键
            {
                keyPressed = true;  // 设置标志为按下了按键
                PlayKeyPressSound();  // 播放按键音效
            }
            yield return null;
        }

        // 玩家按下了按键，箭头和按键渐渐消失
        yield return StartCoroutine(FadeOutArrowAndKey(index));

        // 显示下一个箭头和按键
        if (currentArrowIndex + 1 < arrows.Length)
        {
            // 更新索引并显示下一个箭头
            currentArrowIndex = (currentArrowIndex + 1) % arrows.Length;
            StartCoroutine(ShowArrowAndKey(currentArrowIndex));  // 递归调用以显示下一个箭头
        }
        else
        {
            // 达到最大循环次数后，一起渐变显示所有箭头和按键到指定透明度
            StartCoroutine(FadeAllArrowsAndKeysToTransparency());
        }
    }

    // 播放按键按下音效
    void PlayKeyPressSound()
    {
        audioSource.Play();  // 播放音效
    }

    IEnumerator FadeInArrowAndKey(int index)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            arrows[index].color = new Color(arrows[index].color.r, arrows[index].color.g, arrows[index].color.b, alpha);
            if (keySprites[index] != null)
            {
                keySprites[index].color = new Color(keySprites[index].color.r, keySprites[index].color.g, keySprites[index].color.b, alpha);
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator FadeOutArrowAndKey(int index)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            arrows[index].color = new Color(arrows[index].color.r, arrows[index].color.g, arrows[index].color.b, alpha);
            if (keySprites[index] != null)
            {
                keySprites[index].color = new Color(keySprites[index].color.r, keySprites[index].color.g, keySprites[index].color.b, alpha);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        if (keySprites[index] != null)
        {
            Destroy(keySprites[index].gameObject);
        }

    }

    // 根据箭头的索引，返回对应的按键
    KeyCode GetKeyForArrow(int index)
    {
        switch (index)
        {
            case 0: return KeyCode.W;
            case 1: return KeyCode.S;
            case 2: return KeyCode.D;
            case 3: return KeyCode.A;
            case 4: return KeyCode.E;
            case 5: return KeyCode.Q;
            default: return KeyCode.None;  // 如果索引无效，返回None
        }
    }

    IEnumerator FadeAllArrowsAndKeysToTransparency()
    {
        // 渐变到最终透明度
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, finalTransparency, timer / fadeDuration);
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
                if (keySprites[i] != null)
                {
                    keySprites[i].color = new Color(keySprites[i].color.r, keySprites[i].color.g, keySprites[i].color.b, alpha);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // 等待5秒
        yield return new WaitForSeconds(WaitUntilDisappear);

        // 渐变消失
        timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(finalTransparency, 0, timer / fadeDuration);
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
                if (keySprites[i] != null)
                {
                    keySprites[i].color = new Color(keySprites[i].color.r, keySprites[i].color.g, keySprites[i].color.b, alpha);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }
        QuestUIManager.QuestManager.CompleteTask("", 1);//完成新手教程
    }

    // 实现按键动画效果
    IEnumerator KeyAnimation(SpriteRenderer keySprite, int index)
    {
        Vector3 normalScale = new Vector3(1f, 1f, 1f);  // 正常时的缩放
        Vector3 pressedScale = new Vector3(0.9f, 0.9f, 1);  // 按下时的缩放
        Color normalColor = Color.white;  // 正常时的颜色
        Color pressedColor = new Color(0.8f, 0.8f, 0.8f);  // 按下时的颜色

        float animationSpeed = 1.5f;  // 动画的速度
        float lerpTime = 0f; // 用于平滑过渡的时间

        while (keySprite != null)  // 在动画循环中检查是否为null
        {
            lerpTime += Time.deltaTime * animationSpeed;
            if (lerpTime > 1f)
            {
                lerpTime = 0f;  // 重置动画时间
            }

            // 使用PingPong函数来实现按下和恢复的动画循环
            float scaleLerp = Mathf.PingPong(lerpTime, 1f);

            // 按键的缩放和颜色平滑过渡
            keySprite.transform.localScale = Vector3.Lerp(normalScale, pressedScale, scaleLerp);
            keySprite.color = Color.Lerp(normalColor, pressedColor, scaleLerp);

            yield return null;
        }
    }
}
