using System.Collections;
using UnityEngine;

public class PortalAnimation : MonoBehaviour
{
    private Animator animator;
    private AudioSource firstAudioSource;  // 第一个 AudioSource
    private AudioSource secondAudioSource; // 第二个 AudioSource
    public float fadeDuration = 0.05f;  // 渐变持续时间
    
    void Awake()
    {
        // 获取两个 AudioSource 组件
        firstAudioSource = GetComponents<AudioSource>()[0];
        secondAudioSource = GetComponents<AudioSource>()[1];

        // 设置第二个 AudioSource 为循环
        secondAudioSource.loop = true;
    }

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // 检查第一个音频是否播放完毕
        if (!firstAudioSource.isPlaying && !secondAudioSource.isPlaying)
        {
            // 当第一个音频播放完，开始渐变淡入第二个音频
            StartCoroutine(FadeIn(secondAudioSource));
        }
    }

    // 这个函数将会在动画事件触发时被调用
    public void OnAnimationComplete()
    {
        // 设置 IsComplete 为 true
        animator.SetBool("IsOpening", false);
        animator.SetBool("IsLooping", true);
    }

    private IEnumerator FadeIn(AudioSource fadeInSource)
    {
        // 获取设置的音量作为目标音量
        float targetVolume = fadeInSource.volume;

        float startVolume = 0f;
        fadeInSource.volume = startVolume;  // 初始音量为 0

        fadeInSource.Play();  // 播放第二个音频

        while (fadeInSource.volume < targetVolume)
        {
            fadeInSource.volume += Time.deltaTime / fadeDuration;  // 逐渐增加音量
            yield return null;
        }

        fadeInSource.volume = targetVolume;  // 保证最终音量为目标值
    }
}
