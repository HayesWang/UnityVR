using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectDetail : MonoBehaviour
{
    [Header("UI元素")]
    public GameObject detailPanel;
    public TextMeshProUGUI itemNameText;
    public TextMeshProUGUI itemDescriptionText;
    public RawImage displayImage;
    
    [Header("渲染设置")]
    public Camera renderCamera;
    public RenderTexture renderTexture;
    
    [Header("交互设置")]
    public float dragSpeed = 20f;
    
    [Header("物体管理")]
    public DisplayObjectManager objectManager;
    
    private GameObject currentObject;
    private bool isDragging = false;
    private Vector3 lastMousePosition;
    private CameraControl playerCamera;  // 引用玩家相机控制器
    
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
    }
    
    void Update()
    {
        // 空格键关闭
        if (detailPanel.activeInHierarchy && Input.GetKeyDown(KeyCode.Space))
        {
            ClosePanel();
        }
        
        // 处理旋转
        if (currentObject && detailPanel.activeInHierarchy)
        {
            HandleObjectRotation();
        }
    }
    
    void HandleObjectRotation()
    {
        // 处理拖拽旋转
        if (Input.GetMouseButtonDown(0) && IsMouseOverUI())
        {
            isDragging = true;
            lastMousePosition = Input.mousePosition;
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
            }
        }
        
        // 滚轮缩放
        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.01f && IsMouseOverUI())
        {
            currentObject.transform.localScale *= (1f + scroll * 0.1f);
        }
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
        
        // 禁用玩家相机控制
        DisableCameraControl();
        
        // 显示鼠标并解锁
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 设置UI文本
        if (itemNameText)
            itemNameText.text = item.itemName;
            
        if (itemDescriptionText)
            itemDescriptionText.text = item.itemDescription;
        
        // 显示3D模型
        if (objectManager)
        {
            currentObject = objectManager.ShowObject(item.itemId);
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
        
        // 锁定并隐藏鼠标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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