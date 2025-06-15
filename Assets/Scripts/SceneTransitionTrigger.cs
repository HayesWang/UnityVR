using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionTrigger : MonoBehaviour
{
    [Header("场景跳转设置")]
    public string targetSceneName;         // 目标场景名称
    
    [Header("交互设置")]
    public KeyCode interactionKey = KeyCode.F; // 交互按键
    public string interactionPrompt = "按F键进入"; // 交互提示
    public float detectionDistance = 3f;   // 检测距离

    [Header("描边设置")]
    public Color outlineColor = new Color(0f, 0.8f, 1f, 1f); // 蓝色描边
    public float outlineScale = 1.05f;     // 描边物体的放大比例

    [Header("场景转换特效")]
    public GameObject transitionParticlePrefab; // 场景转换粒子特效预制体
    public float particleEffectDuration = 1.5f; // 粒子特效持续时间

    private bool isPlayerNearby = false;
    private HUDControl hudControl;
    private Transform mainCamera;
    
    // 描边相关
    private GameObject outlineObject;
    private Material outlineMaterial;
    
    private void Start()
    {
        // 获取HUD控制器
        hudControl = FindObjectOfType<HUDControl>();
        
        // 获取主相机
        mainCamera = Camera.main.transform;
        
        // 创建描边材质和对象
        CreateOutlineMaterial();
        CreateOutlineObject();

        // 移除了默认粒子特效的创建，只使用指定的预制体
    }

    private void Update()
    {
        // 检查与相机的距离
        CheckDistance();
        
        // 如果玩家在附近，按下交互键时才播放粒子效果并加载场景
        if (isPlayerNearby && Input.GetKeyDown(interactionKey))
        {
            PlayTransitionEffectAndLoadScene();
        }
    }
    
    // 播放转场特效并加载场景
    private void PlayTransitionEffectAndLoadScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("目标场景名称未设置!");
            return;
        }
        
        // 停止显示交互提示
        if (hudControl != null)
        {
            hudControl.HideItemInfo();
            hudControl.HideTriggerButton();
        }
        
        // 播放粒子特效 - 只在按下交互按钮时播放指定的粒子特效
        if (transitionParticlePrefab != null)
        {
            // 在玩家与触发器的交互点创建粒子效果
            Vector3 effectPosition = transform.position;
            if (mainCamera != null)
            {
                // 计算玩家与物体之间的中点，或者略微偏向玩家位置
                effectPosition = Vector3.Lerp(transform.position, mainCamera.position, 0.3f);
            }
            
            GameObject particleEffect = Instantiate(transitionParticlePrefab, effectPosition, Quaternion.identity);
            particleEffect.SetActive(true);
            
            // 不要立即销毁粒子效果对象，让它完成播放
            ParticleSystem ps = particleEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
                Destroy(particleEffect, particleEffectDuration);
            }
            
            Debug.Log("播放场景转换粒子特效");
        }
        else
        {
            Debug.LogWarning("未指定场景转换粒子特效预制体!");
        }
        
        Debug.Log("正在加载场景: " + targetSceneName);
        
        // 延迟加载场景，让粒子效果有时间播放
        if (SceneManager.Instance != null)
        {
            SceneManager.Instance.LoadScene(targetSceneName, particleEffectDuration * 0.7f);
        }
        else
        {
            // 如果没有找到SceneManager实例，使用协程延迟加载
            StartCoroutine(DelayedLoadScene(targetSceneName, particleEffectDuration * 0.7f));
        }
    }
    
    // 延迟加载场景的协程(备用方案)
    private System.Collections.IEnumerator DelayedLoadScene(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    // 创建描边材质
    private void CreateOutlineMaterial()
    {
        outlineMaterial = new Material(Shader.Find("Standard"));
        outlineMaterial.SetFloat("_Mode", 3); // 透明模式
        outlineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        outlineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        outlineMaterial.SetInt("_ZWrite", 0);
        outlineMaterial.DisableKeyword("_ALPHATEST_ON");
        outlineMaterial.EnableKeyword("_ALPHABLEND_ON");
        outlineMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        outlineMaterial.renderQueue = 3000;
        
        // 设置描边颜色
        Color outlineColorWithAlpha = outlineColor;
        outlineColorWithAlpha.a = 0.5f; // 半透明
        outlineMaterial.color = outlineColorWithAlpha;
        outlineMaterial.EnableKeyword("_EMISSION");
        outlineMaterial.SetColor("_EmissionColor", outlineColor * 0.5f);
    }

    // 创建描边对象
    private void CreateOutlineObject()
    {
        // 创建一个放大版本的物体
        outlineObject = Instantiate(gameObject, transform.position, transform.rotation, transform);
        outlineObject.name = gameObject.name + "_Outline";
        
        // 移除所有非渲染相关组件
        Component[] components = outlineObject.GetComponentsInChildren<Component>();
        foreach (Component component in components)
        {
            if (!(component is Transform) && 
                !(component is MeshFilter) && 
                !(component is MeshRenderer) && 
                !(component is SkinnedMeshRenderer))
            {
                Destroy(component);
            }
        }
        
        // 放大描边物体
        outlineObject.transform.localScale = Vector3.one * outlineScale;
        
        // 应用描边材质
        Renderer[] renderers = outlineObject.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            Material[] materials = new Material[renderer.sharedMaterials.Length];
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = outlineMaterial;
            }
            renderer.sharedMaterials = materials;
        }
        
        // 默认隐藏描边
        outlineObject.SetActive(false);
    }

    // 显示物体描边
    private void ShowHighlight()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(true);
        }
    }

    // 隐藏物体描边
    private void HideHighlight()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (outlineObject != null)
        {
            Destroy(outlineObject);
        }
        
        if (outlineMaterial != null)
        {
            Destroy(outlineMaterial);
        }
    }

    // 检测玩家是否进入交互范围 (保留原有的触发器检测作为备选方案)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            ShowHighlight();
            
            // 显示交互提示
            if (hudControl != null)
            {
                hudControl.ShowItemInfo(interactionPrompt);
                hudControl.ShowTriggerButton();
            }
        }
    }

    // 检测玩家是否离开交互范围 (保留原有的触发器检测作为备选方案)
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            HideHighlight();
            
            // 隐藏交互提示
            if (hudControl != null)
            {
                hudControl.HideItemInfo();
                hudControl.HideTriggerButton();
            }
        }
    }

    // 加载目标场景 (替换为使用PlayTransitionEffectAndLoadScene)
    private void LoadTargetScene()
    {
        PlayTransitionEffectAndLoadScene();
    }

    // 检查与相机的距离
    private void CheckDistance()
    {
        if (mainCamera == null)
            return;
            
        // 计算当前对象与相机的距离
        float distance = Vector3.Distance(transform.position, mainCamera.position);
        
        // 之前不在范围内，现在进入范围
        if (!isPlayerNearby && distance <= detectionDistance)
        {
            isPlayerNearby = true;
            
            // 显示交互提示
            if (hudControl != null)
            {
                hudControl.ShowTriggerButton();
                hudControl.ShowItemInfo(interactionPrompt);
            }
            
            // 显示描边高亮效果
            ShowHighlight();
        }
        // 之前在范围内，现在离开范围
        else if (isPlayerNearby && distance > detectionDistance)
        {
            isPlayerNearby = false;
            
            // 隐藏交互提示
            if (hudControl != null)
            {
                hudControl.HideTriggerButton();
                hudControl.HideItemInfo();
            }
            
            // 隐藏描边高亮效果
            HideHighlight();
        }
    }
}