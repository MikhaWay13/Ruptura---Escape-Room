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
        painelInventory.SetActive(false);
        
    }


    public void SetHandCursor (bool state)
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


    

}
