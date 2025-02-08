using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteractionDetector : MonoBehaviour
{
    public GameObject interactionButton; // 交互按钮
    public float interactionDistance = 2f; // 交互距离

    private GameObject player; // 角色对象

    private void Initialize()
    {
        player = GameObject.FindWithTag("Player");
        // 初始时隐藏交互按钮
        interactionButton.SetActive(false);
    }
    private void Start()
    {
        Initialize();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Initialize();
    }

    private void Update()
    {
        if (player != null)
        {
            // 计算角色与可交互物体之间的距离
            float distance = Vector3.Distance(player.transform.position, transform.position);
            if (distance <= interactionDistance)
            {
                // 角色靠近，显示交互按钮
                interactionButton.SetActive(true);
            }
            else
            {
                // 角色远离，隐藏交互按钮
                interactionButton.SetActive(false);
            }
        }
    }
}