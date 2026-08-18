using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIManager : MonoBehaviour
{
    public static UIManager instance; //singleton


//inventário
    public GameObject painelInventory;
    //interação
    public GameObject HandCursor;
    public GameObject BackImage;

    private void Awake()
    {
        instance = this; //singleton

    }

    private void Start()
    {
        if (painelInventory != null)
        {
            painelInventory.SetActive(false);
        }

        SetHandCursor(false);
        SetBackImage(false);
        
    }


    public void SetHandCursor (bool state)
    {
        if (HandCursor != null)
        {
            HandCursor.SetActive(state);
        }
    }


    public void SetBackImage(bool state)
    {
        if (BackImage != null)
        {
            BackImage.SetActive(state);
        }
    }

    


    public void SetInventory(bool invActive)
    {
        if (painelInventory == null)
        {
            return;
        }

        invActive = !invActive;
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


    

}
