using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class start : MonoBehaviour
{
    [Header("旁白设置")]
    public NarrationManager narrationManager; // 要激活的旁白管理器
    
    [Header("场景跳转设置")]
    public string targetSceneName = "YourTargetScene"; // 目标场景名称
    public float waitTimeAfterNarration = 1f; // 旁白结束后等待时间
    
    [Header("场景转换触发器设置(可选)")]
    public SceneTransitionTrigger sceneTransitionTrigger; // 可选的场景转换触发器
    public bool useTransitionTrigger = false; // 是否使用转换触发器
    
    [Header("开始界面控制")]
    public Canvas startUICanvas; // 开始界面Canvas
    public GameObject startUIPanel; // 开始界面面板
    public CanvasGroup startUICanvasGroup; // 用于渐隐效果的CanvasGroup
    public float fadeOutSpeed = 2f; // 界面渐隐速度
    public bool useInstantHide = false; // 是否立即隐藏（不使用渐变）
    
    private bool hasTriggered = false; // 防止重复触发
    private bool isWaitingForNarration = false; // 是否正在等待旁白结束
    
    void Start()
    {
        // 如果没有指定旁白管理器，尝试在场景中查找
        if (narrationManager == null)
        {
            narrationManager = FindObjectOfType<NarrationManager>();
            if (narrationManager == null)
            {
                Debug.LogWarning("未找到NarrationManager，将直接跳转场景");
            }
        }
        
        // 如果要使用转换触发器但没有指定，尝试查找
        if (useTransitionTrigger && sceneTransitionTrigger == null)
        {
            sceneTransitionTrigger = FindObjectOfType<SceneTransitionTrigger>();
            if (sceneTransitionTrigger == null)
            {
                Debug.LogWarning("未找到SceneTransitionTrigger，将使用直接场景跳转");
                useTransitionTrigger = false;
            }
        }
        
        // 自动查找开始界面组件
        SetupStartUI();
    }
    
    void SetupStartUI()
    {
        // 如果没有指定开始界面Canvas，尝试找到当前GameObject的Canvas
        if (startUICanvas == null)
        {
            startUICanvas = GetComponentInParent<Canvas>();
            if (startUICanvas == null)
            {
                startUICanvas = FindObjectOfType<Canvas>();
            }
        }
        
        // 如果没有指定开始界面面板，使用当前GameObject
        if (startUIPanel == null)
        {
            startUIPanel = gameObject;
        }
        
        // 如果没有CanvasGroup且需要渐变效果，自动添加
        if (startUICanvasGroup == null && !useInstantHide)
        {
            if (startUICanvas != null)
            {
                startUICanvasGroup = startUICanvas.GetComponent<CanvasGroup>();
                if (startUICanvasGroup == null)
                {
                    startUICanvasGroup = startUICanvas.gameObject.AddComponent<CanvasGroup>();
                }
            }
            else if (startUIPanel != null)
            {
                startUICanvasGroup = startUIPanel.GetComponent<CanvasGroup>();
                if (startUICanvasGroup == null)
                {
                    startUICanvasGroup = startUIPanel.AddComponent<CanvasGroup>();
                }
            }
        }
    }
    
    void Update()
    {
        // 只在未触发且未等待旁白时响应空格键
        if (Input.GetKeyDown(KeyCode.Space) && !hasTriggered && !isWaitingForNarration)
        {
            StartNarrationSequence();
        }
    }
    
    void StartNarrationSequence()
    {
        hasTriggered = true;
        
        // 隐藏开始界面
        HideStartUI();
        
        if (narrationManager != null)
        {
            Debug.Log("开始播放旁白");
            
            // 订阅旁白状态变化事件
            narrationManager.OnNarrationStateChanged += OnNarrationStateChanged;
            
            // 重置并开始播放旁白
            narrationManager.ResetNarration();
            isWaitingForNarration = true;
        }
        else
        {
            Debug.LogWarning("旁白管理器未设置，直接跳转场景");
            // 如果没有旁白管理器，直接跳转场景
            StartCoroutine(TransitionToScene());
        }
    }
    
    void HideStartUI()
    {
        if (useInstantHide)
        {
            // 立即隐藏
            if (startUICanvas != null)
            {
                startUICanvas.gameObject.SetActive(false);
                Debug.Log("立即隐藏开始界面Canvas");
            }
            else if (startUIPanel != null)
            {
                startUIPanel.SetActive(false);
                Debug.Log("立即隐藏开始界面面板");
            }
        }
        else
        {
            // 使用渐变效果隐藏
            if (startUICanvasGroup != null)
            {
                StartCoroutine(FadeOutStartUI());
                Debug.Log("开始渐隐开始界面");
            }
            else
            {
                // 如果没有CanvasGroup，回退到立即隐藏
                Debug.LogWarning("没有找到CanvasGroup，使用立即隐藏");
                if (startUICanvas != null)
                {
                    startUICanvas.gameObject.SetActive(false);
                }
                else if (startUIPanel != null)
                {
                    startUIPanel.SetActive(false);
                }
            }
        }
    }
    
    IEnumerator FadeOutStartUI()
    {
        float startAlpha = startUICanvasGroup.alpha;
        
        while (startUICanvasGroup.alpha > 0)
        {
            startUICanvasGroup.alpha -= Time.deltaTime * fadeOutSpeed;
            yield return null;
        }
        
        // 确保完全透明
        startUICanvasGroup.alpha = 0f;
        startUICanvasGroup.interactable = false;
        startUICanvasGroup.blocksRaycasts = false;
        
        // 可选：完全隐藏GameObject以节省性能
        if (startUICanvas != null)
        {
            startUICanvas.gameObject.SetActive(false);
        }
        else if (startUIPanel != null)
        {
            startUIPanel.SetActive(false);
        }
        
        Debug.Log("开始界面渐隐完成");
    }
    
    private void OnNarrationStateChanged(bool isActive)
    {
        // 当旁白变为非活动状态时，开始场景跳转
        if (!isActive && isWaitingForNarration)
        {
            Debug.Log("旁白播放完成，准备跳转场景");
            isWaitingForNarration = false;
            
            // 取消订阅事件
            if (narrationManager != null)
            {
                narrationManager.OnNarrationStateChanged -= OnNarrationStateChanged;
            }
            
            // 开始场景跳转
            StartCoroutine(TransitionToScene());
        }
    }
    
    IEnumerator TransitionToScene()
    {
        // 等待指定时间
        if (waitTimeAfterNarration > 0)
        {
            Debug.Log($"等待 {waitTimeAfterNarration} 秒后跳转场景");
            yield return new WaitForSeconds(waitTimeAfterNarration);
        }
        
        // 执行场景跳转
        if (useTransitionTrigger && sceneTransitionTrigger != null)
        {
            Debug.Log("使用场景转换触发器跳转");
            // 如果SceneTransitionTrigger有公共方法TriggerTransition，使用它
            // 否则使用反射调用私有方法
            try
            {
                var method = typeof(SceneTransitionTrigger).GetMethod("TriggerTransition");
                if (method != null)
                {
                    method.Invoke(sceneTransitionTrigger, null);
                }
                else
                {
                    // 使用反射调用私有方法StartTransition
                    var startTransitionMethod = typeof(SceneTransitionTrigger)
                        .GetMethod("StartTransition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (startTransitionMethod != null)
                    {
                        startTransitionMethod.Invoke(sceneTransitionTrigger, null);
                    }
                    else
                    {
                        Debug.LogError("无法找到场景转换方法，回退到直接跳转");
                        LoadTargetScene();
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("调用场景转换触发器时出错: " + e.Message);
                LoadTargetScene();
            }
        }
        else
        {
            Debug.Log("直接跳转到目标场景");
            LoadTargetScene();
        }
    }
    
    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            try
            {
                Debug.Log($"跳转到场景: {targetSceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
            }
            catch (System.Exception e)
            {
                Debug.LogError("加载场景时出错: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("目标场景名称未设置！");
        }
    }
    
    // 清理事件订阅，防止内存泄漏
    void OnDestroy()
    {
        if (narrationManager != null)
        {
            narrationManager.OnNarrationStateChanged -= OnNarrationStateChanged;
        }
    }
    
    // 重置状态，允许再次触发（可选，用于测试）
    public void ResetTrigger()
    {
        hasTriggered = false;
        isWaitingForNarration = false;
        
        // 可选：重新显示开始界面
        ShowStartUI();
    }
    
    // 显示开始界面（用于重置或调试）
    public void ShowStartUI()
    {
        if (startUICanvas != null)
        {
            startUICanvas.gameObject.SetActive(true);
            if (startUICanvasGroup != null)
            {
                startUICanvasGroup.alpha = 1f;
                startUICanvasGroup.interactable = true;
                startUICanvasGroup.blocksRaycasts = true;
            }
        }
        else if (startUIPanel != null)
        {
            startUIPanel.SetActive(true);
        }
    }
}