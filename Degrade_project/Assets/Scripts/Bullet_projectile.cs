using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_projectile : MonoBehaviour
{
    private float angle;  // 子弹的旋转角度

    // 设置子弹的旋转
    public void SetDirection(float newAngle)
    {
        angle = newAngle;

        // 更新子弹的朝向
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // 让子弹沿着给定的方向飞行
    void Update()
    {
        // 在这里根据方向控制子弹的移动，例如:
        transform.Translate(Vector3.up * Time.deltaTime * 10f); // 子弹沿朝向飞行
    }
}
