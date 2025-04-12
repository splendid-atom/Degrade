using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopArmBombController : MonoBehaviour
{
    public ParticleSystem explosion;
    public bool isExploded = false;
    public void Explosion()
    {
        if(isExploded){
            explosion.Play();
            isExploded = false;
        }
    }
    void Update()
    {
        Explosion();
    }
}
