using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemButtonPrefab;  // 物品按钮预设
    public RectTransform inventoryPanel; // 物品栏面板
    public int maxItems = 10; // 最大物品数量
    public int itemsPerRow = 5; // 每行显示物品数量
    private float[] cooldownTimers; // 存储每个物品的冷却时间
    private Button[] itemButtons; // 存储按钮引用

    public GameObject itemsSwitchScroll; // 物品切换的 ScrollRect

    private int currentPage = 0;  // 当前显示的物品栏页（0表示第一行，1表示第二行）
    private bool isMouseOverInventoryPanel = false; // 跟踪鼠标是否在物品栏上

    void Start()
    {
        // 初始化物品栏
        InitializeInventory();

        ScrollRect scrollRect = itemsSwitchScroll.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.onValueChanged.AddListener((Vector2 value) =>
            {
                currentPage = Mathf.RoundToInt((1f - value.y) * (Mathf.CeilToInt(maxItems / (float)itemsPerRow) - 1));
                UpdateInventoryDisplay();
            });
        }

        // 订阅 ItemManager 的物品变更事件
        ItemManager.itemManager.OnItemAdded += (index) => UpdateItemDisplay(index);
    }

    void Update()
    {
        isMouseOverInventoryPanel = RectTransformUtility.RectangleContainsScreenPoint(inventoryPanel, Input.mousePosition);
        if (isMouseOverInventoryPanel)
        {
            if (Input.GetAxis("Mouse ScrollWheel") > 0f) // 滚轮向上
            {
                SwitchPage(-1);
            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0f) // 滚轮向下
            {
                SwitchPage(1);
            }
        }

        GetScrollBarValue();

        for (int i = 0; i < Mathf.Min(itemsPerRow, ItemManager.itemManager.inventoryItems.Count - currentPage * itemsPerRow); i++)
        {
            int itemIndex = currentPage * itemsPerRow + i;
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                if (itemIndex < itemButtons.Length && itemButtons[itemIndex] != null)
                {
                    itemButtons[itemIndex].onClick.Invoke();
                }
            }
        }
    }

    void InitializeInventory() {
        int itemCount = Mathf.Min(maxItems, ItemManager.itemManager.inventoryItems.Count);
        cooldownTimers = new float[maxItems];
        itemButtons = new Button[maxItems];

        for (int i = 0; i < maxItems; i++) {
            GameObject itemButton = Instantiate(itemButtonPrefab, inventoryPanel);
            itemButtons[i] = itemButton.GetComponent<Button>();
            TextMeshProUGUI itemName = itemButton.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemNumber = itemButton.transform.Find("ItemNumber")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemAmount = itemButton.transform.Find("ItemAmount")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemCooldown = itemButton.transform.Find("ItemCooldown")?.GetComponent<TextMeshProUGUI>();
            Image isObtainedImage = itemButton.transform.Find("IsObtained")?.GetComponent<Image>();
            Image itemIconImage = itemButton.transform.Find("ItemIcon")?.GetComponent<Image>();
            Image cooldownMask = itemButton.transform.Find("CooldownMask")?.GetComponent<Image>();
            int index = i;

            // 默认设置为未取得状态
            itemName.text = "";
            itemNumber.text = "";
            itemAmount.text = "";
            itemIconImage.gameObject.SetActive(false);
            itemButtons[i].interactable = false;
            cooldownMask.fillAmount = 1f;
            isObtainedImage.gameObject.SetActive(true);
            itemCooldown.text = "";

            if (i < itemCount) {
                InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[i];
                if (inventoryItem.isObtained) {
                    itemName.text = inventoryItem.item.itemName;
                    itemNumber.text = $"{i + 1}";
                    itemAmount.text = $"{inventoryItem.amount}";
                    cooldownMask.fillAmount = 0f;
                    itemButtons[i].interactable = true;
                    if (inventoryItem.item.itemIcon != null) {
                        itemIconImage.sprite = inventoryItem.item.itemIcon;
                        itemIconImage.gameObject.SetActive(true);
                    }
                    isObtainedImage.gameObject.SetActive(false);
                }
                inventoryItem.OnAmountChanged += () => UpdateItemAmount(index, itemAmount);
            }

            itemButtons[i].onClick.AddListener(() => UseItem(index, cooldownMask, itemCooldown));
            if (i >= itemsPerRow) {
                itemButton.gameObject.SetActive(false);
            }
        }
        UpdateInventoryDisplay();
    }
    void UpdateItemDisplay(int itemIndex) {
        if (itemIndex >= maxItems || itemIndex >= itemButtons.Length || itemButtons[itemIndex] == null) {
            Debug.LogWarning("Invalid item index: " + itemIndex);
            return;
        }

        InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[itemIndex];
        GameObject itemButton = itemButtons[itemIndex].gameObject;

        TextMeshProUGUI itemName = itemButton.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemNumber = itemButton.transform.Find("ItemNumber")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemAmount = itemButton.transform.Find("ItemAmount")?.GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI itemCooldown = itemButton.transform.Find("ItemCooldown")?.GetComponent<TextMeshProUGUI>();
        Image isObtainedImage = itemButton.transform.Find("IsObtained")?.GetComponent<Image>();
        Image itemIconImage = itemButton.transform.Find("ItemIcon")?.GetComponent<Image>();
        Image cooldownMask = itemButton.transform.Find("CooldownMask")?.GetComponent<Image>();
        Button itemButtonComponent = itemButton.GetComponent<Button>();

        Debug.Log($"Updating item {itemIndex}: {inventoryItem.item.itemName}, isObtained: {inventoryItem.isObtained}");

        if (inventoryItem.isObtained) {
            itemName.text = inventoryItem.item.itemName;
            itemNumber.text = $"{itemIndex + 1}";
            itemAmount.text = $"{inventoryItem.amount}";
            cooldownMask.fillAmount = 0f;
            itemButtonComponent.interactable = true;
            if (inventoryItem.item.itemIcon != null) {
                itemIconImage.sprite = inventoryItem.item.itemIcon;
                itemIconImage.gameObject.SetActive(true);
            } else {
                itemIconImage.gameObject.SetActive(false);
            }
            isObtainedImage.gameObject.SetActive(false);
        } else {
            itemName.text = "";
            itemNumber.text = "";
            itemAmount.text = "";
            cooldownMask.fillAmount = 1f;
            itemButtonComponent.interactable = false;
            itemIconImage.gameObject.SetActive(false);
            isObtainedImage.gameObject.SetActive(true);
        }
        itemCooldown.text = cooldownTimers[itemIndex] > 0 ? $"{cooldownTimers[itemIndex]:F1}" : "";

        int itemPage = itemIndex / itemsPerRow;
        if (itemPage != currentPage) {
            currentPage = itemPage;
            ScrollRect scrollRect = itemsSwitchScroll.GetComponent<ScrollRect>();
            if (scrollRect != null) {
                int totalPages = Mathf.CeilToInt(maxItems / (float)itemsPerRow);
                float targetNormalizedPosition = totalPages > 1 ? 1f - (currentPage / (float)(totalPages - 1)) : 1f;
                scrollRect.normalizedPosition = new Vector2(0f, targetNormalizedPosition);
            }
        }
        UpdateInventoryDisplay();
    }

    void SwitchPage(int direction)
    {
        int totalPages = Mathf.CeilToInt(maxItems / (float)itemsPerRow);
        currentPage = Mathf.Clamp(currentPage + direction, 0, totalPages - 1);

        ScrollRect scrollRect = itemsSwitchScroll.GetComponent<ScrollRect>();
        if (scrollRect != null)
        {
            float targetNormalizedPosition = totalPages > 1 ? 1f - (currentPage / (float)(totalPages - 1)) : 1f;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(targetNormalizedPosition);
        }

        UpdateInventoryDisplay();
    }

    void UpdateInventoryDisplay()
    {
        int startIndex = currentPage * itemsPerRow;
        int endIndex = Mathf.Min(startIndex + itemsPerRow, maxItems);

        for (int i = 0; i < maxItems; i++)
        {
            if (i >= startIndex && i < endIndex && i < ItemManager.itemManager.inventoryItems.Count)
            {
                itemButtons[i].gameObject.SetActive(true);
                InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[i];
                TextMeshProUGUI itemName = itemButtons[i].transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI itemNumber = itemButtons[i].transform.Find("ItemNumber")?.GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI itemAmount = itemButtons[i].transform.Find("ItemAmount")?.GetComponent<TextMeshProUGUI>();
                Image itemIconImage = itemButtons[i].transform.Find("ItemIcon")?.GetComponent<Image>();
                Image cooldownMask = itemButtons[i].transform.Find("CooldownMask")?.GetComponent<Image>();
                Image isObtainedImage = itemButtons[i].transform.Find("IsObtained")?.GetComponent<Image>();
                Button itemButtonComponent = itemButtons[i].GetComponent<Button>();

                if (inventoryItem.isObtained)
                {
                    itemName.text = inventoryItem.item.itemName;
                    itemNumber.text = $"{i + 1}";
                    itemAmount.text = $"{inventoryItem.amount}";
                    cooldownMask.fillAmount = cooldownTimers[i] > 0 ? cooldownTimers[i] / inventoryItem.item.cooldownTime : 0f;
                    if (inventoryItem.item.itemIcon != null)
                    {
                        itemIconImage.sprite = inventoryItem.item.itemIcon;
                        itemIconImage.gameObject.SetActive(true);
                    }
                    isObtainedImage.gameObject.SetActive(false);

                    // 特殊处理“时空碎片”的按钮可交互性
                    if (inventoryItem.item.itemName == "时空碎片")
                    {
                        itemButtonComponent.interactable = inventoryItem.amount >= 3;
                    }
                    else
                    {
                        itemButtonComponent.interactable = true;
                    }
                }
                else
                {
                    itemName.text = "";
                    itemNumber.text = "";
                    itemAmount.text = "";
                    itemButtonComponent.interactable = false;
                    cooldownMask.fillAmount = 1f;
                    itemIconImage.gameObject.SetActive(false);
                    isObtainedImage.gameObject.SetActive(true);
                }
            }
            else
            {
                itemButtons[i].gameObject.SetActive(false);
            }
        }
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
            UpdateInventoryDisplay();
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
        InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[index];
        itemAmount.text = $"{inventoryItem.amount}";
    }
}