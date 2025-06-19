using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections; // 添加这行来支持 IEnumerator

public class ObjectDetail : MonoBehaviour
{
    [Header("UI元素")]
    public GameObject detailPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public TextMeshProUGUI itemEraText;  // 添加年代显示文本
    public RawImage displayImage;
    
    [Header("渲染设置")]
    public Camera renderCamera;
    public RenderTexture renderTexture;
    
    [Header("交互设置")]
    public float dragSpeed = 20f;
    
    [Header("微小旋转设置")]
    [Tooltip("是否启用根据鼠标位置的微小旋转")]
    public bool enableSubtleRotation = true;
    [Tooltip("微小旋转的强度")]
    [Range(0.01f, 1.0f)]
    public float subtleRotationStrength = 0.15f;
    [Tooltip("微小旋转的平滑度")]
    [Range(0.1f, 10.0f)]
    public float subtleRotationSmoothing = 2.0f;
    
    [Header("物体管理")]
    public DisplayObjectManager objectManager;
    
    [Header("旁白设置")]
    [Tooltip("关闭界面后是否触发旁白")]
    public bool triggerNarrationOnClose = false;
    [Tooltip("要触发的旁白触发器")]
    public NarrationTrigger narrationTrigger; // 直接引用 NarrationTrigger
    
    private GameObject currentObject;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private CameraControl playerCamera;     // 引用玩家相机控制器
    private Quaternion targetRotation;      // 目标旋转
    private Quaternion originalRotation;    // 原始旋转
    private PickableItem currentItem;       // 当前物品
    private HUDControl hudControl;          // HUD控制器
    private bool ignoreSpaceInput = false; // 添加这个字段到类的顶部
    
    void Start()
    {
        // 初始化渲染纹理
        if (renderCamera && renderTexture && displayImage)
        {
            renderCamera.targetTexture = renderTexture;
            displayImage.texture = renderTexture;
            
            // 设置相机不渲染天空盒
            renderCamera.clearFlags = CameraClearFlags.SolidColor;
            renderCamera.backgroundColor = Color.clear; // 透明背景
        }
        
        // 初始隐藏面板
        if (detailPanel)
            detailPanel.SetActive(false);
            
        // 获取玩家相机控制器
        playerCamera = FindObjectOfType<CameraControl>();
        if (playerCamera == null)
        {
            Debug.LogWarning("未找到CameraControl组件，可能无法正确禁用相机控制");
        }
        
        // 获取HUD控制器
        hudControl = FindObjectOfType<HUDControl>();
    }
    
    void Update()
    {
        // 空格键关闭，但要检查是否应该忽略输入
        if (detailPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Space) && !ignoreSpaceInput)
        {
            ClosePanel();
            // 防止空格键事件传递到其他系统
            StartCoroutine(IgnoreSpaceForDuration(0.5f)); // 忽略0.5秒的空格输入
        }
        
        // 处理旋转
        if (currentObject && detailPanel.activeInHierarchy)
        {
            HandleObjectRotation();
        }
    }
    
    // 在指定时间内忽略空格键输入
    private IEnumerator IgnoreSpaceForDuration(float duration)
    {
        ignoreSpaceInput = true;
        yield return new WaitForSecondsRealtime(duration);
        ignoreSpaceInput = false;
        Debug.Log("空格键输入恢复正常");
    }
    
    // 临时屏蔽空格键输入，防止影响旁白系统
    private IEnumerator IgnoreSpaceForOneFrame()
    {
        // 等待当前帧结束
        yield return null;
        
        // 可选：额外等待一帧，确保旁白系统完全初始化
        yield return null;
    }
    
    void HandleObjectRotation()
    {
        // 处理拖拽旋转
        if (Input.GetMouseButtonDown(0) && IsMouseOverUI())
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
            
            // 保存拖拽开始时的旋转
            originalRotation = currentObject.transform.rotation;
        }
        
        if (isDragging)
        {
            if (Input.GetMouseButton(0))
            {
                Vector3 delta = Input.mousePosition - lastMousePosition;
                currentObject.transform.Rotate(Vector3.up, -delta.x * dragSpeed * Time.unscaledDeltaTime, Space.World);
                currentObject.transform.Rotate(Vector3.right, delta.y * dragSpeed * Time.unscaledDeltaTime, Space.World);
                lastMousePosition = Input.mousePosition;
            }
            else if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
                
                // 更新原始旋转为当前旋转
                originalRotation = currentObject.transform.rotation;
            }
        }
        else if (enableSubtleRotation && !isDragging)
        {
            // 添加根据鼠标位置的微小旋转
            ApplySubtleRotation();
        }
        
        // 滚轮缩放
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f && IsMouseOverUI())
        {
            currentObject.transform.localScale *= (1f + scroll * 0.1f);
        }
    }
    
    void ApplySubtleRotation()
    {
        // 获取鼠标在屏幕上的归一化位置 (0-1)
        Vector2 mousePos = Input.mousePosition;
        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 mouseOffset = (mousePos - screenCenter) / screenCenter; // 范围约为-1到1
        
        // 限制鼠标偏移量在合理范围内
        mouseOffset.x = Mathf.Clamp(mouseOffset.x, -1f, 1f);
        mouseOffset.y = Mathf.Clamp(mouseOffset.y, -1f, 1f);
        
        // 计算基于鼠标位置的目标旋转角度
        float targetAngleY = -mouseOffset.x * subtleRotationStrength * 15f; // 水平移动影响Y轴旋转
        float targetAngleX = mouseOffset.y * subtleRotationStrength * 10f;  // 垂直移动影响X轴旋转
        
        // 创建目标旋转
        Quaternion targetYaw = Quaternion.Euler(0, targetAngleY, 0);
        Quaternion targetPitch = Quaternion.Euler(targetAngleX, 0, 0);
        targetRotation = originalRotation * targetYaw * targetPitch;
        
        // 平滑过渡到目标旋转
        currentObject.transform.rotation = Quaternion.Slerp(
            currentObject.transform.rotation, 
            targetRotation, 
            Time.unscaledDeltaTime * subtleRotationSmoothing
        );
    }
    
    bool IsMouseOverUI()
    {
        if (!displayImage) return false;
        
        RectTransform rect = displayImage.GetComponent<RectTransform>();
        Vector2 local;
        
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect, Input.mousePosition, null, out local))
        {
            return rect.rect.Contains(local);
        }
        
        return false;
    }
    
    public void ShowItemDetail(PickableItem item)
    {
        if (!item) return;
        
        // 保存当前物品引用
        currentItem = item;
        
        // 禁用玩家相机控制
        DisableCameraControl();
        
        // 隐藏HUD
        if (hudControl != null)
        {
            hudControl.HideAllHUDElements();
        }
        
        // 显示鼠标并解锁
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 设置UI文本
        if (itemNameText)
            itemNameText.text = item.itemName;
            
        if (itemDescriptionText)
            itemDescriptionText.text = item.itemDescription;
            
        // 添加年代显示
        if (itemEraText)
            itemEraText.text = item.itemEra;
        
        // 显示3D模型
        if (objectManager)
        {
            currentObject = objectManager.ShowObject(item.itemId);
            
            // 保存初始旋转状态
            if (currentObject)
            {
                originalRotation = currentObject.transform.rotation;
                targetRotation = originalRotation;
            }
        }
        
        // 显示面板
        detailPanel.SetActive(true);
        
        // 暂停游戏
        Time.timeScale = 0f;
    }
    
    public void ClosePanel()
    {
        detailPanel.SetActive(false);
        
        // 隐藏所有物体
        if (objectManager)
        {
            objectManager.HideAllObjects();
        }
        
        currentObject = null;
        
        // 恢复游戏
        Time.timeScale = 1f;
        
        // 重新启用玩家相机控制
        EnableCameraControl();
        
        // 显示HUD
        if (hudControl != null)
        {
            hudControl.ShowAllHUDElements();  // 修改这里
        }
        
        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        // 使用延迟调用触发旁白，确保详情界面完全关闭后再触发
        if (triggerNarrationOnClose && narrationTrigger != null)
        {
            // 延迟调用，确保界面已经完全关闭
            StartCoroutine(DelayedNarrationTrigger());
        }
    }

    private IEnumerator DelayedNarrationTrigger()
    {
        // 等待足够长的时间，确保详情界面完全关闭
        yield return new WaitForSecondsRealtime(0.3f);
        
        // 现在触发旁白
        TriggerNarration();
    }
    
    // 直接调用 NarrationTrigger 的旁白触发功能
    private void TriggerNarration()
    {
        if (narrationTrigger == null)
        {
            Debug.LogError("未设置 NarrationTrigger，无法触发旁白");
            return;
        }
        
        // 使用反射调用 NarrationTrigger 的私有方法 TriggerNarration
        try
        {
            System.Type type = narrationTrigger.GetType();
            System.Reflection.MethodInfo method = type.GetMethod("TriggerNarration", 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
                
            if (method != null)
            {
                method.Invoke(narrationTrigger, null);
                Debug.Log("成功通过 NarrationTrigger 触发旁白");
            }
            else
            {
                Debug.LogError("无法找到 NarrationTrigger.TriggerNarration 方法");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("调用 NarrationTrigger 时发生错误: " + e.Message);
        }
    }
    
    // 禁用相机控制
    private void DisableCameraControl()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = false;
        }
    }
    
    // 重新启用相机控制
    private void EnableCameraControl()
    {
        if (playerCamera != null)
        {
            playerCamera.enabled = true;
        }
    }
}