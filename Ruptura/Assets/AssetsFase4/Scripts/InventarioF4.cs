using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class InventarioF4 : MonoBehaviour
{
    public static InventarioF4 instance;

    public ItensF4[] slots;
    public Image[] slotImage;
    public int[] slotAmount;

    private void Awake()
    {
        instance = this;
    }


    public bool AddItensF4(ItensF4 newItem)
    {


        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null || slots[i].itemName == newItem.itemName)
            {
                slots[i] = newItem;
                slotAmount[i]++;
                slotImage[i].sprite = slots[i].itemSprite;

                return true;
            }
        }
        return false;


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
