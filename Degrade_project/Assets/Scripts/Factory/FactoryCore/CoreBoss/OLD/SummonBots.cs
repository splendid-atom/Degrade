using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonBots : MonoBehaviour
{
    [SerializeField] GameObject[] BotPrefabs;
    [SerializeField] public float spawnInterval = 0.5f;
    [SerializeField] Vector2 spawnRangeX = new Vector2(-3.1f, 3.2f);
    [SerializeField] Vector2 spawnRangeY = new Vector2(-3.1f, 3.2f);
    
    [SerializeField] Transform player;
    
    // 添加父对象引用
    [SerializeField] Transform botsParent;
    
    private Coroutine spawnCoroutine;
    
    void OnEnable() {
        // 只有在脚本被启用时才开始生成机器人
        player = GameObject.FindGameObjectWithTag("Player").transform;
        
        // 如果没有指定父对象，可以创建一个
        if (botsParent == null) {
            //find object named BotsContainer
            GameObject parentObj = GameObject.Find("Scientist");
            //GameObject parentObj = Object.Findob
            botsParent = parentObj.transform;
            Debug.Log("创建了机器人父对象: BotsContainer");
        }
        
        spawnCoroutine = StartCoroutine(SpawnBots());
        Debug.Log("机器人生成器已启用");
    }
    
    void OnDisable() {
        // 当脚本被禁用时停止生成
        if (spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        Debug.Log("机器人生成器已禁用");
    }

    IEnumerator SpawnBots() {
        while (true) {
            // 检查脚本是否仍处于启用状态
            if (!this.enabled) {
                yield break;
            }
            
            Vector2 pos = new Vector2(
                botsParent.position.x + Random.Range(spawnRangeX.x, spawnRangeX.y),
                botsParent.position.y + Random.Range(spawnRangeY.x, spawnRangeY.y)
            );
            int index = Random.Range(0, BotPrefabs.Length);
            GameObject bot = Instantiate(BotPrefabs[index], pos, Quaternion.identity);
            
            // 设置机器人的父对象
            if (botsParent != null) {
                bot.transform.SetParent(botsParent);
            }
            
            // 确保机器人有正确的标签
            bot.tag = "Bot";
            
            yield return new WaitForSeconds(spawnInterval);
        }
    }
}