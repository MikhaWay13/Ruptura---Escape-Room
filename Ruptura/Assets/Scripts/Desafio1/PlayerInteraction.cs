using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerInteraction : MonoBehaviour
{
    public float rayDistance = 2f;
    public float RotateSpeed = 200f;

    public Transform objectViewer;

    public UnityEvent OnView;

    private Camera myCam;

    private Interactables currentInteractable;

    private Vector3 originPosition;
    private Quaternion originRotation;
    private bool isViewing;

    void Start()
    {
        myCam = Camera.main;
    }

    void Update()
    {
        CheckInteractables();
    }

    void CheckInteractables()
    {

        if(isViewing){
            if(currentInteractable.ite.grabbable && Input.GetMouseButton(0)){
                RotateObject();
            }
            return;
        }
        RaycastHit hit;


        Vector3 rayOrigin = myCam.ViewportToWorldPoint(
            new Vector3(0.5f, 0.5f, 0.5f)
        );

        if (Physics.Raycast(rayOrigin, myCam.transform.forward, out hit, rayDistance))
        {
            Interactables interactable = hit.collider.GetComponent<Interactables>();

            if (interactable != null)
            {
                UIManager.instance.SetHandCursor(true);
                if(Input.GetMouseButtonDown(0)) 
                {
                    OnView.Invoke();

                    currentInteractable = interactable;

                    isViewing= true;

                    if(currentInteractable.item.grabbable)
                    {
                        originPosition=currentInteractable.transform.position;
                        originRotation=currentInteractable.transform.rotation;
                        StartCoroutine(MovingObject(currentInteractable, objectViewer.position));
                    }
                }                                         //mudar pro Input Manager
            
            }
            else
            {
                UIManager.instance.SetHandCursor(false);
            }
        }
        else
        {
            UIManager.instance.SetHandCursor(false);
        }

    }



    IEnumerator MovingObject(Interactables obj, Vector3 position)
    {
        float timer=0;
        while(timer<1)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, position, Time.deltaTime*5); 
            timer+= Time.deltaTime;
            yield return null;
        }

        obj.transform.position=position;
    }


    void RotateObject(){
        float x = Input.GetAxis("Mouse X");
        float y =Input.GetAxis("Mouse Y");
        currentInteractable.transform.Rotate(myCam.transform.right, Mathf.Deg2Rad* y* rotateSpeed, Space.World);
        currentInteractable.transform.Rotate(myCam.transform.up, Mathf.Deg2Rad* x* RotateSpeed, Space.World);

    }

}