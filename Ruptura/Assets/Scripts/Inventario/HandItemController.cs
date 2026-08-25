using UnityEngine;

public class HandItemController : MonoBehaviour
{
    [Header("Ponto da mão")]
    [SerializeField] private Transform handPoint;

    [Header("Itens equipáveis")]
    [SerializeField] private HandItemEntry[] items;

    private GameObject currentHandObject;
    private Item currentItem;

    public Item CurrentItem => currentItem;

    private void Awake()
    {
        InventoryItemSelection selection =
            FindFirstObjectByType<InventoryItemSelection>();

        if (selection != null)
        {
            selection.OnItemSelected += EquipItem;
        }
    }

    private void OnDestroy()
    {
        InventoryItemSelection selection =
            FindFirstObjectByType<InventoryItemSelection>();

        if (selection != null)
        {
            selection.OnItemSelected -= EquipItem;
        }
    }

    private void EquipItem(Item item)
    {
        if (item == null)
            return;

        HandItemEntry entry =
            FindEntry(item);

        if (entry == null)
        {
            return;
        }

        if (!entry.equipInHand)
        {
            return;
        }

        ClearHand();

        if (entry.prefab == null ||
            handPoint == null)
            return;

        currentHandObject =
            Instantiate(
                entry.prefab,
                handPoint
            );

        currentHandObject.transform.localPosition =
            entry.localPosition;

        currentHandObject.transform.localRotation =
            Quaternion.Euler(
                entry.localRotation
            );

        currentHandObject.transform.localScale =
            entry.localScale;

        currentItem = item;
    }

    private HandItemEntry FindEntry(Item item)
    {
        if (items == null)
            return null;

        foreach (HandItemEntry entry in items)
        {
            if (entry != null &&
                entry.item == item)
            {
                return entry;
            }
        }

        return null;
    }

    public void ClearHand()
    {
        if (currentHandObject != null)
        {
            Destroy(currentHandObject);
        }

        currentHandObject = null;
        currentItem = null;
    }
}

[System.Serializable]
public class HandItemEntry
{
    public Item item;

    public bool equipInHand = true;

    public GameObject prefab;

    public Vector3 localPosition;

    public Vector3 localRotation;

    public Vector3 localScale = Vector3.one;
}