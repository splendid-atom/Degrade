using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerController : MonoBehaviour
{


    Rigidbody2D rigidbody2d;
    public float speed = 3.0f;
    public InputAction MoveActionWASD;
    Vector2 move;
    Animator animator;
    Vector2 moveDirection = new Vector2(1,0);
    // private Vector3 offset;

    // Start is called before the first frame update
    void Start()
    {
        // offset = Camera.main.transform.position - transform.position;
        rigidbody2d = GetComponent<Rigidbody2D>();
        MoveActionWASD.Enable();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveActionWASD.ReadValue<Vector2>();
        if(!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y,0.0f))
        {
            moveDirection.Set(move.x, move.y);
            //sets its length to 1 but keeps its direction the same. 
            moveDirection.Normalize();

        }
        animator.SetFloat("Look X", moveDirection.x);
        animator.SetFloat("Look Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);
        // Camera.main.transform.position = transform.position + offset; 
    }
    void FixedUpdate()
    {
        //使用角色自身的坐标系移动
        move = move.x*transform.right + move.y*transform.up;
        Vector2 position = (Vector2)rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }
}
