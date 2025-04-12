using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityFactorySceneHDRP;
public class TrashDiableBelt : MonoBehaviour
{
    public bool isPortalOn = true;
    public void DisablePortalCollider()
    {
        if (isPortalOn)
        {
            isPortalOn = false;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(isPortalOn){
            other.gameObject.GetComponent<CustomSplineAnimate>().isOnBelt = false;            
        }

    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(isPortalOn){
            other.gameObject.GetComponent<CustomSplineAnimateTrashEnemies>().isOnBelt = false;
            Destroy(other.gameObject);            
        }

    }
}
