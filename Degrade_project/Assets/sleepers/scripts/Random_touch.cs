using System;
using UnityEngine;

public class Random_touch : MonoBehaviour {
    System.Random rand;

	void Start () {

        for (int i = 0; i < transform.childCount; i++)
        {
            rand = new System.Random(transform.GetChild(i).GetInstanceID());
            float ternAmount = (float)rand.Next(500)/100;
            transform.GetChild(i).transform.Rotate(new Vector3(0, 1, 0) * ternAmount);
            ternAmount = (float)rand.Next(500)/100;
            transform.GetChild(i).transform.Rotate(new Vector3(1, 0, 0) * ternAmount);
            ternAmount = (float)rand.Next(500)/100;
            transform.GetChild(i).transform.Rotate(new Vector3(0, 0, 1) * ternAmount);

        }
    }	
}