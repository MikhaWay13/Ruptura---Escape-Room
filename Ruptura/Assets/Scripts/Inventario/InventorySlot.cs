using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text itemName;

    private InventoryItemData item;

    public void Setup(InventoryItemData newItem)
    {
        item = newItem;

        if (icon != null)
        {
            icon.sprite = item.Icon;
            icon.enabled = item.Icon != null;
        }

        if (itemName != null)
        {
            itemName.text = item.ItemName;
        }
    }
}