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
    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        MoveActionWASD.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveActionWASD.ReadValue<Vector2>();
    }
    void FixedUpdate()
    {
        Vector2 position = (Vector2)rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }
}
