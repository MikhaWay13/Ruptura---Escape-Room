using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu]
public class Item : ScriptableObject
{
[Header("Interação")]
    public bool grabbable;
    public AudioClip audioClip;
    public string text;


    [Header("Inventário")]
    public bool ToInventory;
    public string itemName;
    public Sprite itemSprite;


}

