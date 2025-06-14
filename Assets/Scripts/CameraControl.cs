using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;        // 移动速度
    public float rotationSpeed = 2f;    // 视角旋转速度

    [Header("拾取设置")]
    public KeyCode pickUpKey = KeyCode.F; // 拾取按键改为F键

    [Header("HUD引用")]
    public HUDControl hudControl;       // HUD控制器引用

    private float rotationX = 0f;       // 垂直旋转角度
    private float rotationY = 0f;       // 水平旋转角度
    private PickableItem currentItem;   // 当前可拾取的物品

    // Start is called before the first frame update
    void Start()
    {
        // 锁定并隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // 如果没有指定HUD控制器，尝试自动查找
        if (hudControl == null)
        {
            hudControl = FindObjectOfType<HUDControl>();
            if (hudControl == null)
            {
                Debug.LogWarning("未找到HUDControl组件，物品名称将不会显示在HUD上");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 处理视角旋转
        HandleRotation();
        // 处理移动
        HandleMovement();
        // 处理拾取
        HandlePickup();
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

        // 计算移动方向
        Vector3 moveDirection = transform.right * horizontal + transform.forward * vertical;
        
        // 应用移动
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
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
