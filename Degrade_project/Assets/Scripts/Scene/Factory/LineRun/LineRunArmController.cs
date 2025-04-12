using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRunArmController : MonoBehaviour
{
    public Animator animator;
    private bool isPlayerInside = false;
    private bool isPlayerCatched = false;
    public LineRunArmAnimation LineRunArmAnimation;
    public Transform ArmCatcherTransform;
    private Transform playerTransform;
    public Transform EnvironmentTransform;
    public float duration = 3f;
    public bool isPlayerCatchedBool(){
        return isPlayerCatched;
    }
    void Update()
    {
        // Debug.Log("isPlayerInside:"+isPlayerInside);
        // Debug.Log("isPlayerCatched:"+isPlayerCatched);
        // Debug.Log("isPlayerReleased:"+LineRunArmAnimation.GetIsReleased());
        if(!isPlayerCatched && isPlayerInside && LineRunArmAnimation.GetIsCatching())
        {
            // Debug.Log("Player Catched!");
            isPlayerCatched = true;
        }

        // Call the function to fix player position if caught
        if(isPlayerCatched && playerTransform != null)
        {
            FixPlayerToCatcher();
        }
        if(isPlayerCatched && LineRunArmAnimation.GetIsReleased()){
            isPlayerCatched = false;
            StartCoroutine(PlayerFallingToDie());

        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerInside = true;
            animator.SetBool("isCatched", true);
            if(playerTransform == null)
            {
                playerTransform = other.transform;
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            isPlayerInside = false;
        }
    }

    // New function to handle fixing player transform
    private void FixPlayerToCatcher()
    {
        playerTransform.SetParent(ArmCatcherTransform);
        playerTransform.localPosition = Vector3.zero; // Adjust offset as needed
        // Optional: Add additional transform constraints here if needed
    }
    // New coroutine for player falling effect
    IEnumerator PlayerFallingToDie()
    {
        // Debug.Log("PlayerFallingToDie");
        if(playerTransform == null) yield break;

        // Step 1: Detach from catcher and reparent to environment
        playerTransform.SetParent(EnvironmentTransform);
        
        Vector3 originalPosition = playerTransform.position;
        Vector3 targetPosition = new Vector3(originalPosition.x, originalPosition.y, originalPosition.z + 30f); // Fall along Z-axis

        // Fall animation (rise along Z-axis, similar to FallingFloors)
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            playerTransform.position = Vector3.Lerp(originalPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        playerTransform.position = targetPosition; 

        isPlayerCatched = false;
        playerTransform = null; 
        isPlayerInside = false;
        LineRunArmAnimation.isNotReleasedPlayer();
        LineRunArmAnimation.ResetLineRunArm();
        PlayerController.Instance.InstantDie();

    }
}