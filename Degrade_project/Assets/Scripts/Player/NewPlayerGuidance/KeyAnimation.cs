using UnityEngine;

public class KeyAnimation : MonoBehaviour
{
    private SpriteRenderer keySprite;  // 用于显示键盘按键的SpriteRenderer
    public Vector3 pressedScale = new Vector3(0.9f, 0.9f, 1);  // 按下时的缩放
    public Vector3 normalScale = new Vector3(1f, 1f, 1f);  // 正常时的缩放
    public Color pressedColor = new Color(0.8f, 0.8f, 0.8f);  // 按下时的颜色
    public Color normalColor = Color.white;  // 正常时的颜色

    public float animationSpeed = 1.5f;  // 动画的速度

    private float lerpTime = 0f; // 用于平滑过渡的时间

    void Start()
    {
        // 获取当前对象的SpriteRenderer
        keySprite = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (keySprite == null)
        {
            Debug.LogWarning("SpriteRenderer not found on this GameObject.");
            return;  // 如果没有SpriteRenderer，停止执行
        }

        // 让动画持续变化，模拟键盘按下和恢复的效果
        lerpTime += Time.deltaTime * animationSpeed;

        // 保证lerpTime在0到1之间
        if (lerpTime > 1f)
        {
            lerpTime = 0f;  // 重置动画时间
        }

        // 使用PingPong函数来实现按下和恢复的动画循环
        float scaleLerp = Mathf.PingPong(lerpTime, 1f);

        // 按键的缩放和颜色平滑过渡
        keySprite.transform.localScale = Vector3.Lerp(normalScale, pressedScale, scaleLerp);
        keySprite.color = Color.Lerp(normalColor, pressedColor, scaleLerp);
    }
}
