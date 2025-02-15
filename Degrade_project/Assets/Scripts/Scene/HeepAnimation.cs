using UnityEngine;

public class HeepAnimation : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 根据玩家输入或其他条件控制动画
        if (Input.GetKey(KeyCode.Z))  // 玩家按下 W 键时
        {
            animator.SetBool("IsForming", true);
        }
    }
    // 这个函数将会在动画事件触发时被调用
    public void OnAnimationComplete()
    {
        // 设置 IsComplete 为 true
        animator.SetBool("IsComplete",true); 
    }
}
