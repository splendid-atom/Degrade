using System.Collections;
using UnityEngine;

public class NewPlayerGuide : MonoBehaviour
{
    public SpriteRenderer[] arrows;
    public float fadeDuration = 2f;
    private int currentArrowIndex = 0;
    private int loopCounter = 0;
    public int maxLoops = 2; // 设置循环的最大次数
    public float finalTransparency = 0.4f; // 最终透明度
    public int arrows_number = 6;
    public float WaitUntilDisappear = 5f;
    void Start()
    {
        if (arrows.Length != arrows_number)
        {
            return;
        }

        for (int i = 0; i < arrows.Length; i++)
        {
            arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, 0);
        }

        StartCoroutine(FadeArrowIn(currentArrowIndex));
    }

    IEnumerator FadeArrowIn(int index)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, 1, timer / fadeDuration);
            arrows[index].color = new Color(arrows[index].color.r, arrows[index].color.g, arrows[index].color.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        StartCoroutine(FadeArrowOut(index));
    }

    IEnumerator FadeArrowOut(int index)
    {
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(1, 0, timer / fadeDuration);
            arrows[index].color = new Color(arrows[index].color.r, arrows[index].color.g, arrows[index].color.b, alpha);
            timer += Time.deltaTime;
            yield return null;
        }

        currentArrowIndex = (currentArrowIndex + 1) % arrows.Length;
        loopCounter++;

        // 检查是否达到最大循环次数
        if (loopCounter < maxLoops * arrows.Length)
        {
            StartCoroutine(FadeArrowIn(currentArrowIndex));
        }
        else if (loopCounter == maxLoops * arrows.Length)
        {
            // 达到最大循环次数后，一起渐变显示所有箭头到指定透明度
            StartCoroutine(FadeAllArrowsToTransparency());
        }
    }

    IEnumerator FadeAllArrowsToTransparency()
    {
        // 渐变到最终透明度
        float timer = 0;
        while (timer < fadeDuration)
        {
            float alpha = Mathf.Lerp(0, finalTransparency, timer / fadeDuration);
            for (int i = 0; i < arrows.Length; i++)
            {
                arrows[i].color = new Color(arrows[i].color.r, arrows[i].color.g, arrows[i].color.b, alpha);
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
            }
            timer += Time.deltaTime;
            yield return null;
        }
    }

}
