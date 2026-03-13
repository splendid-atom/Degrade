using UnityEngine;
using System.Collections;

public class csParticleMove : MonoBehaviour
{
    public float speed = 5f;
    private Vector3 moveDirection;

    void Start()
    {
        // Default to forward if no direction is set
        if (moveDirection == Vector3.zero)
        {
            moveDirection = transform.forward;
        }
    }

    void Update()
    {
        transform.Translate(moveDirection * speed * Time.deltaTime, Space.World);
    }

    void OecameInvisible()
    {
        Destroy(gameObject);
    }

    // This method can be called by the emitter to set the direction
    public void SetDirection(Vector3 direction)
    {
        moveDirection = direction.normalized;
    }
}