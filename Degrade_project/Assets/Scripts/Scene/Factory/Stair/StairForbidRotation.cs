using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StairForbidRotation : MonoBehaviour
{
    public static StairForbidRotation Instance;
    public bool isPlayerInsideTrigger = false;
    private void Awake()
    {
        Instance = this;
    }
    public bool IsPlayerInside()
    {
        return isPlayerInsideTrigger;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = true;
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = true;
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInsideTrigger = false;
        }
    }
}
