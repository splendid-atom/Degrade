using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;  

public class BambooMazeHint : MonoBehaviour
{
    public static BambooMazeHint instance;
    [SerializeField] private Tilemap MazeHint1; 
    [SerializeField] private Tilemap MazeHint2; 
    public bool isHintOn = false;
    private bool currentHint = false; 
    private bool isSwitching = false;
    public float duration = 0.2f;
    
    // 存储正在运行的透明度调整协程
    private Dictionary<Tilemap, Coroutine> alphaCoroutines = new Dictionary<Tilemap, Coroutine>();

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        if (MazeHint1 == null)
            Debug.LogError("MazeHint1 not found or missing Tilemap component!");
        if (MazeHint2 == null)
            Debug.LogError("MazeHint2 not found or missing Tilemap component!");

        if (MazeHint1 != null) SetTilemapAlphaImmediate(MazeHint1, 0);
        if (MazeHint2 != null) SetTilemapAlphaImmediate(MazeHint2, 0);
    }

    void Update()
    {
        //更新当前移动竹林的状态，第一种or第二种
        currentHint = BambooGroupMovement.instance.isInitial;
        if (isHintOn)
        {       
            switchHint();
        }
        else if (MazeHint1 != null && MazeHint2 != null) 
        {
            StartTilemapAlphaCoroutine(MazeHint1, 0);
            StartTilemapAlphaCoroutine(MazeHint2, 0);
        }
    }

    private void switchHint()
    {
        if (!isSwitching)
            StartCoroutine(HintOn());
    }

    IEnumerator HintOn()
    {
        isSwitching = true;

        if (currentHint)
        {
            StartTilemapAlphaCoroutine(MazeHint2, 1);
            StartTilemapAlphaCoroutine(MazeHint1, 0);
        }
        else
        {
            StartTilemapAlphaCoroutine(MazeHint1, 1);
            StartTilemapAlphaCoroutine(MazeHint2, 0);
        }
        
        // currentHint = !currentHint;
        yield return new WaitForSeconds(5.0f);
        isHintOn = false;
        isSwitching = false;
    }

    // 立即设置透明度
    private void SetTilemapAlphaImmediate(Tilemap tilemap, float alpha)
    {
        if (tilemap == null) return;
        Color color = tilemap.color;
        color.a = alpha; 
        tilemap.color = color;
    }
    
    // 启动或替换透明度协程
    private void StartTilemapAlphaCoroutine(Tilemap tilemap, float targetAlpha)
    {
        if (tilemap == null) return;

        // 如果该Tilemap已有运行中的协程，先停止它
        if (alphaCoroutines.ContainsKey(tilemap) && alphaCoroutines[tilemap] != null)
        {
            StopCoroutine(alphaCoroutines[tilemap]);
        }

        // 启动新的协程，并存入字典
        Coroutine newCoroutine = StartCoroutine(SetTilemapAlpha(tilemap, targetAlpha));
        alphaCoroutines[tilemap] = newCoroutine;
    }

    // 平滑调整透明度的协程
    private IEnumerator SetTilemapAlpha(Tilemap tilemap, float targetAlpha)
    {
        if (tilemap == null) yield break;

        Color currentColor = tilemap.color;
        float startAlpha = currentColor.a;
        float timeElapsed = 0;
        float updateInterval = 0.033f; 

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / duration);
            currentColor.a = alpha;
            tilemap.color = currentColor;

            yield return new WaitForSeconds(updateInterval);
        }

        // 确保最终设置为目标透明度
        currentColor.a = targetAlpha;
        tilemap.color = currentColor;
    }
}
