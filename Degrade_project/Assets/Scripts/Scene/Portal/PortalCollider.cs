using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalCollider : MonoBehaviour
{
    // 当玩家靠近时触发
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.name == "riverPortalCollider")
            {
                RiverPortalAnimation.instance.isPlayerApproach = true;
            }
            else
            {
                PortalAnimation.instance.isPlayerApproach = true;
            }
        }
    }

    // 当玩家离开时触发
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.name == "riverPortalCollider")
            {
                RiverPortalAnimation.instance.isPlayerApproach = false;
            }
            else
            {
                PortalAnimation.instance.isPlayerApproach = false;
            }
        }
    }
}
