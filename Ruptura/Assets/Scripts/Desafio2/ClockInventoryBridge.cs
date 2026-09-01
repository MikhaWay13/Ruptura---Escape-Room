using System;
using UnityEngine;
using UnityEngine.UI;

public class ClockInventoryBridge : MonoBehaviour
{
    [Header("Inventário")]
    [SerializeField] private InventoryController inventory;

    [Header("Botões dos Slots")]
    [SerializeField] private Button[] slotButtons;

    public event Action<Item> OnItemSelected;

    private void Awake()
    {
        if (inventory == null)
            inventory = InventoryController.instance;
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
            slotButtons[i].onClick.AddListener(() => SelectItem(slotIndex));
        }
    }

    private void SelectItem(int index)
    {
        if (inventory == null)
            return;

        Item item = inventory.GetItemAtSlot(index);

        if (item == null)
            return;

        OnItemSelected?.Invoke(item);
    }
}