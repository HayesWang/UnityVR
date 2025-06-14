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
    }

    private void Update()
    {
        // 检查与相机的距离
        CheckDistance();
        
        // 如果玩家在附近
        if (isPlayerNearby && Input.GetKeyDown(interactionKey))
        {
            LoadTargetScene();
        }
    }
    
    // 检查与相机的距离
    private void CheckDistance()
    {
        if (mainCamera == null) return;
        
        float distance = Vector3.Distance(mainCamera.position, transform.position);
        
        // 如果在检测距离内，显示描边和提示
        if (distance <= detectionDistance)
        {
            if (!isPlayerNearby)
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
        else
        {
            if (isPlayerNearby)
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
        // 创建一个父物体作为描边容器
        outlineObject = new GameObject(gameObject.name + "_Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = Vector3.zero;
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one; // 不再使用统一缩放
        
        // 复制所有MeshFilter和Renderer
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter meshFilter in meshFilters)
        {
            GameObject outlinePart = new GameObject("OutlinePart");
            outlinePart.transform.SetParent(outlineObject.transform);
            outlinePart.transform.position = meshFilter.transform.position;
            outlinePart.transform.rotation = meshFilter.transform.rotation;
            
            // 保持原有物体的xyz比例，同时整体放大
            Vector3 originalScale = meshFilter.transform.localScale;
            outlinePart.transform.localScale = new Vector3(
                originalScale.x * outlineScale,
                originalScale.y * outlineScale,
                originalScale.z * outlineScale
            );
            
            MeshFilter outlineMeshFilter = outlinePart.AddComponent<MeshFilter>();
            outlineMeshFilter.sharedMesh = meshFilter.sharedMesh;
            
            MeshRenderer outlineRenderer = outlinePart.AddComponent<MeshRenderer>();
            outlineRenderer.sharedMaterial = outlineMaterial;
        }
        
        // 复制所有SkinnedMeshRenderer
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
        {
            GameObject outlinePart = new GameObject("OutlinePart");
            outlinePart.transform.SetParent(outlineObject.transform);
            outlinePart.transform.position = skinnedMeshRenderer.transform.position;
            outlinePart.transform.rotation = skinnedMeshRenderer.transform.rotation;
            
            // 保持原有物体的xyz比例，同时整体放大
            Vector3 originalScale = skinnedMeshRenderer.transform.localScale;
            outlinePart.transform.localScale = new Vector3(
                originalScale.x * outlineScale,
                originalScale.y * outlineScale,
                originalScale.z * outlineScale
            );
            
            SkinnedMeshRenderer outlineSkinnedRenderer = outlinePart.AddComponent<SkinnedMeshRenderer>();
            outlineSkinnedRenderer.sharedMesh = skinnedMeshRenderer.sharedMesh;
            outlineSkinnedRenderer.sharedMaterial = outlineMaterial;
            outlineSkinnedRenderer.bones = skinnedMeshRenderer.bones;
            outlineSkinnedRenderer.rootBone = skinnedMeshRenderer.rootBone;
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

    // 加载目标场景
    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("目标场景名称未设置!");
            return;
        }
        
        Debug.Log("正在加载场景: " + targetSceneName);
        
        // 使用Unity内置的SceneManager加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
    }
}