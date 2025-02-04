using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemButtonPrefab;  // 物品按钮预设
    public RectTransform inventoryPanel; // 物品栏面板
    public int maxItems = 10; // 最大物品数量
    public int itemsPerRow = 5; // 每行显示物品数量
    private float[] cooldownTimers; // 存储每个物品的冷却时间
    private Button[] itemButtons; // 存储按钮引用

    private void Start()
    {
        // 初始化物品栏
        InitializeInventory();
    }
    void Update()
    {
        for (int i = 0; i < Mathf.Min(maxItems, ItemManager.itemManager.inventoryItems.Length); i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i)) // 监听数字键 1-9 和 0
            {
                if (i < itemButtons.Length && itemButtons[i] != null) // 确保按钮存在
                {
                    itemButtons[i].onClick.Invoke(); // 触发按钮点击事件
                }
            }
        }
    }
    void InitializeInventory()
    {
        int itemCount = Mathf.Min(maxItems, ItemManager.itemManager.inventoryItems.Length);
        int totalItemsToDisplay = Mathf.CeilToInt(itemCount / (float)itemsPerRow) * itemsPerRow;

        // 初始化冷却时间数组
        cooldownTimers = new float[itemCount];
        itemButtons = new Button[itemCount]; // 初始化数组

        for (int i = 0; i < totalItemsToDisplay; i++)
        {
            GameObject itemButton = Instantiate(itemButtonPrefab, inventoryPanel);
            TextMeshProUGUI itemName = itemButton.transform.Find("ItemName")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemNumber = itemButton.transform.Find("ItemNumber")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemAmount = itemButton.transform.Find("ItemAmount")?.GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI itemCooldown = itemButton.transform.Find("ItemCooldown")?.GetComponent<TextMeshProUGUI>();
            Image isObtainedImage = itemButton.transform.Find("IsObtained")?.GetComponent<Image>();
            Image itemIconImage = itemButton.transform.Find("ItemIcon")?.GetComponent<Image>();
            Image cooldownMask = itemButton.transform.Find("CooldownMask")?.GetComponent<Image>(); // 冷却遮罩

            if (i < itemCount)
            {
                InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[i];

                itemName.text = inventoryItem.item.itemName;
                itemNumber.text = $"{i + 1}";
                itemAmount.text = $"{inventoryItem.amount}";
                
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

                // 初始化冷却遮罩
                cooldownMask.fillAmount = 0f;
                itemCooldown.text="";
                Button itemButtonComponent = itemButton.GetComponent<Button>();
                itemButtons[i] = itemButtonComponent; // 存储按钮
                int index = i;
                itemButtonComponent.onClick.AddListener(() => UseItem(index, cooldownMask,itemCooldown));

                Debug.Log($"Item {i} added to inventory.");
            }
            else
            {
                itemName.text = "";
                itemNumber.text = "";
                itemAmount.text = "";
                itemIconImage.gameObject.SetActive(false);
                itemButton.GetComponent<Button>().interactable = false;

                Debug.Log($"Placeholder {i - itemCount} added.");
            }
        }
    }

    void UseItem(int itemIndex, Image cooldownMask,TextMeshProUGUI itemCooldown)
    {
        InventoryItem inventoryItem = ItemManager.itemManager.inventoryItems[itemIndex];
        if (cooldownTimers[itemIndex] > 0)
        {
            Debug.Log($"物品 {inventoryItem.item.itemName} 正在冷却，剩余时间: {cooldownTimers[itemIndex]:F1} 秒");
            return;
        }

        Debug.Log($"使用物品：{inventoryItem.item.itemName}");
        ItemManager.itemManager.UseItem(itemIndex);

        if (inventoryItem.item.cooldownTime > 0)
        {
            StartCoroutine(CooldownCoroutine(itemIndex, inventoryItem.item.cooldownTime, cooldownMask,itemCooldown));
        }
    }

    IEnumerator CooldownCoroutine(int itemIndex, float cooldownTime, Image cooldownMask,TextMeshProUGUI itemCooldown)
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
}
