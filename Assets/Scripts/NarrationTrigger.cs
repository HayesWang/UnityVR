using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationTrigger : MonoBehaviour
{
    [Header("基本设置")]
    public float interactionDistance = 3f;    // 交互距离
    public string promptText = "旁白触发点";  // 提示文本
    public KeyCode interactionKey = KeyCode.F; // 交互按键
    public bool canInteractOnce = true;       // 是否只能交互一次
    
    [Header("UI显示设置")]
    public Sprite promptIcon;                 // 提示图标
    public Color promptTextColor = Color.white; // 提示文本颜色
    public string promptDescription = "按F键触发旁白"; // 提示描述
    
    [Header("描边设置")]
    public Color outlineColor = new Color(0.2f, 0.8f, 1f, 1f); // 蓝色
    public float outlineScale = 1.05f;        // 描边物体的放大比例
    
    [Header("旁白设置")]
    public NarrationManager narrationManager; // 旁白管理器引用
    
    private GameObject outlineObject;         // 描边对象
    private Material outlineMaterial;         // 描边材质
    [HideInInspector]
    public bool hasInteracted = false;       // 是否已经交互过
    private bool isPlayerNearby = false;      // 玩家是否在附近
    private HUDControl hudControl;            // HUD控制器引用

    void Start()
    {
        // 创建描边材质
        CreateOutlineMaterial();
        
        // 创建描边对象
        CreateOutlineObject();
        
        // 获取HUD控制器
        hudControl = FindObjectOfType<HUDControl>();
        
        // 如果未指定旁白管理器，尝试查找场景中的旁白管理器
        if (narrationManager == null)
        {
            narrationManager = FindObjectOfType<NarrationManager>();
            if (narrationManager == null)
            {
                Debug.LogWarning("未找到NarrationManager，请在Inspector中分配或添加到场景中");
            }
        }
    }

    void Update()
    {
        // 检查与相机的距离
        CheckDistance();
        
        // 如果玩家在附近且按下交互键，触发旁白
        if (isPlayerNearby && Input.GetKeyDown(interactionKey) && (!canInteractOnce || !hasInteracted))
        {
            TriggerNarration();
        }
    }
    
    // 检查与相机的距离
    private void CheckDistance()
    {
        if (Camera.main == null)
            return;
            
        // 计算当前对象与相机的距离
        float distance = Vector3.Distance(transform.position, Camera.main.transform.position);
        
        // 之前不在范围内，现在进入范围
        if (!isPlayerNearby && distance <= interactionDistance)
        {
            isPlayerNearby = true;
            
            // 显示交互提示
            if (hudControl != null)
            {
                hudControl.ShowTriggerButton();
                hudControl.ShowItemInfo(promptText, promptTextColor);
            }
            
            // 显示描边高亮效果
            ShowHighlight();
        }
        // 之前在范围内，现在离开范围
        else if (isPlayerNearby && distance > interactionDistance)
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
    
    // 触发旁白
    private void TriggerNarration()
    {
        if (narrationManager != null)
        {
            // 如果之前未交互，则增加进度
            if (!hasInteracted)
            {
                hasInteracted = true;
                
                // 更新HUD语音进度
                if (hudControl != null)
                {
                    hudControl.IncreaseNarrationsProgress();
                }
            }
            
            // 隐藏交互提示
            if (hudControl != null)
            {
                hudControl.HideItemInfo();
                hudControl.HideTriggerButton();
            }
            
            // 如果只能交互一次，隐藏高亮
            if (canInteractOnce)
            {
                HideHighlight();
            }
            
            // 重置并显示旁白
            narrationManager.ResetNarration();
        }
        else
        {
            Debug.LogWarning("未设置NarrationManager，无法触发旁白");
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
        
        // 添加发光效果
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
        if (outlineObject != null && (!canInteractOnce || !hasInteracted))
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

    // 重置交互状态
    public void ResetInteraction()
    {
        hasInteracted = false;
        
        if (isPlayerNearby)
        {
            ShowHighlight();
            
            if (hudControl != null)
            {
                hudControl.ShowTriggerButton();
                hudControl.ShowItemInfo(promptText, promptTextColor);
            }
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
    
    // 用于在编辑器中可视化交互范围
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(outlineColor.r, outlineColor.g, outlineColor.b, 0.3f);
        Gizmos.DrawSphere(transform.position, interactionDistance);
    }
}