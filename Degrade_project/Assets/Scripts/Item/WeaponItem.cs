using UnityEngine;

// 武器类，继承自Item
[CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Weapon")]
public class WeaponItem : Item
{
    public int attackPower;  // 武器攻击力

    public override void Use(AudioSource audioSource)
    {
        Debug.Log($"Using weapon: {itemName}, Power: {attackPower}");
        // 播放使用音效
        if (useSound != null)
        {
            audioSource.PlayOneShot(useSound);
        }
    }
}