using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[System.Serializable]
public class NarrationPage
{
    [TextArea(3, 10)]
    public string content;         // 独白内容
    public float displayTime = 3f; // 自动显示时间（如果启用自动翻页）
    public AudioClip voiceClip;    // 可选的配音片段
}

public class NarrationManager : MonoBehaviour
{
    [Header("独白设置")]
    [SerializeField] private NarrationPage[] narrationPages;
    [SerializeField] private bool autoProgress = false;    // 是否自动翻页
    [SerializeField] private KeyCode nextPageKey = KeyCode.Space; // 手动翻页按键
    
    [Header("UI组件")]
    [SerializeField] private TextMeshProUGUI textDisplay;
    [SerializeField] private GameObject narrationPanel;  // 整个独白面板
    
    [Header("淡出设置")]
    [SerializeField] private float fadeOutDuration = 1.5f;
    [SerializeField] private Color textColor = Color.white;
    
    [Header("音频设置")]
    [SerializeField] private AudioSource audioSource;

    private int currentPageIndex = -1;
    private bool isDisplaying = false;
    private Coroutine displayCoroutine;

    // 旁白状态属性和事件
    public bool IsNarrationActive { get; private set; } = false;
    public event Action<bool> OnNarrationStateChanged;

    private void Start()
    {
        // 初始化
        if (narrationPanel == null)
        {
            narrationPanel = gameObject;
        }

        if (textDisplay != null)
        {
            textDisplay.color = textColor;
        }
        
        // 如果有独白页，立即显示第一页
        if (narrationPages != null && narrationPages.Length > 0)
        {
            ShowNextPage();
        }
    }

    private void Update()
    {
        // 检测按键翻页
        if (Input.GetKeyDown(nextPageKey) && !isDisplaying)
        {
            ShowNextPage();
        }
    }

    /// <summary>
    /// 显示下一页独白
    /// </summary>
    public void ShowNextPage()
    {
        // 如果当前有正在显示的协程，停止它
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }

        currentPageIndex++;
        
        // 检查是否还有更多页面
        if (currentPageIndex < narrationPages.Length)
        {
            // 设置旁白状态为活动
            SetNarrationActive(true);
            
            displayCoroutine = StartCoroutine(DisplayPage(narrationPages[currentPageIndex]));
        }
        else
        {
            // 所有独白已显示完毕，执行淡出
            StartCoroutine(FadeOut());
        }
    }

    /// <summary>
    /// 显示指定的独白页
    /// </summary>
    private IEnumerator DisplayPage(NarrationPage page)
    {
        isDisplaying = true;
        
        // 显示文本
        if (textDisplay != null)
        {
            textDisplay.text = page.content;
        }
        
        // 播放音频
        if (audioSource != null && page.voiceClip != null)
        {
            audioSource.clip = page.voiceClip;
            audioSource.Play();
        }
        
        // 如果启用了自动翻页，等待指定时间后翻页
        if (autoProgress)
        {
            yield return new WaitForSeconds(page.displayTime);
            ShowNextPage();
        }
        
        isDisplaying = false;
    }

    /// <summary>
    /// 淡出HUD
    /// </summary>
    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color originalTextColor = textDisplay.color;
        Image[] images = narrationPanel.GetComponentsInChildren<Image>();
        Color[] originalImageColors = new Color[images.Length];
        
        // 保存所有图像的原始颜色
        for (int i = 0; i < images.Length; i++)
        {
            originalImageColors[i] = images[i].color;
        }
        
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            float normalizedTime = elapsedTime / fadeOutDuration;
            float alpha = Mathf.Lerp(1f, 0f, normalizedTime);
            
            // 更新文本透明度
            if (textDisplay != null)
            {
                Color newColor = originalTextColor;
                newColor.a = alpha;
                textDisplay.color = newColor;
            }
            
            // 更新所有图像的透明度
            for (int i = 0; i < images.Length; i++)
            {
                Color newColor = originalImageColors[i];
                newColor.a = alpha;
                images[i].color = newColor;
            }
            
            yield return null;
        }
        
        // 完全隐藏
        narrationPanel.SetActive(false);
        
        // 设置旁白状态为非活动
        SetNarrationActive(false);
    }

    /// <summary>
    /// 设置旁白活动状态并触发事件
    /// </summary>
    private void SetNarrationActive(bool active)
    {
        if (IsNarrationActive != active)
        {
            IsNarrationActive = active;
            OnNarrationStateChanged?.Invoke(active);
        }
    }

    /// <summary>
    /// 重置独白，从头开始显示
    /// </summary>
    public void ResetNarration()
    {
        // 重置索引
        currentPageIndex = -1;
        
        // 停止任何正在进行的协程
        if (displayCoroutine != null)
        {
            StopCoroutine(displayCoroutine);
        }
        
        // 重置所有UI元素
        narrationPanel.SetActive(true);
        if (textDisplay != null)
        {
            Color c = textDisplay.color;
            c.a = 1f;
            textDisplay.color = c;
        }
        
        // 重置所有图像的透明度
        Image[] images = narrationPanel.GetComponentsInChildren<Image>();
        for (int i = 0; i < images.Length; i++)
        {
            Color c = images[i].color;
            c.a = 1f;
            images[i].color = c;
        }
        
        // 显示第一页
        ShowNextPage();
    }

    /// <summary>
    /// 跳转到指定页面
    /// </summary>
    public void JumpToPage(int pageIndex)
    {
        if (pageIndex >= 0 && pageIndex < narrationPages.Length)
        {
            currentPageIndex = pageIndex - 1;
            ShowNextPage();
        }
    }
}