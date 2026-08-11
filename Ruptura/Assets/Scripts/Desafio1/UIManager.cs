using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class UIManager : MonoBehaviour
{


    public static UIManager instance;

    public GameObject HandCursor;
    public GameObject BackImage;

    private void Awake()
    {
        instance=this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetHandCursor (bool state)
    {
        HandCursor.SetActive(state);
    }


     public void SetBackImage (bool state)
    {
        BackImage.SetActive(state);
    }
}
