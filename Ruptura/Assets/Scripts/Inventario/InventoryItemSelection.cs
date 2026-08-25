using System;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSelection : MonoBehaviour
{
    [Header("Botões dos Slots")]
    [SerializeField] private Button[] slotButtons;

    public event Action<Item> OnItemSelected;

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
        if (InventoryController.instance == null)
        {
            Debug.LogWarning(
                "InventoryItemSelection: InventoryController não encontrado."
            );

            return;
        }

        Item item =
            InventoryController.instance.GetItemAtSlot(index);

        if (item == null)
            return;

        OnItemSelected?.Invoke(item);
    }
}