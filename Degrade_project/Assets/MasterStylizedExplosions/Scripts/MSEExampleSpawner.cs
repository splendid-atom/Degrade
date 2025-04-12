using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MSEExampleSpawner : MonoBehaviour
{
    public ParticlesObj particles;
    int index = 0;
    public GameObject CurParticle;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            index -= 1;
            if(index < 0)
            {
                index = particles.Particles.Count - 1;
            }
            Spawn();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            index += 1;
            if(index >= particles.Particles.Count)
            {
                index = 0;
            }
            Spawn();
        }
        if (index > particles.Particles.Count - 1)
        {
            index = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            Spawn();
        }
    }
    private void Start()
    {
        Spawn();
    }
    public void Spawn()
    {
        if (CurParticle != null)
        {
            Destroy(CurParticle);
        }
        CurParticle = Instantiate(particles.Particles[index]);
    }
}
