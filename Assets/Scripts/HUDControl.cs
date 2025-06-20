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
    
    [Header("文字描边设置")]
    public Color outlineColor = Color.white; // 描边颜色
    public float outlineWidth = 0.2f;       // 描边宽度

    [Header("进度显示")]
    public TextMeshProUGUI narrationsProgressText;     // 语音进度文本框
    public TextMeshProUGUI itemsProgressText;          // 物品进度文本框

    [Header("年代设置")]
    [SerializeField] private string currentEra = "2024";  // 当前年代
    public TextMeshProUGUI eraDisplayText;              // 年代显示文本组件
    public bool showEraInHUD = true;                    // 是否在HUD中显示年代

    // 语音进度
    private int collectedNarrationsCount = 0;          // 已收集语音数量
    private int totalNarrationsCount = 0;              // 总语音数量
    private string narrationsProgressType = "语音";     // 语音进度类型名称

    // 物品进度
    private int collectedItemsCount = 0;               // 已收集物品数量
    private int totalItemsCount = 0;                   // 总物品数量
    private string itemsProgressType = "物品";          // 物品进度类型名称

    private CanvasGroup itemPanelCanvasGroup; // 用于控制物品面板的透明度
    private CanvasGroup triggerCanvasGroup;   // 用于控制触发键的透明度
    private string currentItemName = "";      // 当前显示的物品名称
    private bool shouldShowItemInfo = false;  // 是否应该显示物品信息
    private bool shouldShowTrigger = false;   // 是否应该显示触发键

    // 添加一个CanvasGroup引用
    [Header("隐藏/显示控制")]
    public CanvasGroup hudCanvasGroup;  // 整个HUD的CanvasGroup

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
            // 设置默认描边
            ApplyTextOutline(itemNameText);
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
            // 设置默认描边
            ApplyTextOutline(itemDescriptionText);
        }

        // 初始化进度文本
        if (narrationsProgressText != null)
        {
            narrationsProgressText.text = "";
            ApplyTextOutline(narrationsProgressText);
        }

        // 初始化物品进度文本
        if (itemsProgressText != null)
        {
            itemsProgressText.text = "";
            ApplyTextOutline(itemsProgressText);
        }

        // 初始化年代显示
        if (eraDisplayText != null && showEraInHUD)
        {
            eraDisplayText.text = currentEra;
            ApplyTextOutline(eraDisplayText);
        }
        else if (eraDisplayText != null)
        {
            eraDisplayText.gameObject.SetActive(false);
        }

        // 初始化进度
        InitProgress();

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
    /// 为TextMeshPro文本应用描边效果
    /// </summary>
    /// <param name="textComponent">要应用描边的文本组件</param>
    private void ApplyTextOutline(TextMeshProUGUI textComponent)
    {
        if (textComponent != null)
        {
            // 启用描边
            textComponent.enableVertexGradient = false;
            textComponent.outlineWidth = outlineWidth;
            textComponent.outlineColor = outlineColor;
            // 确保描边可见
            textComponent.materialForRendering.EnableKeyword("OUTLINE_ON");
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
            // 重新应用描边
            ApplyTextOutline(itemNameText);
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
            // 重新应用描边
            ApplyTextOutline(itemDescriptionText);
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
            // 重新应用描边
            ApplyTextOutline(itemNameText);
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

    /// <summary>
    /// 更新语音进度显示
    /// </summary>
    /// <param name="collected">已收集数量</param>
    /// <param name="total">总数量</param>
    public void UpdateNarrationsProgress(int collected, int total)
    {
        if (narrationsProgressText != null)
        {
            narrationsProgressText.text = $"{collected}/{total}";
        }
    }

    /// <summary>
    /// 更新物品进度显示
    /// </summary>
    /// <param name="collected">已收集数量</param>
    /// <param name="total">总数量</param>
    public void UpdateItemsProgress(int collected, int total)
    {
        if (itemsProgressText != null)
        {
            itemsProgressText.text = $"{collected}/{total}";
        }
    }

    /// <summary>
    /// 初始化进度
    /// </summary>
    private void InitProgress()
    {
        // 重置计数
        collectedNarrationsCount = 0;
        totalNarrationsCount = 0;
        collectedItemsCount = 0;
        totalItemsCount = 0;
        
        // 查找所有旁白触发器
        NarrationTrigger[] narrationTriggers = FindObjectsOfType<NarrationTrigger>();
        foreach (NarrationTrigger trigger in narrationTriggers)
        {
            totalNarrationsCount++;
            if (trigger.hasInteracted)
            {
                collectedNarrationsCount++;
            }
        }
        
        // 查找所有可拾取物品
        PickableItem[] pickableItems = FindObjectsOfType<PickableItem>();
        foreach (PickableItem item in pickableItems)
        {
            totalItemsCount++;
            if (item.isPicked)
            {
                collectedItemsCount++;
            }
        }
        
        // 更新语音进度显示
        UpdateNarrationsProgress(collectedNarrationsCount, totalNarrationsCount);
        // 更新物品进度显示
        UpdateItemsProgress(collectedItemsCount, totalItemsCount);
    }

    /// <summary>
    /// 增加语音进度
    /// </summary>
    public void IncreaseNarrationsProgress()
    {
        collectedNarrationsCount++;
        if (collectedNarrationsCount > totalNarrationsCount)
        {
            collectedNarrationsCount = totalNarrationsCount;
        }
        UpdateNarrationsProgress(collectedNarrationsCount, totalNarrationsCount);
    }

    /// <summary>
    /// 增加物品进度
    /// </summary>
    public void IncreaseItemsProgress()
    {
        collectedItemsCount++;
        if (collectedItemsCount > totalItemsCount)
        {
            collectedItemsCount = totalItemsCount;
        }
        UpdateItemsProgress(collectedItemsCount, totalItemsCount);
    }

    /// <summary>
    /// 设置进度类型名称
    /// </summary>
    /// <param name="type">类型名称</param>
    public void SetNarrationsProgressType(string type)
    {
        narrationsProgressType = type;
        UpdateNarrationsProgress(collectedNarrationsCount, totalNarrationsCount);
    }

    public void SetItemsProgressType(string type)
    {
        itemsProgressType = type;
        UpdateItemsProgress(collectedItemsCount, totalItemsCount);
    }

    /// <summary>
    /// 设置当前年代
    /// </summary>
    /// <param name="era">年代字符串</param>
    public void SetCurrentEra(string era)
    {
        currentEra = era;
        UpdateEraDisplay();
    }

    /// <summary>
    /// 获取当前年代
    /// </summary>
    /// <returns>当前年代字符串</returns>
    public string GetCurrentEra()
    {
        return currentEra;
    }

    /// <summary>
    /// 更新年代显示
    /// </summary>
    private void UpdateEraDisplay()
    {
        if (eraDisplayText != null && showEraInHUD)
        {
            eraDisplayText.text = currentEra;
            eraDisplayText.gameObject.SetActive(true);
        }
        else if (eraDisplayText != null)
        {
            eraDisplayText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 切换年代显示
    /// </summary>
    /// <param name="show">是否显示年代</param>
    public void ToggleEraDisplay(bool show)
    {
        showEraInHUD = show;
        UpdateEraDisplay();
    }

    /// <summary>
    /// 隐藏所有HUD元素
    /// </summary>
    public void HideAllHUDElements()
    {
        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 0f;
            hudCanvasGroup.interactable = false;
            hudCanvasGroup.blocksRaycasts = false;
        }
        else
        {
            // 如果没有CanvasGroup，则尝试隐藏各个元素
            if (itemPanel != null)
                itemPanel.SetActive(false);
                
            if (triggerButtonImage != null)
                triggerButtonImage.gameObject.SetActive(false);
                
            // 隐藏进度文本
            if (narrationsProgressText != null)
                narrationsProgressText.gameObject.SetActive(false);
                
            if (itemsProgressText != null)
                itemsProgressText.gameObject.SetActive(false);

            // 隐藏年代显示
            if (eraDisplayText != null)
                eraDisplayText.gameObject.SetActive(false);
        }
        
        Debug.Log("HUD元素已隐藏");
    }

    /// <summary>
    /// 显示所有HUD元素
    /// </summary>
    public void ShowAllHUDElements()
    {
        if (hudCanvasGroup != null)
        {
            hudCanvasGroup.alpha = 1f;
            hudCanvasGroup.interactable = true;
            hudCanvasGroup.blocksRaycasts = true;
        }
        else
        {
            // 如果没有CanvasGroup，则尝试显示各个元素
            if (itemPanel != null)
                itemPanel.SetActive(true);
                
            // 进度文本始终显示
            if (narrationsProgressText != null)
                narrationsProgressText.gameObject.SetActive(true);
                
            if (itemsProgressText != null)
                itemsProgressText.gameObject.SetActive(true);
                
            // 触发按钮根据设置决定是否显示
            if (triggerButtonImage != null && !showTriggerOnlyWithItem)
                triggerButtonImage.gameObject.SetActive(true);

            // 年代显示根据设置决定是否显示
            if (eraDisplayText != null && showEraInHUD)
                eraDisplayText.gameObject.SetActive(true);
        }
        
        Debug.Log("HUD元素已恢复显示");
    }
}

