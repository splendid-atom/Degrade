using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GarbageTruckTrigger : MonoBehaviour
{
    public bool isPlayerInTrigger = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public bool IsPlayerInTrigger()
    {
        return isPlayerInTrigger;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            isPlayerInTrigger = true;
        }
    }
}
