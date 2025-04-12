using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRunGarbageCart : MonoBehaviour
{
    public List<GameObject> TrashInsideCart;
    public int MaxTrashInCart = 9;
    public BoxCollider2D TrashContainerPos;
    public float offset_z = -1.0f;
    void Update()
    {
        if (TrashInsideCart.Count > MaxTrashInCart)
        {
            GameObject trash = TrashInsideCart[0];
            TrashInsideCart.RemoveAt(0);
            Destroy(trash);

        }
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")){
            if(LineRunController.Instance.GetActiveBelt() != null){
                PlayerController.Instance.InstantDie();  
            }
            
        }
        if (!other.CompareTag("Player"))
        {
            LineRunTrashController trashController = other.GetComponent<LineRunTrashController>();
            if (trashController)
            {
                if(trashController.WoodenCableAnimation!=null){
                    trashController.WoodenCableAnimation.ResetRotation();
                }
                // Add trash to cart
                GameObject trash = other.gameObject;
                TrashInsideCart.Add(trash);
                // Debug.Log($"{gameObject.name}: Added {trash.name} to cart. Count: {TrashInsideCart.Count}", this);

                // Remove belt influence and disable collider
                trashController.RemoveBelt(other.GetComponent<BeltTriggerMovement>());
                trashController.DisableCollider();

                // Randomly position trash within TrashContainerPos
                PlaceTrashRandomly(trash);
            }
        }
    }
    void PlaceTrashRandomly(GameObject trash)
    {
        if (TrashContainerPos == null)
        {
            Debug.LogError("TrashContainerPos is not assigned on " + gameObject.name, this);
            return;
        }

        // Get the bounds of the TrashContainerPos collider
        Bounds bounds = TrashContainerPos.bounds;

        // Generate random position within bounds
        float randomX = Random.Range(bounds.min.x, bounds.max.x);
        float randomY = Random.Range(bounds.min.y, bounds.max.y);
        Vector3 randomPosition = new Vector3(randomX, randomY, trash.transform.position.z+offset_z); // Preserve Z

        // 生成随机旋转（仅围绕 Z 轴，适合 2D）
        float randomZRotation = Random.Range(0f, 360f); // 0 到 360 度的随机旋转
        Quaternion randomRotation = Quaternion.Euler(0f, 0f, randomZRotation);

        // Set trash position and make it a child of the cart (optional)
        trash.transform.position = randomPosition;
        trash.transform.rotation = randomRotation; // 设置世界旋转
        trash.transform.SetParent(transform, true); // Keeps local scale, optional

        // Debug.Log($"{gameObject.name}: Placed {trash.name} at {randomPosition}", this);
    }
}
