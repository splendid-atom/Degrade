using System.Collections;
using UnityEngine;

public class TrapGenerator : MonoBehaviour {
    [SerializeField] GameObject[] trapPrefabs;
    [SerializeField] public float spawnInterval = 0.5f;
    [SerializeField] Vector2 spawnRangeX = new Vector2(-.1f, 0.2f);
    [SerializeField] Vector2 spawnRangeY = new Vector2(-.1f, 0.2f);
    
    [SerializeField] Transform player;
    [SerializeField] float trapLifetime = 3f;
    
    private Coroutine spawnCoroutine;
    
    void OnEnable() {
        // 只有在脚本被启用时才开始生成陷阱
        player = GameObject.FindGameObjectWithTag("Player").transform;
        spawnCoroutine = StartCoroutine(SpawnTraps());
        Debug.Log("陷阱生成器已启用");
    }
    
    void OnDisable() {
        // 当脚本被禁用时停止生成
        if (spawnCoroutine != null) {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }
        Debug.Log("陷阱生成器已禁用");
    }

    IEnumerator SpawnTraps() {
        while (true) {
            // 检查脚本是否仍处于启用状态
            if (!this.enabled) {
                yield break;
            }
            
            Vector3 pos = new Vector3(
                player.position.x + Random.Range(spawnRangeX.x, spawnRangeX.y),
                player.position.y + Random.Range(spawnRangeY.x, spawnRangeY.y),
                player.position.z + 2f
            );
            int index = Random.Range(0, trapPrefabs.Length);
            GameObject trap = Instantiate(trapPrefabs[index], pos, Quaternion.identity);
            
            // 确保陷阱有正确的标签
            trap.tag = "Trap";
            
            yield return new WaitForSeconds(spawnInterval);
            Destroy(trap, trapLifetime);
        }
    }
}