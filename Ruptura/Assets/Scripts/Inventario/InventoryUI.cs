using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryUI : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField]
    private InputActionReference inventoryAction;

    [Header("Painel")]
    [SerializeField]
    private GameObject inventoryPanel;

    [Header("Itens")]
    [SerializeField]
    private Transform itemContainer;

    [SerializeField]
    private GameObject itemSlotPrefab;

    [Header("Notificação")]
    [SerializeField]
    private GameObject itemNotification;

    [SerializeField]
    private TMP_Text notificationText;

    [SerializeField, Min(0.1f)]
    private float notificationDuration = 2f;

    private Coroutine notificationCoroutine;

    public bool IsOpen =>
        inventoryPanel != null &&
        inventoryPanel.activeSelf;

    private void Awake()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        if (itemNotification != null)
        {
            itemNotification.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (inventoryAction != null)
        {
            inventoryAction.action.performed += OnInventory;
            inventoryAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (inventoryAction != null)
        {
            inventoryAction.action.performed -= OnInventory;
            inventoryAction.action.Disable();
        }
    }

    private void Start()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.OnItemAdded += HandleItemAdded;
        Inventory.Instance.OnItemRemoved += HandleItemRemoved;

        RefreshInventory();
    }

    private void OnDestroy()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.OnItemAdded -= HandleItemAdded;
        Inventory.Instance.OnItemRemoved -= HandleItemRemoved;
    }

    private void OnInventory(
        InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        ToggleInventory();
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null)
            return;

        bool newState =
            !inventoryPanel.activeSelf;

        inventoryPanel.SetActive(newState);

        if (newState)
        {
            RefreshInventory();
        }
    }

    private void RefreshInventory()
    {
        if (itemContainer == null)
            return;

        foreach (Transform child in itemContainer)
        {
            Destroy(child.gameObject);
        }

        if (Inventory.Instance == null)
            return;

        foreach (
            InventoryItemData item
            in Inventory.Instance.Items)
        {
            CreateItemSlot(item);
        }
    }

    private void CreateItemSlot(
        InventoryItemData item)
    {
        if (itemSlotPrefab == null)
            return;

        GameObject slot =
            Instantiate(
                itemSlotPrefab,
                itemContainer
            );

        InventorySlot inventorySlot =
            slot.GetComponent<InventorySlot>();

        if (inventorySlot != null)
        {
            inventorySlot.Setup(item);
        }
    }

    private void HandleItemAdded(
        InventoryItemData item)
    {
        RefreshInventory();

        ShowNotification(item);
    }

    private void HandleItemRemoved(
        InventoryItemData item)
    {
        RefreshInventory();
    }

    private void ShowNotification(
        InventoryItemData item)
    {
        if (itemNotification == null ||
            notificationText == null)
        {
            return;
        }

        notificationText.text =
            $"Adicionado ao inventário:\n" +
            $"{item.ItemName}";

        itemNotification.SetActive(true);

        if (notificationCoroutine != null)
        {
            StopCoroutine(notificationCoroutine);
        }

        notificationCoroutine =
            StartCoroutine(
                HideNotification()
            );
    }

    private IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(
            notificationDuration
        );

        if (itemNotification != null)
        {
            itemNotification.SetActive(false);
        }
    }
}