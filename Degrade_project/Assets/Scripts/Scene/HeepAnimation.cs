using UnityEngine;

public class HeepAnimation : MonoBehaviour
{
    private Animator animator;

    public RuntimeAnimatorController firstAnimatorController; // 第一个 Animator Controller
    public RuntimeAnimatorController secondAnimatorController; // 第二个 Animator Controller

    private bool isUsingSecondAnimator = false; // 是否已经切换到第二个 Animator


    void Start()
    {
        animator = GetComponent<Animator>();

        // 初始时设置第一个 Animator Controller
        if (firstAnimatorController != null)
        {
            animator.runtimeAnimatorController = firstAnimatorController;
        }
    }

    void Update()
    {
        // 如果满足切换条件，切换到第二个 Animator Controller
        if (!isUsingSecondAnimator && VillageSceneController.instance.isTimeMachine) 
        {
            SwitchToSecondAnimator();

        }
        if(isUsingSecondAnimator&&VillageSceneController.instance.isTimeMachineMasked){
            animator.SetBool("IsForming", true);
        }

    }

    void SwitchToSecondAnimator()
    {
        // 切换到第二个 Animator Controller
        if (secondAnimatorController != null)
        {
            animator.runtimeAnimatorController = secondAnimatorController;
            isUsingSecondAnimator = true;  // 标记已经切换
        }
    }

    // 这个函数将会在动画事件触发时被调用
    public void OnAnimationComplete()
    {
        // 设置 IsComplete 为 true
        animator.SetBool("IsComplete", true);
    }
}
