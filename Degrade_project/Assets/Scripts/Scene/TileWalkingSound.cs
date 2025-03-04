using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileWalkingSound : MonoBehaviour
{
    private Tilemap tilemap; // 引用Tilemap
    public List<TileBase> grassTiles; // 草地Tile的List
    public List<AudioClip> grassSounds; // 草地音效列表
    private AudioSource audioSource; // 播放音效的AudioSource

    private Vector3 lastPosition;

    // Start is called before the first frame update
    void Start()
    {
        // 确保tilemap和AudioSource已经被赋值
        if (tilemap == null)
        {
            tilemap = GameObject.Find("Tilemap").GetComponent<Tilemap>(); // 假设Tilemap对象名字为Tilemap
        }

        // 获取AudioSource组件，确保玩家对象上有AudioSource组件
        audioSource = GameObject.Find("PlayerCharacter").GetComponent<AudioSource>(); // 确保播放器对象是玩家
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 playerPosition = GameObject.Find("PlayerCharacter").transform.position;

        // 检查玩家位置是否发生变化
        if (playerPosition != lastPosition)
        {
            CheckTileForGrass(playerPosition); // 检查当前tile是否是草地
        }
        else
        {
            // 玩家没有移动时停止播放音效
            if (audioSource.isPlaying)
            {
                audioSource.Stop(); // 停止音效播放
            }
        }

        lastPosition = playerPosition; // 更新玩家位置
    }

    // 检查玩家所在的Tile是否是草地
    void CheckTileForGrass(Vector3 position)
    {
        Vector3Int tilePosition = tilemap.WorldToCell(position); // 将世界坐标转化为Tile坐标
        TileBase tile = tilemap.GetTile(tilePosition); // 获取该Tile的TileBase

        if (tile != null && grassTiles.Contains(tile)) // 如果该tile在草地列表中
        {
            // Debug.Log("Player is on grass!");
            PlayGrassSound(); // 播放草地音效
        }
    }

    // 播放草地音效
    void PlayGrassSound()
    {
        if (audioSource != null && grassSounds.Count > 0) // 确保音效列表非空
        {
            // 从音效列表中随机选择一个音效
            AudioClip randomClip = grassSounds[Random.Range(0, grassSounds.Count)];

            // 播放草地音效
            if (!audioSource.isPlaying) // 确保音效没有重复播放
            {
                audioSource.clip = randomClip;
                audioSource.Play();
            }
        }
    }
}
