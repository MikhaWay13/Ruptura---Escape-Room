using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // troque por Text (já usando UnityEngine.UI) se não usar TextMeshPro

public class UIF4 : MonoBehaviour
{
    public static UIF4 instance; //singleton


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

    private void Awake()
    {
        instance = this; //singleton
    }

    private void Start()
    {
        painelInventory.SetActive(false);
        itemUIPanel.SetActive(false);
    }

    public void SetHandCursor(bool state)
    {
        HandCursor.SetActive(state);
    }

    public void SetBackImage(bool state)
    {
        BackImage.SetActive(state);
    }


    public void SetInventory(bool invActive)
    {
        invActive = !invActive;
        painelInventory.SetActive(invActive);
        if (invActive)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void OpenItemUI(ItensF4 item)
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
