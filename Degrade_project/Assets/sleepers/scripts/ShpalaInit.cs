using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShpalaInit : MonoBehaviour
{

    public bool Generatator = false;
    public GameObject ShpalasToInit;
    public int count;
    public float Dist;
    Quaternion direction;

    void Start() {

        direction = transform.rotation;

        if (Generatator)
        {
            for (int i = 0; i < count; )
            {
                Vector3 trans = new Vector3(
                        transform.position.x + Dist * ++i,
                        transform.position.y,
                        transform.position.z
                    );
                Vector3 transOposite = new Vector3(
                        transform.position.x - Dist * i,
                        transform.position.y,
                        transform.position.z
                    );
                Instantiate(ShpalasToInit, trans, direction);
                Instantiate(ShpalasToInit, transOposite, direction);
            }
        }
    }

}
