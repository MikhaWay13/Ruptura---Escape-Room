using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Object", menuName = "Inventory Objects/Create New")]
public class InventoryController : ScriptableObject
{
    
    public string itemName;

    public Sprite itemSprite;
}
