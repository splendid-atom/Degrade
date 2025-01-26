using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet_projectile : MonoBehaviour
{
    [SerializeField] private float speed;

    private Vector2 direction;


    public void OnBecameInvisible()
    {
        Destroy(gameObject);
    }
    public void SetDirection(Vector2 direction)
    {
        this.direction = direction;
    }
        

    // Start is called before the first frame update
    // void Start()
    // {
        
    // }

    // Update is called once per frame
    void Update()
    {
      transform.position = direction * speed * Time.deltaTime + (Vector2)transform.position;
    }
}

/*

2个引用

0个引用

0个引用
private void Update()
transform.position = direction * speed *Time.deltaTime +（Vector2)transform.position;*/