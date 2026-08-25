using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSelection : MonoBehaviour
{
    [Header("Inventário")]
    [SerializeField] private InventoryController inventory;

    [Header("Botões dos Slots")]
    [SerializeField] private Button[] slotButtons;

    public event Action<Item> OnItemSelected;

    private void Awake()
    {
        if (inventory == null)
        {
            inventory =
                InventoryController.instance;
        }
    }

    private void Start()
    {
        ConnectButtons();
    }

    private void ConnectButtons()
    {
        if (slotButtons == null)
            return;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            if (slotButtons[i] == null)
                continue;

            int slotIndex = i;

            slotButtons[i].onClick.AddListener(
                () => SelectSlot(slotIndex)
            );
        }
    }

    private void SelectSlot(int index)
    {
        if (inventory == null)
            return;

        if (inventory.slots == null)
            return;

        if (index < 0 ||
            index >= inventory.slots.Length)
            return;

        Item selectedItem =
            inventory.slots[index];

        if (selectedItem == null)
            return;

        if (inventory.slotAmount[index] <= 0)
            return;

        OnItemSelected?.Invoke(
            selectedItem
        );
    }
}