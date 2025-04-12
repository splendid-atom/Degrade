using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopArmContainerCollider : MonoBehaviour
{
    public bool isPlayerInside = false;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool IsPlayerInside()
    {
        return isPlayerInside;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            isPlayerInside = true;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            isPlayerInside = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            isPlayerInside = false;
        }
    }
}
