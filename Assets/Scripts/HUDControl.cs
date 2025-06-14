using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDControl : MonoBehaviour
{
    [Header("UI引用")]
    public TextMeshProUGUI itemNameText;    // 物品名称文本组件
    public Image itemThumbnailImage;        // 物品缩略图图像组件
    public Image itemPanelBackground;       // 物品面板背景
    public GameObject itemPanel;            // 物品信息面板
    public Image triggerButtonImage;        // 触发按钮图像
    public TextMeshProUGUI itemDescriptionText; // 物品描述文本(可选)

    [Header("触发按钮设置")]
    public Sprite keyboardTriggerSprite;    // 键盘触发键图像
    public Sprite controllerTriggerSprite;  // 控制器触发键图像
    public bool showTriggerOnlyWithItem = true; // 仅在有物品时显示触发键

    [Header("显示设置")]
    public float fadeInSpeed = 5f;          // 淡入速度
    public float fadeOutSpeed = 8f;         // 淡出速度
    public Color defaultTextColor = Color.white; // 默认文本颜色
    public Color defaultPanelColor = new Color(0, 0, 0, 0.6f); // 默认面板颜色
    public Sprite defaultItemThumbnail;     // 默认物品缩略图

    private CanvasGroup itemPanelCanvasGroup; // 用于控制物品面板的透明度
    private CanvasGroup triggerCanvasGroup;   // 用于控制触发键的透明度
    private string currentItemName = "";      // 当前显示的物品名称
    private bool shouldShowItemInfo = false;  // 是否应该显示物品信息
    private bool shouldShowTrigger = false;   // 是否应该显示触发键

    void Start()
    {
        // 初始化物品面板
        if (itemPanel != null)
        {
            // 获取或添加CanvasGroup组件
            itemPanelCanvasGroup = itemPanel.GetComponent<CanvasGroup>();
            if (itemPanelCanvasGroup == null)
            {
                itemPanelCanvasGroup = itemPanel.AddComponent<CanvasGroup>();
            }
            // 初始时隐藏面板
            itemPanelCanvasGroup.alpha = 0f;
        }
        else
        {
            Debug.LogWarning("未设置物品面板(ItemPanel)，请在Inspector中分配");
        }

        // 初始化触发键
        if (triggerButtonImage != null)
        {
            // 设置默认触发键图像
            triggerButtonImage.sprite = keyboardTriggerSprite;
            
            // 获取或添加CanvasGroup组件
            triggerCanvasGroup = triggerButtonImage.GetComponent<CanvasGroup>();
            if (triggerCanvasGroup == null)
            {
                triggerCanvasGroup = triggerButtonImage.gameObject.AddComponent<CanvasGroup>();
            }
            // 初始时隐藏触发键
            triggerCanvasGroup.alpha = 0f;
        }
        else
        {
            Debug.LogWarning("未设置触发按钮图像(TriggerButtonImage)，请在Inspector中分配");
        }

        // 初始化物品面板背景
        if (itemPanelBackground != null)
        {
            // 设置默认面板颜色
            itemPanelBackground.color = defaultPanelColor;
        }

        // 初始化物品名称文本
        if (itemNameText != null)
        {
            // 设置默认文本颜色
            itemNameText.color = defaultTextColor;
        }
        else
        {
            Debug.LogWarning("未设置物品名称文本(ItemNameText)，请在Inspector中分配");
        }

        // 初始化物品缩略图
        if (itemThumbnailImage != null)
        {
            // 设置默认缩略图
            itemThumbnailImage.sprite = defaultItemThumbnail;
            itemThumbnailImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("未设置物品缩略图图像(ItemThumbnailImage)，请在Inspector中分配");
        }

        // 初始化物品描述文本
        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }

        // 隐藏物品信息
        HideItemInfo();
    }

    void Update()
    {
        // 处理物品面板淡入淡出效果
        if (itemPanelCanvasGroup != null)
        {
            if (shouldShowItemInfo)
            {
                // 淡入效果
                itemPanelCanvasGroup.alpha = Mathf.Lerp(itemPanelCanvasGroup.alpha, 1f, fadeInSpeed * Time.deltaTime);
            }
            else
            {
                // 淡出效果
                itemPanelCanvasGroup.alpha = Mathf.Lerp(itemPanelCanvasGroup.alpha, 0f, fadeOutSpeed * Time.deltaTime);
            }
        }

        // 处理触发键淡入淡出效果
        if (triggerCanvasGroup != null)
        {
            if (shouldShowTrigger)
            {
                // 淡入效果
                triggerCanvasGroup.alpha = Mathf.Lerp(triggerCanvasGroup.alpha, 1f, fadeInSpeed * Time.deltaTime);
            }
            else
            {
                // 淡出效果
                triggerCanvasGroup.alpha = Mathf.Lerp(triggerCanvasGroup.alpha, 0f, fadeOutSpeed * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// 显示物品信息
    /// </summary>
    /// <param name="item">可拾取物品</param>
    public void ShowItemInfo(PickableItem item)
    {
        if (item == null)
            return;

        currentItemName = item.itemName;
        shouldShowItemInfo = true;

        // 设置物品名称文本
        if (itemNameText != null)
        {
            itemNameText.text = item.itemName;
            itemNameText.color = item.itemNameColor;
        }

        // 设置物品缩略图
        if (itemThumbnailImage != null)
        {
            if (item.itemThumbnail != null)
            {
                itemThumbnailImage.sprite = item.itemThumbnail;
                itemThumbnailImage.gameObject.SetActive(true);
            }
            else
            {
                itemThumbnailImage.sprite = defaultItemThumbnail;
                itemThumbnailImage.gameObject.SetActive(defaultItemThumbnail != null);
            }
        }

        // 设置物品描述文本
        if (itemDescriptionText != null && !string.IsNullOrEmpty(item.itemDescription))
        {
            itemDescriptionText.text = item.itemDescription;
        }
        else if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }

        // 显示触发键（如果需要）
        if (!showTriggerOnlyWithItem || shouldShowItemInfo)
        {
            ShowTriggerButton();
        }
    }

    /// <summary>
    /// 显示物品信息（保留旧方法以保持兼容性）
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <param name="textColor">文本颜色（可选）</param>
    public void ShowItemInfo(string itemName, Color? textColor = null)
    {
        if (string.IsNullOrEmpty(itemName))
            return;

        currentItemName = itemName;
        shouldShowItemInfo = true;

        // 使用提供的颜色或默认颜色
        if (itemNameText != null)
        {
            itemNameText.text = itemName;
            itemNameText.color = textColor ?? defaultTextColor;
        }

        if (itemThumbnailImage != null)
        {
            itemThumbnailImage.sprite = defaultItemThumbnail;
            itemThumbnailImage.gameObject.SetActive(defaultItemThumbnail != null);
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }

        // 显示触发键（如果需要）
        if (!showTriggerOnlyWithItem || shouldShowItemInfo)
        {
            ShowTriggerButton();
        }
    }

    /// <summary>
    /// 隐藏物品信息
    /// </summary>
    public void HideItemInfo()
    {
        shouldShowItemInfo = false;
        currentItemName = "";

        if (itemNameText != null)
        {
            itemNameText.text = "";
        }

        if (itemThumbnailImage != null)
        {
            itemThumbnailImage.gameObject.SetActive(false);
        }

        if (itemDescriptionText != null)
        {
            itemDescriptionText.text = "";
        }

        // 如果设置为仅在显示物品时显示触发键，则隐藏触发键
        if (showTriggerOnlyWithItem)
        {
            HideTriggerButton();
        }
    }

    /// <summary>
    /// 显示触发按钮
    /// </summary>
    /// <param name="useController">是否使用控制器图标</param>
    public void ShowTriggerButton(bool useController = false)
    {
        if (triggerButtonImage != null)
        {
            triggerButtonImage.sprite = useController ? controllerTriggerSprite : keyboardTriggerSprite;
            shouldShowTrigger = true;
        }
    }

    /// <summary>
    /// 隐藏触发按钮
    /// </summary>
    public void HideTriggerButton()
    {
        shouldShowTrigger = false;
    }

    /// <summary>
    /// 检查是否正在显示指定物品
    /// </summary>
    /// <param name="itemName">物品名称</param>
    /// <returns>是否正在显示该物品</returns>
    public bool IsShowingItem(string itemName)
    {
        return shouldShowItemInfo && currentItemName == itemName;
    }
}

