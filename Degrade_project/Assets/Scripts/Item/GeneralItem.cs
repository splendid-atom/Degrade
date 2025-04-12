using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 通用item
[CreateAssetMenu(fileName = "New GeneralItem", menuName = "Items/GeneralItem")]
public class GeneralItem : Item
{
    // 重写 Use() 方法，实现具体的使用逻辑
    public override void Use(AudioSource audioSource)
    {
        Debug.Log($"Using item: {itemID}");

        if (itemID == 5) { // 时光手表
            if (audioSource != null) audioSource.Play();

            if (QuestUIManager.QuestManager != null && !QuestUIManager.QuestManager.isTaskCompleted("真要进去吗?", 2)) {
                if (BambooMazeHint.instance != null) BambooMazeHint.instance.isHintOn = true;
            }
            else {
                if (VillageSceneController.instance != null) {
                    if (!VillageSceneController.instance.wasTimeMachine) {
                        VillageSceneController.instance.isTimeMachine = true;
                    }
                    if (!VillageSceneController.instance.wasSecondTimeMachine &&
                        VillageSceneController.instance.wasTimeMachine) {
                        VillageSceneController.instance.isSecondTimeMachine = true;
                    }
                }
            }

            if (InterSceneMemory.instance != null && InterSceneMemory.instance.isInFactory1()) {
                Debug.Log("TimeMachine");
                Time.timeScale = 0.5f;
                InterSceneMemory.instance.isTimeSlowed = true;
                // 启动协程来恢复时间
                if (InterSceneMemory.instance != null) {
                    InterSceneMemory.instance.StartCoroutine(ResetTimeScaleAfterDelay(10f));
                }
            }
        }

        if (itemID == 4) { // 时空碎片
            Debug.Log("TimePiece");

            if (FetchItemController.instance != null && FetchItemController.instance.UsePortalHint != null) {
                FetchItemController.instance.UsePortalHint.gameObject.SetActive(false);
            }

            if (InterSceneMemory.instance != null)
                InterSceneMemory.instance.isSwitchToFactory1 = true;
        }

        // 播放使用音效
        if (useSound != null && audioSource != null) {
            audioSource.PlayOneShot(useSound);
        }
    }

    // 恢复时间缩放的协程
    private IEnumerator ResetTimeScaleAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay); // 使用 WaitForSecondsRealtime 确保即使在暂停状态下也能等待
        Time.timeScale = 1.0f;
    }
}
