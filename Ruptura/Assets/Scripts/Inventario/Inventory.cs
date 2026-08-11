using System;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    [Header("Itens")]
    [SerializeField] private List<InventoryItemData> items = new();

    public IReadOnlyList<InventoryItemData> Items => items;

    public event Action<InventoryItemData> OnItemAdded;
    public event Action<InventoryItemData> OnItemRemoved;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool AddItem(InventoryItemData item)
    {
        if (item == null)
            return false;

        if (items.Contains(item))
            return false;

        items.Add(item);

        OnItemAdded?.Invoke(item);

        return true;
    }

    public bool RemoveItem(InventoryItemData item)
    {
        if (item == null)
            return false;

        if (!items.Remove(item))
            return false;

        OnItemRemoved?.Invoke(item);

        return true;
    }

    public bool HasItem(InventoryItemData item)
    {
        return item != null && items.Contains(item);
    }
}