 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FacingCamera : MonoBehaviour
{
    Transform[] childs;
    
    void Start()
    {
        childs = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            childs[i] = transform.GetChild(i);
        }
    }

    // Update is called once per frame
    void Update()
    {
        RotateObjects();
    }   
    void RotateObjects(){
        for(int i = 0; i < childs.Length; i++){
            if(childs[i].gameObject.layer != LayerMask.NameToLayer("MinimapOnly")){
                childs[i].rotation = Camera.main.transform.rotation;
            }
            
        }
    }
}
