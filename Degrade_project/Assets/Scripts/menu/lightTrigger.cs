using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lightTrigger : MonoBehaviour
{
    private BoxCollider2D triggerCollider; // 触发器的BoxCollider2D
    private GameObject player; // 玩家对象
    private bool lightControl = false;
    public GameObject LightContent;

    void Start()
    {
        player = GameObject.Find("PlayerCharacter");
        triggerCollider = gameObject.GetComponent<BoxCollider2D>();
    }
    void Update()
    {
        if (lightControl == true)
        {
            if (Input.GetKeyDown(KeyCode.G))
            {
                lightManager.lightmanager.lightLamp(gameObject.transform.parent);
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 判断触发器内是否是玩家
        if (other.gameObject == player && doorTrigger.doortrigger.isTrickRaised)
        {
            lightControl = true;
            LightContent.SetActive(true);

        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // 判断触发器内是否是玩家
        if (other.gameObject == player)
        {
            lightControl = false;
            LightContent.SetActive(false);
        }
    }
}
