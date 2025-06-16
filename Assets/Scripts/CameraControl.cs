using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;        // 移动速度
    public float rotationSpeed = 2f;    // 视角旋转速度
    public float fixedHeight = 1.8f;    // 相机固定高度

    [Header("方向锁定")]
    public bool enableDirectionLock = false;  // 是否启用方向锁定
    public enum AllowedDirection         // 允许移动的方向枚举
    {
        All,                // 所有方向
        ForwardOnly,        // 只能前进
        ForwardBackward,    // 只能前后移动
        LeftRight,          // 只能左右移动
        Custom              // 自定义方向限制
    }
    public AllowedDirection allowedDirection = AllowedDirection.All;  // 允许的移动方向
    [Range(0, 1)]
    public float forwardMovement = 1f;  // 前/后移动权重 (1=允许, 0=禁止)
    [Range(0, 1)]
    public float rightMovement = 1f;    // 左/右移动权重 (1=允许, 0=禁止)

    [Header("拾取设置")]
    public KeyCode pickUpKey = KeyCode.F; // 拾取按键改为F键

    [Header("HUD引用")]
    public HUDControl hudControl;       // HUD控制器引用
    
    [Header("旁白控制")]
    public NarrationManager narrationManager; // 旁白管理器引用
    public bool canMoveWhileNarration = false; // 旁白期间是否可以移动

    private float rotationX = 0f;       // 垂直旋转角度
    private float rotationY = 0f;       // 水平旋转角度
    private PickableItem currentItem;   // 当前可拾取的物品
    private float initialY;             // 初始Y位置
    private bool canMove = true;        // 是否可以移动

    // Start is called before the first frame update
    void Start()
    {
        // 锁定并隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 保存初始高度
        initialY = transform.position.y;

        // 如果没有指定HUD控制器，尝试自动查找
        if (hudControl == null)
        {
            hudControl = FindObjectOfType<HUDControl>();
            if (hudControl == null)
            {
                Debug.LogWarning("未找到HUDControl组件，物品名称将不会显示在HUD上");
            }
        }
        
        // 如果没有指定旁白管理器，尝试自动查找
        if (narrationManager == null)
        {
            narrationManager = FindObjectOfType<NarrationManager>();
            if (narrationManager == null)
            {
                Debug.LogWarning("未找到NarrationManager组件，旁白控制将不起作用");
            }
            else
            {
                // 订阅旁白状态变化事件
                narrationManager.OnNarrationStateChanged += OnNarrationStateChanged;
                
                // 初始化移动状态
                if (!canMoveWhileNarration)
                {
                    canMove = !narrationManager.IsNarrationActive;
                }
            }
        }
        
        // 根据选择的方向锁定类型设置移动权重
        UpdateMovementWeights();
    }
    
    // 根据选择的方向锁定更新移动权重
    private void UpdateMovementWeights()
    {
        if (!enableDirectionLock)
        {
            forwardMovement = 1f;
            rightMovement = 1f;
            return;
        }
        
        switch (allowedDirection)
        {
            case AllowedDirection.All:
                forwardMovement = 1f;
                rightMovement = 1f;
                break;
            case AllowedDirection.ForwardOnly:
                forwardMovement = 1f;
                rightMovement = 0f;
                break;
            case AllowedDirection.ForwardBackward:
                forwardMovement = 1f;
                rightMovement = 0f;
                break;
            case AllowedDirection.LeftRight:
                forwardMovement = 0f;
                rightMovement = 1f;
                break;
            case AllowedDirection.Custom:
                // 使用Inspector中设置的自定义值
                break;
        }
    }
    
    // 在销毁时取消订阅事件
    private void OnDestroy()
    {
        if (narrationManager != null)
        {
            narrationManager.OnNarrationStateChanged -= OnNarrationStateChanged;
        }
    }
    
    // 处理旁白状态变化
    private void OnNarrationStateChanged(bool isActive)
    {
        if (!canMoveWhileNarration)
        {
            canMove = !isActive;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 如果在运行时修改了方向锁定设置，更新移动权重
        #if UNITY_EDITOR
        UpdateMovementWeights();
        #endif
        
        // 处理视角旋转（无论是否可移动，视角旋转总是可用的）
        HandleRotation();
        
        // 只有在可移动时才处理移动和拾取
        if (canMove)
        {
            // 处理移动
            HandleMovement();
            // 处理拾取
            HandlePickup();
        }
    }

    void HandleRotation()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * rotationSpeed;

        // 计算旋转角度
        rotationY += mouseX;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f); // 限制垂直视角范围

        // 应用旋转
        transform.rotation = Quaternion.Euler(rotationX, rotationY, 0);
    }

    void HandleMovement()
    {
        // 获取输入
        float horizontal = Input.GetAxis("Horizontal");    // A/D 或 左右箭头
        float vertical = Input.GetAxis("Vertical");        // W/S 或 上下箭头
        
        // 应用方向锁定
        if (enableDirectionLock)
        {
            // 如果选择了只能前进，禁止后退
            if (allowedDirection == AllowedDirection.ForwardOnly && vertical < 0)
            {
                vertical = 0;
            }
            
            // 应用移动权重
            vertical *= forwardMovement;
            horizontal *= rightMovement;
        }

        // 计算移动方向，但只使用水平分量
        Vector3 forward = transform.forward;
        forward.y = 0; // 清除Y分量，确保只在水平面移动
        forward.Normalize();
        
        Vector3 right = transform.right;
        right.y = 0; // 清除Y分量，确保只在水平面移动
        right.Normalize();
        
        Vector3 moveDirection = right * horizontal + forward * vertical;
        
        // 应用移动
        Vector3 newPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        
        // 保持固定高度
        newPosition.y = initialY;
        
        // 更新位置
        transform.position = newPosition;
    }

    void HandlePickup()
    {
        // 发射射线检测可拾取物体
        RaycastHit hit;
        // 先发射一条足够远的射线，判断是否有PickableItem
        float maxCheckDistance = 100f;
        if (Physics.Raycast(transform.position, transform.forward, out hit, maxCheckDistance))
        {
            PickableItem item = hit.collider.GetComponent<PickableItem>();
            if (item != null && !item.isPicked && item.canPickup)
            {
                // 判断距离是否在物体自己的pickUpDistance范围内
                float distance = Vector3.Distance(transform.position, hit.point);
                if (distance <= item.pickUpDistance)
                {
                    // 当物体变化时更新HUD
                    if (currentItem != item)
                    {
                        // 隐藏之前物体的高亮
                        if (currentItem != null)
                        {
                            currentItem.HideHighlight();
                        }
                        
                        currentItem = item;
                        // 显示物体高亮
                        item.ShowHighlight();
                        
                        // 在HUD上显示物品名称
                        if (hudControl != null)
                        {
                            hudControl.ShowItemInfo(item.itemName);
                        }
                    }
                    
                    // 如果按下拾取键
                    if (Input.GetKeyDown(pickUpKey))
                    {
                        PickUpItem(item);
                    }
                }
                else
                {
                    ClearCurrentItem();
                }
            }
            else
            {
                ClearCurrentItem();
            }
        }
        else
        {
            ClearCurrentItem();
        }
    }

    void ClearCurrentItem()
    {
        if (currentItem != null)
        {
            currentItem.HideHighlight();
            
            // 隐藏HUD上的物品名称
            if (hudControl != null)
            {
                hudControl.HideItemInfo();
            }
            
            currentItem = null;
        }
    }

    void PickUpItem(PickableItem item)
    {
        if (item != null && !item.isPicked && item.canPickup)
        {
            item.isPicked = true;
            
            // 隐藏HUD上的物品名称
            if (hudControl != null)
            {
                hudControl.HideItemInfo();
            }
            
            // 让物体消失
            item.gameObject.SetActive(false);
            
            currentItem = null;
        }
    }
}
