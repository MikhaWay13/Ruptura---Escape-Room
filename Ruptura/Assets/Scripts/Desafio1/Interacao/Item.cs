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

    [Header("Leitura (UI)")]
    public bool hasReadableUI;
    [TextArea(3, 10)]
    public string uiText;
    public Sprite uiImage; // opcional � deixe null se o item n�o tiver imagem

}

