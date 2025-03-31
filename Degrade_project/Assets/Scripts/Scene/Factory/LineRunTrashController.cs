using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRunTrashController : MonoBehaviour
{
    [SerializeField] private List<BeltTriggerMovement> activeBelts = new List<BeltTriggerMovement>(); // 存储当前影响垃圾的传送带
    public float AddTrashSpeed = 0f;
    public float HitDamage = 0f;
    public Rigidbody2D rigidbody;
    public BoxCollider2D boxcollider2D;
    public WoodenCableAnimation WoodenCableAnimation=null;

    public void DisableCollider()
    {
        boxcollider2D.enabled = false;
        rigidbody.isKinematic = true;
        // Halve the scale
        Vector3 currentScale = transform.localScale;
        transform.localScale = currentScale / 2f; // More efficient than setting x, y, z individually
        // Debug.Log($"{gameObject.name}: Scale changed from {currentScale} to {transform.localScale}", this);
    }

    public void AddBelt(BeltTriggerMovement belt)
    {
        if (!activeBelts.Contains(belt))
        {
            activeBelts.Add(belt);
        }
    }

    public void RemoveBelt(BeltTriggerMovement belt)
    {
        if (activeBelts.Contains(belt))
        {
            activeBelts.Remove(belt);
        }
    }

    public BeltTriggerMovement GetActiveBelt()
    {
        return activeBelts.Count > 0 ? activeBelts[0] : null; // 返回最先加入的传送带
    }
    public void HitPlayer(){
        PlayerController.Instance.HitPlayerDamage(HitDamage);
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // 确保玩家物体有 "Player" 标签
        {
            HitPlayer();
        }
    }
    
}
