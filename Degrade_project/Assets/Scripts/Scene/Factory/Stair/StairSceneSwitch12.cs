using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairSceneSwitch12 : MonoBehaviour
{
    public bool isPlayerInSceneSwitch = false;
    public bool getPlayerInSceneSwitch12()
    {
        return isPlayerInSceneSwitch;
    }
    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player"))
        {
            isPlayerInSceneSwitch = true;
            Debug.Log("玩家进入了 SceneSwitch12 的触发器！");
        }
    }
}
