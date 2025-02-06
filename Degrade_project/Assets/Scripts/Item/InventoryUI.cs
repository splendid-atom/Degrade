using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems; // 添加这个命名空间以使用EventSystem和PointerEventData
public class InventoryUI : MonoBehaviour
{
    public GameObject itemButtonPrefab;  // 物品按钮预设
    public RectTransform inventoryPanel; // 物品栏面板
    public int maxItems = 10; // 最大物品数量
    public int itemsPerRow = 5; // 每行显示物品数量
    private float[] cooldownTimers; // 存储每个物品的冷却时间
    private Button[] itemButtons; // 存储按钮引用

    public GameObject itemsSwitchScroll; // 物品切换的 ScrollRect
    // public GameObject itemList; // 物品列表容器

    private int currentPage = 0;  // 当前显示的物品栏页（0表示第一行，1表示第二行）
     private bool isMouseOverInventoryPanel = false; // 添加一个变量来跟踪鼠标是否在物品栏上

    void Start()
    {
        // 初始化物品栏
        InitializeInventory();

        ScrollRect scrollRect = itemsSwitchScroll.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            // 监听拖动时，更新物品栏行
            scrollRect.onValueChanged.AddListener((Vector2 value) => {
                // 根据 ScrollRect 的垂直位置更新当前页数
                currentPage = Mathf.RoundToInt((1f - value.y) * (Mathf.CeilToInt(maxItems / (float)itemsPerRow) - 1));
                UpdateInventoryDisplay();
            });
        }
    }


    void Update()
    {
        isMouseOverInventoryPanel = RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, Input.mousePosition);
        // 如果鼠标在物品栏上，则监听滚轮滑动来切换物品页
        if (isMouseOverInventoryPanel){
            // 监听滚轮滑动来切换物品页
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // 滚轮向上
            {
                SwitchPage(-1); // 切换到上一行
                // Debug.Log("切换到上一页");
                // Debug.Log("itemButtons.Length:"+itemButtons.Length);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // 滚轮向下
            {
                SwitchPage(1); // 切换到下一行
                // Debug.Log("切换到下一页");
            }
        }

        GetScrollBarValue();
        // 监听快捷键 1-5 对应当前显示的物品
        for (int i = 0; i < Mathf.Min(itemsPerRow, ItemManager.itemManager.inventoryItems.Length - currentPage * itemsPerRow); i++)
        {
            int itemIndex = currentPage * itemsPerRow + i;
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // 监听数字键 1-5
            {
                if (itemIndex < itemButtons.Length && itemButtons[itemIndex] != null) // 确保按钮存在
                {
                    itemButtons[itemIndex].onClick.Invoke(); // 触发按钮点击事件
                }
            }
        }
    }

    void InitializeInventory()
    {
        int itemCount = Mathf.Min(maxItems, ItemManager.itemManager.inventoryItems.Length);

        cooldownTimers = new float[maxItems];
        itemButtons = new Button[maxItems];

        for (int i = 0; i < maxItems; i++)
        {
            GameObject itemButton = Instantiate(itemButtonPrefab, inventoryPanel);
            TextMeshProUGUI itemName = itemButton.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemNumber = itemButton.transform.Find("ItemNumber")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemAmount = itemButton.transform.Find("ItemAmount")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemCooldown = itemButton.transform.Find("ItemCooldown")?.GetComponent<TextMeshProUGUI>();
            Image isObtainedImage = itemButton.transform.Find("IsObtained")?.GetComponent<Image>();
            Image itemIconImage = itemButton.transform.Find("ItemIcon")?.GetComponent<Image>();
            Image cooldownMask = itemButton.transform.Find("CooldownMask")?.GetComponent<Image>(); // 冷却遮罩
            int index = i;  // 关键！创建局部变量
            if (i < itemCount)
            {
                InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[i];
                if (inventoryItem.isObtained)
                {
                    itemName.text = inventoryItem.item.itemName;
                    itemNumber.text = $"{i + 1}";
                    itemAmount.text = $"{inventoryItem.amount}";
                    cooldownMask.fillAmount = 0f;
                }
                else
                {
                    itemName.text = "";
                    itemNumber.text = "";
                    itemAmount.text = "";
                    itemButton.GetComponent<Button>().interactable = false;
                    cooldownMask.fillAmount = 1f;
                }

                if (inventoryItem.item.itemIcon != null)
                {
                    itemIconImage.sprite = inventoryItem.item.itemIcon;
                    itemIconImage.gameObject.SetActive(true);
                }
                else
                {
                    itemIconImage.gameObject.SetActive(false);
                }

                isObtainedImage.gameObject.SetActive(!inventoryItem.isObtained);

                // int index = i;  // 关键！创建局部变量
                inventoryItem.OnAmountChanged += () => UpdateItemAmount(index, itemAmount);

                
                itemCooldown.text = "";
                Button itemButtonComponent = itemButton.GetComponent<Button>();
                itemButtons[i] = itemButtonComponent;
                itemButtonComponent.onClick.AddListener(() => UseItem(index, cooldownMask, itemCooldown));
            }
            else
            {
                itemName.text = "";
                itemNumber.text = "";
                itemAmount.text = "";
                itemIconImage.gameObject.SetActive(false);
                itemButton.GetComponent<Button>().interactable = false;
                itemCooldown.text = "";
                Button itemButtonComponent = itemButton.GetComponent<Button>();
                itemButtons[i] = itemButtonComponent;
                itemButtonComponent.onClick.AddListener(() => UseItem(index, cooldownMask, itemCooldown));
            }

            if (i >= itemsPerRow) // Hide all items except the first row initially
            {
                itemButton.gameObject.SetActive(false);
            }
        }

        // 设置滚动区域的大小
        // RectTransform content = itemList.GetComponent<RectTransform>();
        // int totalRows = Mathf.CeilToInt(ItemManager.itemManager.inventoryItems.Length / (float)itemsPerRow);
        // content.sizeDelta = new Vector2(content.sizeDelta.x, totalRows * 100f);      
    }

    void SwitchPage(int direction)
    {
        int totalPages = Mathf.CeilToInt(maxItems / (float)itemsPerRow);
        currentPage = Mathf.Clamp(currentPage + direction, 0, totalPages - 1);

        // 打印当前页数，查看是否有正确更新
        // Debug.Log($"当前页数: {currentPage}, 总页数: {totalPages}");

        // 检查 currentPage 是否有效
        if (currentPage < 0 || currentPage >= totalPages)
        {
            Debug.LogError($"Invalid page index: {currentPage}");
            return;
        }

        // 计算目标滚动位置
        ScrollRect scrollRect = itemsSwitchScroll.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            float targetNormalizedPosition = 1f - (currentPage / (float)(totalPages - 1));
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetNormalizedPosition);
        }

        UpdateInventoryDisplay();
    }




    void UpdateInventoryDisplay()
    {
        int startIndex = currentPage * itemsPerRow;
        int endIndex = Mathf.Min(startIndex + itemsPerRow, maxItems);

        // 隐藏所有物品
        for (int i = 0; i < itemButtons.Length; i++)
        {
            itemButtons[i].gameObject.SetActive(false);
        }

        // 显示当前页的物品
        for (int i = startIndex; i < endIndex; i++)
        {
            if (i < itemButtons.Length) // 确保索引不超出数组范围
            {
                itemButtons[i].gameObject.SetActive(true);
            }
        }

        // 更新滚动条的值
        UpdateScrollbarValue();
    }

    void UpdateScrollbarValue()
    {
        int totalPages = Mathf.CeilToInt(maxItems / (float)itemsPerRow);
        if (totalPages > 1)
        {
            float targetNormalizedPosition = currentPage / (float)(totalPages - 1);
            Scrollbar scrollbar = itemsSwitchScroll.GetComponent<Scrollbar>();
            if (scrollbar != null)
            {
                scrollbar.value = targetNormalizedPosition;
            }
        }
    }

    void GetScrollBarValue()
    {
        Scrollbar scrollbar = itemsSwitchScroll.GetComponent<Scrollbar>();
        if (scrollbar != null)
        {
            float normalizedPosition = scrollbar.value;
            int totalPages = Mathf.CeilToInt(maxItems / (float)itemsPerRow);
            currentPage = Mathf.FloorToInt(normalizedPosition * (totalPages - 1));
            int direction = (normalizedPosition > 0.5f) ? 1 : -1;
            SwitchPage(direction);
            // Debug.Log($"当前页数: {currentPage}");
        }
    }

    void UseItem(int itemIndex, Image cooldownMask, TextMeshProUGUI itemCooldown)
    {
        InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[itemIndex];
        if (inventoryItem.isObtained)
        {
            if (inventoryItem.amount > 0)
            {
                if (cooldownTimers[itemIndex] > 0)
                {
                    Debug.Log($"物品 {inventoryItem.item.itemName} 正在冷却，剩余时间: {cooldownTimers[itemIndex]:F1} 秒");
                    return;
                }
                // Debug.Log($"使用物品：{inventoryItem.item.itemName}");
                ItemManager.itemManager.UseItem(itemIndex);
                if (inventoryItem.item.cooldownTime > 0)
                {
                    StartCoroutine(CooldownCoroutine(itemIndex, inventoryItem.item.cooldownTime, cooldownMask, itemCooldown));
                }
            }
            else
            {
                Debug.Log($"物品数量不足：{inventoryItem.item.itemName}");
            }
        }
    }

    IEnumerator CooldownCoroutine(int itemIndex, float cooldownTime, Image cooldownMask, TextMeshProUGUI itemCooldown)
    {
        cooldownTimers[itemIndex] = cooldownTime;
        float elapsedTime = 0f;

        while (elapsedTime < cooldownTime)
        {
            cooldownTimers[itemIndex] = cooldownTime - elapsedTime;
            cooldownMask.fillAmount = cooldownTimers[itemIndex] / cooldownTime;
            itemCooldown.text = $"{cooldownTimers[itemIndex]:F1}";
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        cooldownTimers[itemIndex] = 0f;
        cooldownMask.fillAmount = 0f;
        itemCooldown.text = "";
    }

    void UpdateItemAmount(int index, TextMeshProUGUI itemAmount)
    {
        // Debug.Log($"物品index：{index}");
        InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[index];
        itemAmount.text = $"{inventoryItem.amount}";
    }
}
