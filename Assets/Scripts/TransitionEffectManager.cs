using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TransitionEffectManager : MonoBehaviour
{
    public static TransitionEffectManager Instance { get; private set; }

    [Header("转场效果设置")]
    public Material transitionMaterial;
    public float transitionDuration = 1.5f;
    public Color transitionColor = Color.black;
    
    [Header("高级设置")]
    public float extraRadius = 0.2f;
    public float edgeSmoothness = 0.02f;
    
    private string targetSceneName;
    private Camera mainCamera;
    private bool isTransitioning = false;
    
    // 用于渲染转场效果的专用摄像机
    private Camera effectCamera;
    
    private void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeEffectCamera();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // 获取主摄像机引用
        UpdateMainCameraReference();
        
        // 初始化材质
        if (transitionMaterial != null)
        {
            // 复制材质以防止修改原始资源
            transitionMaterial = new Material(transitionMaterial);
            transitionMaterial.SetFloat("_Progress", 0);
            transitionMaterial.SetColor("_Color", transitionColor);
            transitionMaterial.SetFloat("_Radius", extraRadius);
            transitionMaterial.SetFloat("_Smoothness", edgeSmoothness);
        }
        else
        {
            Debug.LogError("转场材质未设置!");
        }
        
        // 设置渲染相机的材质
        if (effectCamera != null && effectCamera.TryGetComponent<UnityEngine.Camera>(out var cam))
        {
            cam.targetTexture = null;
            cam.clearFlags = CameraClearFlags.Nothing;
            cam.cullingMask = 0;
            cam.renderingPath = RenderingPath.Forward;
            cam.enabled = true;
        }
    }
    
    private void InitializeEffectCamera()
    {
        // 创建效果相机
        GameObject effectCameraObj = new GameObject("TransitionEffectCamera");
        effectCameraObj.transform.SetParent(transform);
        effectCamera = effectCameraObj.AddComponent<Camera>();
        effectCamera.depth = 100; // 确保在所有相机之上渲染
        effectCamera.clearFlags = CameraClearFlags.Nothing;
        effectCamera.cullingMask = 0; // 不渲染任何层
        effectCamera.renderingPath = RenderingPath.Forward;
        
        // 添加后处理脚本
        EffectRenderComponent effectRenderer = effectCameraObj.AddComponent<EffectRenderComponent>();
        effectRenderer.transitionMaterial = transitionMaterial;
    }
    
    private void UpdateMainCameraReference()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("找不到主摄像机，效果可能无法正确显示");
        }
    }
    
    private void OnEnable()
    {
        // 监听场景加载事件
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        // 移除场景加载事件监听
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 更新主摄像机引用
        UpdateMainCameraReference();
        
        // 开始淡入效果
        StartCoroutine(TransitionInEffect());
    }
    
    /// <summary>
    /// 开始场景转换
    /// </summary>
    public void TransitionToScene(string sceneName)
    {
        if (isTransitioning) return;
        
        targetSceneName = sceneName;
        StartCoroutine(TransitionOutEffect());
    }
    
    // 淡出效果协程
    private IEnumerator TransitionOutEffect()
    {
        isTransitioning = true;
        
        // 更新中心点为摄像机位置
        if (mainCamera != null)
        {
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(mainCamera.transform.position + mainCamera.transform.forward * 0.1f);
            transitionMaterial.SetFloat("_CenterX", viewportPoint.x);
            transitionMaterial.SetFloat("_CenterY", viewportPoint.y);
        }
        else
        {
            // 默认使用屏幕中心
            transitionMaterial.SetFloat("_CenterX", 0.5f);
            transitionMaterial.SetFloat("_CenterY", 0.5f);
        }
        
        // 开始淡出动画
        float startTime = Time.time;
        while (Time.time < startTime + transitionDuration)
        {
            float progress = (Time.time - startTime) / transitionDuration;
            transitionMaterial.SetFloat("_Progress", progress);
            yield return null;
        }
        
        // 确保最终值为1
        transitionMaterial.SetFloat("_Progress", 1);
        
        // 加载新场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        
        isTransitioning = false;
    }
    
    // 淡入效果协程
    private IEnumerator TransitionInEffect()
    {
        isTransitioning = true;
        
        // 设置初始值
        transitionMaterial.SetFloat("_Progress", 1);
        
        // 更新中心点为摄像机位置
        if (mainCamera != null)
        {
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(mainCamera.transform.position + mainCamera.transform.forward * 0.1f);
            transitionMaterial.SetFloat("_CenterX", viewportPoint.x);
            transitionMaterial.SetFloat("_CenterY", viewportPoint.y);
        }
        
        // 开始淡入动画
        float startTime = Time.time;
        while (Time.time < startTime + transitionDuration)
        {
            float progress = 1 - (Time.time - startTime) / transitionDuration;
            transitionMaterial.SetFloat("_Progress", progress);
            yield return null;
        }
        
        // 确保最终值为0
        transitionMaterial.SetFloat("_Progress", 0);
        
        isTransitioning = false;
    }
}

// 负责渲染后处理效果的组件
[RequireComponent(typeof(Camera))]
public class EffectRenderComponent : MonoBehaviour
{
    public Material transitionMaterial;
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (transitionMaterial != null)
        {
            Graphics.Blit(source, destination, transitionMaterial);
        }
        else
        {
            Graphics.Blit(source, destination);
        }
    }
}