using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;

public class end : MonoBehaviour
{
    [Header("界面设置")]
    public Canvas targetCanvas; // 要控制的Canvas
    public CanvasGroup canvasGroup; // Canvas的CanvasGroup组件
    public float fadeSpeed = 1.0f; // 渐变速度
    
    [Header("视频设置")]
    public VideoPlayer videoPlayer; // 视频播放器组件
    public RawImage videoDisplay; // 显示视频的RawImage
    public VideoClip endingVideo; // 结束视频
    
    [Header("音频设置")]
    public AudioSource backgroundMusic; // 要渐弱的背景音乐
    public float audioFadeOutDuration = 3.0f; // 音频渐弱持续时间（秒）
    public AudioSource endingAudio; // 结束音频（可选，在视频开始时播放）
    public AudioClip endingAudioClip; // 结束音频片段
    
    [Header("HUD控制")]
    public HUDControl hudControl; // HUD控制器引用
    
    [Header("初始设置")]
    public bool startHidden = true; // 初始是否隐藏
    public bool autoPlayVideo = true; // 显示画面时自动播放视频
    
    private bool isFading = false;
    private bool hasTriggered = false; // 防止重复触发
    private Coroutine audioFadeCoroutine; // 音频渐弱协程引用
    
    void Start()
    {
        // 如果没有指定CanvasGroup，尝试获取或添加
        if (canvasGroup == null && targetCanvas != null)
        {
            canvasGroup = targetCanvas.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = targetCanvas.gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // 初始状态设置
        if (canvasGroup != null && startHidden)
        {
            canvasGroup.alpha = 0f; // 开始时完全透明
            canvasGroup.interactable = false; // 不可交互
            canvasGroup.blocksRaycasts = false; // 不阻挡射线
        }
        
        // 如果没有指定VideoPlayer，尝试获取或添加
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
            if (videoPlayer == null && videoDisplay != null)
            {
                videoPlayer = videoDisplay.gameObject.AddComponent<VideoPlayer>();
            }
            else if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
        }
        
        // 设置视频
        if (endingVideo != null && videoPlayer != null)
        {
            videoPlayer.clip = endingVideo;
            videoPlayer.isLooping = true; // 循环播放
            videoPlayer.playOnAwake = false; // 不自动播放
            
            // 如果有视频显示组件，设置渲染纹理
            if (videoDisplay != null)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
                
                // 创建渲染纹理
                if (videoDisplay.texture == null || !(videoDisplay.texture is RenderTexture))
                {
                    int width = 1920; // 默认宽度
                    int height = 1080; // 默认高度
                    
                    if (endingVideo != null && videoPlayer.clip != null)
                    {
                        // 尝试获取视频分辨率
                        width = Mathf.Max(width, (int)videoPlayer.clip.width);
                        height = Mathf.Max(height, (int)videoPlayer.clip.height);
                    }
                    
                    RenderTexture renderTexture = new RenderTexture(width, height, 24);
                    videoDisplay.texture = renderTexture;
                    videoPlayer.targetTexture = renderTexture;
                }
                else
                {
                    videoPlayer.targetTexture = (RenderTexture)videoDisplay.texture;
                }
            }
            else
            {
                // 如果没有显示组件，使用材质渲染模式
                videoPlayer.renderMode = VideoRenderMode.MaterialOverride;
            }
        }
        
        // 设置结束音频
        if (endingAudio == null)
        {
            endingAudio = GetComponent<AudioSource>();
            if (endingAudio == null)
            {
                endingAudio = gameObject.AddComponent<AudioSource>();
            }
        }
        
        if (endingAudioClip != null && endingAudio != null)
        {
            endingAudio.clip = endingAudioClip;
            endingAudio.playOnAwake = false;
            endingAudio.loop = false;
        }
        
        // 如果没有指定HUD控制器，尝试查找
        if (hudControl == null)
        {
            hudControl = FindObjectOfType<HUDControl>();
        }
        
        // 初始时暂停视频
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }
    
    void Update()
    {
        // 检测O键按下
        if (Input.GetKeyDown(KeyCode.O) && !isFading && !hasTriggered && canvasGroup != null)
        {
            TriggerEnding();
        }
    }
    
    public void TriggerEnding()
    {
        hasTriggered = true;
        
        // 隐藏HUD
        if (hudControl != null)
        {
            hudControl.HideAllHUDElements();
            Debug.Log("已隐藏HUD元素");
        }
        
        // 开始播放视频
        if (videoPlayer != null && autoPlayVideo)
        {
            videoPlayer.Play();
            Debug.Log("开始播放结束视频");
        }
        
        // 开始播放结束音频
        if (endingAudio != null && endingAudioClip != null)
        {
            endingAudio.Play();
            Debug.Log("开始播放结束音频");
        }
        
        // 淡出背景音乐
        if (backgroundMusic != null && backgroundMusic.isPlaying)
        {
            // 停止之前的淡出协程（如果有）
            if (audioFadeCoroutine != null)
            {
                StopCoroutine(audioFadeCoroutine);
            }
            
            // 开始新的淡出协程
            audioFadeCoroutine = StartCoroutine(FadeOutAudio(backgroundMusic, audioFadeOutDuration));
            Debug.Log("开始淡出背景音乐");
        }
        
        // 显示结束画面
        StartCoroutine(FadeIn());
    }
    
    IEnumerator FadeIn()
    {
        isFading = true;
        
        // 从当前透明度渐变到完全不透明
        while (canvasGroup.alpha < 1.0f)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        // 确保最终完全不透明
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        
        isFading = false;
    }
    
    // 可选：添加一个渐隐方法
    public void FadeOut()
    {
        if (!isFading && canvasGroup != null)
        {
            StartCoroutine(FadeOutCoroutine());
        }
    }
    
    IEnumerator FadeOutCoroutine()
    {
        isFading = true;
        
        // 从不透明渐变到透明
        while (canvasGroup.alpha > 0.0f)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }
        
        // 确保最终完全透明
        canvasGroup.alpha = 0.0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        
        // 暂停视频和音频
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
        
        if (endingAudio != null && endingAudio.isPlaying)
        {
            endingAudio.Stop();
        }
        
        isFading = false;
        hasTriggered = false; // 重置触发状态，允许再次触发
        
        // 可选：恢复背景音乐
        // if (backgroundMusic != null && !backgroundMusic.isPlaying)
        // {
        //     backgroundMusic.volume = 1.0f;
        //     backgroundMusic.Play();
        // }
        
        // 可选：恢复HUD显示
        // if (hudControl != null)
        // {
        //     hudControl.ShowAllHUDElements();
        // }
    }
    
    // 音频渐弱协程
    IEnumerator FadeOutAudio(AudioSource audioSource, float duration)
    {
        float startVolume = audioSource.volume;
        float startTime = Time.time;
        
        while (Time.time < startTime + duration)
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration; // 0到1的进度值
            
            // 使用线性插值计算当前音量
            audioSource.volume = Mathf.Lerp(startVolume, 0, t);
            
            yield return null;
        }
        
        // 确保音量为0并暂停播放
        audioSource.volume = 0;
        audioSource.Pause();
        
        audioFadeCoroutine = null;
    }
    
    // 音频渐强协程（可选）
    public IEnumerator FadeInAudio(AudioSource audioSource, float targetVolume, float duration)
    {
        float startVolume = audioSource.volume;
        float startTime = Time.time;
        
        // 如果音频源暂停，开始播放
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
        
        while (Time.time < startTime + duration)
        {
            float elapsed = Time.time - startTime;
            float t = elapsed / duration; // 0到1的进度值
            
            // 使用线性插值计算当前音量
            audioSource.volume = Mathf.Lerp(startVolume, targetVolume, t);
            
            yield return null;
        }
        
        // 确保最终音量正确
        audioSource.volume = targetVolume;
    }
    
    // 暂停视频
    public void PauseVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Pause();
        }
    }
    
    // 继续播放视频
    public void ResumeVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Play();
        }
    }
    
    // 停止视频
    public void StopVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
    }
}