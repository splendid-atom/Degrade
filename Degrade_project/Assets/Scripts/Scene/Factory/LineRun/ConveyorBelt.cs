using UnityEngine;

public class ConveyorBelt : MonoBehaviour
{
    public float speed = 1.0f;  // 传送带滚动速度
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offset = Time.time * speed;
        rend.material.mainTextureOffset = new Vector2(offset, 0); // 仅在X轴滚动
    }
}
