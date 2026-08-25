using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    public Item[] slots;
    public Image[] slotImage;
    public int[] slotAmount;

    private void Awake()
    {
        instance = this;
    }


    public bool AddItem(Item newItem)
    {
        if (newItem == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || slots[i].itemName == newItem.itemName)
            {
                slots[i] = newItem;
                slotAmount[i]++;

                if (slotImage[i] != null)
                {
                    slotImage[i].sprite = newItem.itemSprite;
                    slotImage[i].enabled = true;
                }

                return true;
            }
        }

        return false;
    }

    public bool HasItem(Item item)
    {
        if (item == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null &&
                slots[i].itemName == item.itemName &&
                slotAmount[i] > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(Item item)
    {
        if (item == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null ||
                slots[i].itemName != item.itemName ||
                slotAmount[i] <= 0)
            {
                continue;
            }

            slotAmount[i]--;

            if (slotAmount[i] == 0)
            {
                slots[i] = null;

                if (slotImage[i] != null)
                {
                    slotImage[i].sprite = null;
                    slotImage[i].enabled = false;
                }
            }

            return true;
        }

        return false;
    }
    public Item GetItemAtSlot(int index)
{
    if (index < 0 || index >= slots.Length)
        return null;

    if (slotAmount[index] <= 0)
        return null;

    return slots[index];
}

}









//   public void RayInventory(RaycastHit hit,Interactables inventory)
//     {

//             if (Input.GetKeyDown(KeyCode.E) && inventory.item.ToInventory)
//             {
//                 for (int i = 0; i < slots.Length; i++)
//                 {
//                     if (slots[i] == null || slots[i].ItemName == inventory.item.ItemName)
//                     {
//                         slots[i] = inventory.item;
//                         slotAmount[i]++;
//                         slotImage[i].sprite = slots[i].itemSprite;

//                         Destroy(hit.transform.gameObject);
//                         break;
//                     }
//                 }
//             }
    
//     }

