//EnemyBaseBehaviour.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBaseBehaviour : MonoBehaviour
{

    public float moveSpeed = 2.0f;
    public Transform target;
    public float rangeDistance;
    public SpriteRenderer SR;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        TurnDirection();
    }

    protected virtual void Move()
    {
        if (Vector2.Distance(transform.position, target.position) < rangeDistance)
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, moveSpeed * Time.deltaTime);
            
        }
    }

    // protected void TurnDirection()
    // {
    //     if (SR == null)
    //     {
    //         Debug.LogWarning("SpriteRenderer SR is null!");
    //         return;
    //     }
        
    //     if (transform.position.x > target.position.x)
    //     {
    //         SR.flipX = true;
    //         Debug.Log("Setting flipX to true");
    //     }
    //     else
    //     {
    //         SR.flipX = false;
    //         Debug.Log("Setting flipX to false");
    //     }
    // }

    protected bool isFacingRight = true;

    protected void TurnDirection()
    {
        if (target == null)
            return;

        Vector3 scale = transform.localScale;
        if (transform.position.x > target.position.x)
        {
            scale.x = -Mathf.Abs(scale.x);
            isFacingRight = false;
        }
        else
        {
            scale.x = Mathf.Abs(scale.x);
            isFacingRight = true;
        }
        transform.localScale = scale;
    }







}

