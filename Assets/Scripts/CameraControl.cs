using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("移动设置")]
    public float moveSpeed = 5f;        // 移动速度
    public float rotationSpeed = 2f;    // 视角旋转速度

    private float rotationX = 0f;       // 垂直旋转角度
    private float rotationY = 0f;       // 水平旋转角度

    // Start is called before the first frame update
    void Start()
    {
        // 锁定并隐藏鼠标光标
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // 处理视角旋转
        HandleRotation();
        // 处理移动
        HandleMovement();
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
}
