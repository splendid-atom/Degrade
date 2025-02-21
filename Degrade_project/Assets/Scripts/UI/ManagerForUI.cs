using UnityEngine;
using TMPro;

public class ManagerForUI : MonoBehaviour
{
    public TMP_FontAsset GeneralFontAsset; // 统一字体资产

    void Start()
    {
        // 确保GeneralFontAsset已经被赋值
        if (GeneralFontAsset != null)
        {
            // 设置所有TextMeshProUGUI组件的字体
            SetAllTextMeshProFonts(GeneralFontAsset);
        }
        else
        {
            Debug.LogError("GeneralFontAsset is not assigned in PlayerController.");
        }
    }

    // 设置所有TextMeshProUGUI组件的字体
    private void SetAllTextMeshProFonts(TMP_FontAsset fontAsset)
    {
        // 查找场景中所有的TextMeshProUGUI组件
        TextMeshProUGUI[] textMeshProUGUIs = FindObjectsOfType<TextMeshProUGUI>();

        // 遍历所有TextMeshProUGUI组件并设置字体
        foreach (TextMeshProUGUI textMeshPro in textMeshProUGUIs)
        {
            textMeshPro.font = fontAsset;
        }
    }
}
