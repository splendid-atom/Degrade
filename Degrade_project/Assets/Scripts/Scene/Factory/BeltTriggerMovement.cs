using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeltTriggerMovement : MonoBehaviour
{
    public ConveyorBelt conveyorBelt; // 引用传送带
    public float moveSpeed = 2.0f; // 物品的移动速度
    public Transform moveDirectionObject; // MoveDirection 物体
    public float stopDistance = 0.5f; // 当物品距离 MoveDirection 物体小于该距离时停止移动
    private void Start()
    {
        if (conveyorBelt == null)
        {
            conveyorBelt = GetComponentInParent<ConveyorBelt>(); // 自动获取 ConveyorBelt
            // 同步 moveSpeed 到 ConveyorBeltScript.speed
            conveyorBelt.speed = moveSpeed;
        }

        if (moveDirectionObject == null)
        {
            Debug.LogError("请在 Inspector 中指定 MoveDirection 物体！");
        }
    }

    private void Update()
    {
        // 确保每帧同步 moveSpeed
        if (conveyorBelt != null)
        {
            conveyorBelt.speed = moveSpeed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LineRunController.Instance.CheckAndSetCamera();
            other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, -1.3f);
            LineRunController.Instance.AddBelt(this);
        }
        else{
            if (other.GetComponent<SpriteRenderer>() == null){
                other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, -1.3f);
            }
            other.gameObject.GetComponent<LineRunTrashController>()?.AddBelt(this);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LineRunController.Instance.RemoveBelt(this);
            if (LineRunController.Instance.GetActiveBelt() == null)
            {
                LineRunController.Instance.ResetCamera();
                other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, -0.7f);
                RotatingCamera.Instance.EnableRotation();
            }
        }
        else{
            other.gameObject.GetComponent<LineRunTrashController>()?.RemoveBelt(this);
            if (other.gameObject.GetComponent<LineRunTrashController>()?.GetActiveBelt() == null)
            {
                other.transform.position = new Vector3(other.transform.position.x, other.transform.position.y, -0.7f);

            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        // if (other.GetComponent<SpriteRenderer>() == null) return;
        if (moveDirectionObject == null) return; 

        // 计算移动方向
        Vector3 moveDirection = (moveDirectionObject.position - other.transform.position).normalized;
        float distanceToMoveDirection = Vector3.Distance(other.transform.position, moveDirectionObject.position);

        // 如果距离小于停止距离，则不再移动
        if (distanceToMoveDirection < stopDistance) return;

        // 只有最先加入的传送带影响玩家
        if (other.CompareTag("Player"))
        {
            if (LineRunController.Instance.GetActiveBelt() != this) return;
        }
        else
        {
            // 获取垃圾的 LineRunTrashController
            LineRunTrashController trashController = other.GetComponent<LineRunTrashController>();
            if (trashController == null) return;

            // 只有垃圾的最先加入的传送带影响它
            if (trashController.GetActiveBelt() != this) return;
            other.transform.position += moveDirection * (moveSpeed+trashController.AddTrashSpeed) * Time.deltaTime;
            return;
        }
        // 物品可以被所有传送带推动
        other.transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
