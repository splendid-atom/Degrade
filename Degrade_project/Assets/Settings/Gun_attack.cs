using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_attack : MonoBehaviour
{
    [SerializeField] private GameObject projectile;  // 子弹预设
    [SerializeField] private Transform muzzle;       // 枪口位置
    [SerializeField] private Transform playerCharacter; // 玩家角色位置

    private float angle;  // 用来存储武器旋转角度

    public float Offset_x = 0f;  // 屏幕中心的X轴偏移量
    public float Offset_y = 0f;  // 屏幕中心的Y轴偏移量

    // Update is called once per frame
    void Update()
    {
        if(QuestUIManager.QuestManager.quests[0].isCompleted && 
            !SwitchBridgeCamera.instance.isBridgeCameraSwitched){
                return;
        }
        // 获取鼠标的位置
        Vector3 mouseScreenPosition = Input.mousePosition;

        // 获取屏幕的中心坐标，并加上修正量
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 10f);
        screenCenter.x += Offset_x;  // X轴修正
        screenCenter.y += Offset_y;  // Y轴修正

        // 计算鼠标相对于修正后屏幕中心的方向向量
        Vector3 directionToMouse = new Vector3(mouseScreenPosition.x, mouseScreenPosition.y, 10f) - screenCenter;

        // 将方向向量归一化，确保射击方向不受鼠标距离影响
        directionToMouse.Normalize();

        // 计算角度（相对于修正后的屏幕中心，忽略z轴）
        angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;

        // 获取当前相机的旋转角度（用于考虑镜头转动的影响）
        float cameraRotationValueForGun = Camera.main.transform.rotation.eulerAngles.z;

        // 更新武器的旋转角度，使其朝向鼠标方向，考虑相机的旋转
        transform.rotation = Quaternion.Euler(0, 0, angle + cameraRotationValueForGun);

        // 检测空格键按下
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 实例化子弹并设置方向
            GameObject temp = Instantiate(projectile, muzzle.position, Quaternion.identity);

            // 将当前武器的旋转角度传给子弹
            temp.GetComponent<Bullet_projectile>().SetDirection(angle + cameraRotationValueForGun - 90);
        }
    }
}
