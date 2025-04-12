    using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minimalist.Quantity;
public class ShieldGeneratorController : MonoBehaviour
{
    public float Health = 100f;
    private ShieldGeneratorAnimation ShieldGeneratorAnimation;
    private Enemy3 Enemy3;
    //health display setting
    public QuantityBhv quantityBhv; 
    public int direction = 1;
    public Transform HealthDisplay;
    void Start()
    {
        Enemy3 = GetComponent<Enemy3>();
        ShieldGeneratorAnimation = GetComponent<ShieldGeneratorAnimation>();
    }
    void Update()
    {
        HealthDisplaySetting(Health);
        Health = Enemy3.currentHealth;
        if(Health <= 0){
            ShieldGeneratorAnimation.OnAnimationBroken();
        }
        if(Health<=0){
            gameObject.SetActive(false);
        }
    }
    //关于血量显示的设置
    private void HealthDisplaySetting(float currentHealth){
        direction = transform.localScale.x > 0 ? 1 : -1;
        // 确保 HealthDisplay 的 x 轴缩放方向与 direction 一致
        HealthDisplay.localScale = new Vector3(
            Mathf.Abs(HealthDisplay.localScale.x) * direction, // 让 x 方向匹配 direction
            HealthDisplay.localScale.y,
            HealthDisplay.localScale.z
        );
        if (quantityBhv != null)
        {
            quantityBhv.Amount = currentHealth;
        }
    }
}
