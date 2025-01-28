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
    Vector2 moveDirection = new Vector2(1, 0);
    public Transform arrowIndicator;  // 引用ArrowIndicator

    // Start is called before the first frame update
    void Start()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        MoveActionWASD.Enable();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        move = MoveActionWASD.ReadValue<Vector2>();
        if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
        {
            moveDirection.Set(move.x, move.y);
            //sets its length to 1 but keeps its direction the same. 
            moveDirection.Normalize();
        }

        animator.SetFloat("Look X", moveDirection.x);
        animator.SetFloat("Look Y", moveDirection.y);
        animator.SetFloat("Speed", move.magnitude);

        // 确保ArrowIndicator的旋转与角色的移动方向一致
        if (arrowIndicator != null)
        {
            // 计算箭头的旋转角度
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            arrowIndicator.rotation = Quaternion.Euler(new Vector3(0, 0, angle-90));
        }
    }

    void FixedUpdate()
    {
        // 使用角色自身的坐标系移动
        move = move.x * transform.right + move.y * transform.up;
        Vector2 position = (Vector2)rigidbody2d.position + move * speed * Time.deltaTime;
        rigidbody2d.MovePosition(position);
    }
}
