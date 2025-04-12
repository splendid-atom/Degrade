using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRunDisableCollider : MonoBehaviour
{
    public GameObject LineRunContainer;
    void Start()
    {
        if(LineRunContainer==null){
            LineRunContainer = GameObject.Find("LineRunContainer");
        }
    }
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            LineRunContainer.SetActive(false);
        }
    }
}
