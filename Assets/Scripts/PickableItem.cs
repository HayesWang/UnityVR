using UnityEngine;

public class PickableItem : MonoBehaviour
{
    [Header("基本设置")]
    public float pickUpDistance = 30f;    // 拾取距离
    public string itemName = "物品";      // 物品名称
    public bool isPicked = false;        // 是否已被拾取
    public bool canPickup = true;        // 是否可以拾取

    [Header("UI显示设置")]
    public Sprite itemThumbnail;         // 物品缩略图
    public Color itemNameColor = Color.white; // 物品名称颜色
    public string itemDescription = "";   // 物品描述

    [Header("描边设置")]
    public Color outlineColor = new Color(1f, 0.8f, 0f, 1f); // 金色
    public float outlineScale = 1.05f;    // 描边物体的放大比例

    private GameObject outlineObject;     // 描边对象
    private Material outlineMaterial;     // 描边材质

    void Start()
    {
        // 创建描边材质
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
        
        // 创建描边对象
        CreateOutlineObject();
    }

    void CreateOutlineObject()
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

    void OnDestroy()
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

    // 显示物体描边
    public void ShowHighlight()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(true);
        }
    }

    // 隐藏物体描边
    public void HideHighlight()
    {
        if (outlineObject != null)
        {
            outlineObject.SetActive(false);
        }
    }

    // 保留这两个方法以便向后兼容
    public void ShowName()
    {
        ShowHighlight();
    }

    public void HideName()
    {
        HideHighlight();
    }
}