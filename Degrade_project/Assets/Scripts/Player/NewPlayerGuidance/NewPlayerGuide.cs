using System.Collections;
using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class NewPlayerGuide : MonoBehaviour
{
    public SpriteRenderer[] arrows;
    public SpriteRenderer[] keySprites;
    private AudioSource audioSource;
    public float fadeDuration = 0.5f;
    private int currentArrowIndex = 0;
    public int arrows_number = 6;
    public float WaitUntilDisappear = 5f;
    public float finalTransparency = 0.4f;
    private bool[] keyAnimationsRunning;
    public TextMeshProUGUI mapHintText;
    private List<KeyCode> keysBeingPressed = new List<KeyCode>();


    void Start()
    {
        if(mapHintText != null){
            // 隐藏大地图提示
            mapHintText.alpha = 0f;            
        }

        if (arrows.Length != arrows_number || keySprites.Length != arrows_number)
        {
            return;
        }

        keyAnimationsRunning = new bool[arrows_number];
        audioSource = gameObject.GetComponent<AudioSource>();

        for (int i = 0; i < arrows.Length; i++)
        {
            arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, 0);
            keySprites[i].color = new Color(keySprites[i].color.r, keySprites[i].color.g, keySprites[i].color.b, 0);
        }

        StartCoroutine(ShowArrowAndKey(currentArrowIndex));
    }

    IEnumerator ShowArrowAndKey(int index)
    {
        yield return StartCoroutine(FadeInArrowAndKey(index));

        if (!keyAnimationsRunning[index])
        {
            if (keySprites[index] != null)
            {
                StartCoroutine(KeyAnimation(keySprites[index], index));
                keyAnimationsRunning[index] = true;
            }
        }

        KeyCode keyToPress = GetKeyForArrow(index);
        bool keyPressed = false;

        while (!keyPressed)
        {
            if (Input.GetKeyDown(keyToPress))
            {
                // 如果按下了按键，立即触发
                keyPressed = true;
                PlayKeyPressSound();
            }
            else if (Input.GetKey(keyToPress))
            {
                // 如果按键被长按，加入到长按的列表中
                if (!keysBeingPressed.Contains(keyToPress))
                {
                    keysBeingPressed.Add(keyToPress);
                }
            }

            // 检查是否当前箭头的按键已经在长按列表中，如果是，立即触发
            if (keysBeingPressed.Contains(keyToPress))
            {
                keyPressed = true;
                PlayKeyPressSound();
            }

            yield return null;
        }

        yield return StartCoroutine(FadeOutArrowAndKey(index));

        if (currentArrowIndex + 1 < arrows.Length)
        {
            currentArrowIndex = (currentArrowIndex + 1) % arrows.Length;
            StartCoroutine(ShowArrowAndKey(currentArrowIndex));
        }
        else
        {
            StartCoroutine(FadeAllArrowsAndKeysToTransparency());
        }

        // 完成后从长按列表中移除已处理的按键
        keysBeingPressed.Remove(keyToPress);
    }


    void PlayKeyPressSound()
    {
        audioSource.Play();
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
            default: return KeyCode.None;
        }
    }

    IEnumerator FadeAllArrowsAndKeysToTransparency()
    {
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

        yield return new WaitForSeconds(WaitUntilDisappear);

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
        // 完成按键教程后显示大地图提示
        StartCoroutine(ShowMapHint());
        
    }
    IEnumerator ShowMapHint()
    {
        // 初始时隐藏大地图提示
        mapHintText.alpha = 0f;

        // 显示提示“按M打开大地图”并渐变显示
        mapHintText.text = "按M打开大地图";
        yield return StartCoroutine(FadeInText());

        // 等待玩家按下M键
        while (!Input.GetKeyDown(KeyCode.M))
        {
            yield return null;
        }

        // 玩家按下M键后，渐变消失提示
        yield return StartCoroutine(FadeOutText());

        // 显示提示“按M或ESC关闭大地图”并渐变显示
        mapHintText.text = "按M或ESC关闭大地图";
        yield return StartCoroutine(FadeInText());

        // 等待玩家按下M或ESC键来关闭大地图
        while (!Input.GetKeyDown(KeyCode.M) && !Input.GetKeyDown(KeyCode.Escape))
        {
            yield return null;
        }

        // 玩家按下M或ESC键后，渐变消失提示
        yield return StartCoroutine(FadeOutText());

        // 完成任务
        QuestUIManager.QuestManager.CompleteTask("", 1);
    }

    // 渐变显示文本
    IEnumerator FadeInText()
    {
        float timer = 0;
        fadeDuration = 0.2f;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            mapHintText.alpha = alpha;
            timer += Time.deltaTime;
            yield return null;
        }
        mapHintText.alpha = 1f;  // 确保最终透明度为1
    }

    // 渐变消失文本
    IEnumerator FadeOutText()
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            mapHintText.alpha = alpha;
            timer += Time.deltaTime;
            yield return null;
        }
        mapHintText.alpha = 0f;  // 确保最终透明度为0
    }

    IEnumerator KeyAnimation(SpriteRenderer keySprite, int index)
    {
        Vector3 normalScale = new Vector3(1f, 1f, 1f);
        Vector3 pressedScale = new Vector3(0.9f, 0.9f, 1);
        Color normalColor = Color.white;
        Color pressedColor = new Color(0.8f, 0.8f, 0.8f);

        float animationSpeed = 1.5f;
        float lerpTime = 0f;

        while (keySprite != null)
        {
            lerpTime += Time.deltaTime * animationSpeed;
            if (lerpTime > 1f)
            {
                lerpTime = 0f;
            }

            float scaleLerp = Mathf.PingPong(lerpTime, 1f);

            keySprite.transform.localScale = Vector3.Lerp(normalScale, pressedScale, scaleLerp);
            keySprite.color = Color.Lerp(normalColor, pressedColor, scaleLerp);

            yield return null;
        }
    }
}
