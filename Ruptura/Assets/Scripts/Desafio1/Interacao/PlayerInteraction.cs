using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public float rayDistance = 2f;
    public float RotateSpeed = 200f;

    public Transform objectViewer;

    public UnityEvent OnView;
    public UnityEvent OnFinishView;

    private Camera myCam;

    private Interactables currentInteractable;

    private Vector3 originPosition;
    private Quaternion originRotation;
    private bool isViewing;

    private bool canFinish;

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
            if(currentInteractable.item.grabbable && Input.GetMouseButton(0)){
                RotateObject();
            }


            if(canFinish && Input.GetMouseButtonDown(1)){
                FinishView();
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

                    if(interactable.isMoving){
                        return;
                    }

                    OnView.Invoke();

                    currentInteractable = interactable;

                    isViewing= true;

                    Invoke("CanFinish", 1f);

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

    void CanFinish(){
        canFinish=true;
        UIManager.instance.SetBackImage(true);
    }

    void FinishView()
    {
        canFinish = false;
        isViewing = false;
        UIManager.instance.SetBackImage(false);
        if(currentInteractable.item.grabbable)
        {
            currentInteractable.transform.rotation = originRotation;
            StartCoroutine(MovingObject(currentInteractable, originPosition));
        }
        OnFinishView.Invoke();
    }

    IEnumerator MovingObject(Interactables obj, Vector3 position)
    {
        obj.isMoving = true;
        float timer=0;
        while(timer<1)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, position, Time.deltaTime*5); 
            timer+= Time.deltaTime;
            yield return null;
        }

        obj.transform.position=position;
        obj.isMoving = false;
    }


    void RotateObject(){
        float x = Input.GetAxis("Mouse X");
        float y =Input.GetAxis("Mouse Y");
        currentInteractable.transform.Rotate(myCam.transform.right, Mathf.Deg2Rad* y* RotateSpeed, Space.World);
        currentInteractable.transform.Rotate(myCam.transform.up, Mathf.Deg2Rad* x* RotateSpeed, Space.World);

    }

}