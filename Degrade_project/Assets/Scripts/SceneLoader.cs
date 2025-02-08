using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.iOS;

public class NewBehaviourScript : MonoBehaviour
{
    public GameObject eventObj;
    public Button btn1;
    public Button btn2;
    public Button btn3;
    public Animator animator;
    public Canvas targetCanvas;
    // Start is called before the first frame update
    void Start()
    {
        Button[] allButtons = targetCanvas.GetComponentsInChildren<Button>(true);
        foreach (Button button in allButtons)
        {
            // 隐藏按钮
            button.gameObject.SetActive(false);
        }
        // GameObject.DontDestroyOnLoad(this.gameObject);
        GameObject.DontDestroyOnLoad(this.eventObj);

        btn1.onClick.AddListener(LoadScene1);
        btn2.onClick.AddListener(LoadScene2);
        btn3.onClick.AddListener(LoadScene3);

    }

    private void LoadScene1()
    {
        StartCoroutine(LoadScene(0));
    }

    private void LoadScene2()
    {
        StartCoroutine(LoadScene(1));
    }    
    private void LoadScene3()
    {
        StartCoroutine(LoadScene(2));
    } 

    IEnumerator LoadScene(int index)
    {
        animator.SetBool("fadein",true);
        animator.SetBool("fadeout",false);

        yield return new WaitForSeconds(1);

        AsyncOperation async = SceneManager.LoadSceneAsync(index);
        async.completed += OnLoadedScene;
    }

    private void OnLoadedScene(AsyncOperation obj)
    {
        animator.SetBool("fadein",false);
        animator.SetBool("fadeout",true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
