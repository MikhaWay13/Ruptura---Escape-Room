using UnityEngine;

[CreateAssetMenu(
    fileName = "NewInventoryItem",
    menuName = "Inventory/Item"
)]
public class InventoryItemData : ScriptableObject
{
    [Header("Informações")]
    [SerializeField] private string itemName;

    [SerializeField] private Sprite icon;

    public string ItemName => itemName;

    public Sprite Icon => icon;
}