using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject inventoryPanel;

    [Header("Lista de itens")]
    [SerializeField] private Transform itemContainer;

    [SerializeField] private GameObject itemSlotPrefab;

    [Header("Mensagem")]
    [SerializeField] private GameObject itemMessage;
    [SerializeField] private TMP_Text itemMessageText;

    [Header("Configuração")]
    [SerializeField] private float messageDuration = 2f;

    private Coroutine messageCoroutine;

    private void Awake()
    {
        inventoryPanel.SetActive(false);
        itemMessage.SetActive(false);
    }

    private void Start()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.OnItemAdded += OnItemAdded;

        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.OnItemAdded -= OnItemAdded;
    }

    public void OnInventory(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        ToggleInventory();
    }

    private void ToggleInventory()
    {
        bool newState = !inventoryPanel.activeSelf;

        inventoryPanel.SetActive(newState);

        if (newState)
        {
            RefreshInventory();
        }
    }

    private void RefreshInventory()
    {
        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        if (Inventory.Instance == null)
            return;

        foreach (InventoryItemData item in Inventory.Instance.Items)
        {
            CreateItemSlot(item);
        }
    }

    private void CreateItemSlot(InventoryItemData item)
    {
        if (itemSlotPrefab == null)
            return;

        GameObject slot =
            Instantiate(itemSlotPrefab, itemContainer);

        InventorySlot inventorySlot =
            slot.GetComponent<InventorySlot>();

        if (inventorySlot != null)
        {
            inventorySlot.Setup(item);
        }
    }

    private void OnItemAdded(InventoryItemData item)
    {
        RefreshInventory();
        ShowItemMessage(item);
    }

    private void ShowItemMessage(InventoryItemData item)
    {
        if (itemMessage == null ||
            itemMessageText == null)
            return;

        itemMessageText.text =
            $"Adicionado ao inventário:\n{item.ItemName}";

        itemMessage.SetActive(true);

        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine =
            StartCoroutine(HideMessageAfterTime());
    }

    private System.Collections.IEnumerator HideMessageAfterTime()
    {
        yield return new WaitForSeconds(messageDuration);

        itemMessage.SetActive(false);
    }
}