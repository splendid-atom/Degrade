using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpriteToRawImageAnimation : MonoBehaviour
{
    public RawImage rawImage;      // UI上的RawImage组件
    public Sprite[] spriteFrames;  // 你的Sprite动画帧
    public float frameRate = 1/6.0f; // 每帧间隔时间

    private List<Texture2D> textures = new List<Texture2D>();

    void Start()
    {
        // 将Sprite转换为Texture2D
        foreach (var sprite in spriteFrames)
        {
            textures.Add(SpriteToTexture(sprite));
        }

        StartCoroutine(PlayAnimation());
    }

    IEnumerator PlayAnimation()
    {
        int index = 0;
        while (true)
        {
            rawImage.texture = textures[index]; // 切换RawImage的Texture
            index = (index + 1) % textures.Count;
            yield return new WaitForSeconds(frameRate);
        }
    }

    Texture2D SpriteToTexture(Sprite sprite)
    {
        int width = (int)sprite.rect.width;
        int height = (int)sprite.rect.height;

        // 创建新的 Texture2D
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // 获取原始像素数据
        Color[] originalPixels = sprite.texture.GetPixels(
            (int)sprite.rect.x,
            (int)sprite.rect.y,
            width,
            height
        );

        // 翻转 X 轴：创建新的像素数组
        Color[] flippedPixels = new Color[originalPixels.Length];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int originalIndex = y * width + x;              // 原始索引
                int flippedIndex = y * width + (width - 1 - x); // 翻转后的索引
                flippedPixels[originalIndex] = originalPixels[flippedIndex]; // 交换像素
            }
        }

        // 设置新 Texture
        texture.SetPixels(flippedPixels);
        texture.Apply();

        return texture;
    }

}
