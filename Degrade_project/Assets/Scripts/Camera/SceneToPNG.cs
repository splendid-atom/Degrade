using UnityEngine;
using System.IO;

public class SceneToPNG : MonoBehaviour
{
    public Camera captureCamera; // 用于捕捉场景的摄像机
    public RenderTexture renderTexture; // 用来捕捉图像的 RenderTexture

    void Start()
    {
        CaptureScene();
    }

    void CaptureScene()
    {
        // 获取 RenderTexture 的宽度和高度
        int width = renderTexture.width;
        int height = renderTexture.height;

        // 创建一个 Texture2D 用来保存 RenderTexture 的内容
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // 让摄像机渲染到 RenderTexture
        captureCamera.targetTexture = renderTexture;
        captureCamera.Render();

        // 从 RenderTexture 获取图像数据到 Texture2D
        RenderTexture.active = renderTexture;
        texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        texture.Apply();

        // 保存为 PNG 文件
        byte[] bytes = texture.EncodeToPNG();
        File.WriteAllBytes("SceneMap.png", bytes);
        
        // 清理
        captureCamera.targetTexture = null;
        RenderTexture.active = null;

        Debug.Log("Scene captured and saved as SceneMap.png");
    }
}
