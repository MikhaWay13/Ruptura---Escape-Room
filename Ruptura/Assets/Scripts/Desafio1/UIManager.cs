using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class UIManager : MonoBehaviour
{
    public static UIManager instance; //singleton




    //inventário
    public GameObject painelInventory;
    //interação
    public GameObject HandCursor;
    public GameObject BackImage;


    //leitura de item
    public GameObject itemUIPanel;
    public TMP_Text itemUITitleText;
    public TMP_Text itemUIBodyText;
    public Image itemUIImage;
    public GameObject pressE;
    public GameObject interact;   // Arraste o objeto "Press(E) Interagir"
    public GameObject painelAviso;   // Arraste o objeto "Aviso"
    public bool IsInventoryOpen => painelInventory != null && painelInventory.activeSelf;
    private void Awake()
    {
        instance = this; //singleton
    }


    private void Start()
    {
        painelInventory.SetActive(false);
        itemUIPanel.SetActive(false);
        pressE.SetActive(false);
    }


    public void SetHandCursor(bool state)
    {
        HandCursor.SetActive(state);
    }


    public void SetBackImage(bool state)
    {
        BackImage.SetActive(state);
    }


    public void SetPressE(bool state)
    {
        pressE.SetActive(state);
    }


    public void SetPressEInteracao(bool state)
    {
        if (interact != null)
        {
            interact.SetActive(state);
        }
    }


    public void SetAvisoEquipar(bool state)
    {
        if (painelAviso != null)
        {
            painelAviso.SetActive(state);
        }
    }




    public void SetInventory(bool invActive)
    {


        painelInventory.SetActive(invActive);
        if (invActive)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }


    public void OpenItemUI(Item item)
    {
        itemUITitleText.text = item.itemName;
        itemUIBodyText.text = item.uiText;


        bool hasImage = item.uiImage != null;
        itemUIImage.gameObject.SetActive(hasImage);
        if (hasImage) itemUIImage.sprite = item.uiImage;


        itemUIPanel.SetActive(true);
    }


    public void CloseItemUI()
    {
        itemUIPanel.SetActive(false);
    }
}
