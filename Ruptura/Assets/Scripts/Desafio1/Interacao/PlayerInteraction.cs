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


    private InputAction InteractAction;
    private InputAction pressAction;
    private InputAction BackAction;
    private InputAction lookAction;

    private void Awake()
    {
        pressAction = InputSystem.actions.FindAction("Interaction/Press");
        InteractAction = InputSystem.actions.FindAction("Interaction/Interact");
        BackAction = InputSystem.actions.FindAction("Interaction/Back");
        lookAction = InputSystem.actions.FindAction("Interaction/Look");
    }



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

            if(currentInteractable.item.grabbable && pressAction.IsPressed()){
                RotateObject();
            }


            if (canFinish && BackAction.WasPressedThisFrame())
            {
                FinishView();
            }
           

            if (InteractAction.WasPressedThisFrame() && currentInteractable.item.ToInventory)
            {
                
                bool verificate = InventoryController.instance.AddItem(currentInteractable.item);
                
                if (verificate)
                {
                    isViewing = false;
                    canFinish = false;
                    UIManager.instance.SetBackImage(false);
                    //criar UI de pressionar E
                    OnFinishView.Invoke();
                    
                    Destroy(currentInteractable.gameObject);
                    return; 
                }
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
                if(pressAction.WasPressedThisFrame()) 
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


    void RotateObject()
    {
        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        float x = mouseDelta.x;
        float y = mouseDelta.y;
        currentInteractable.transform.Rotate(myCam.transform.right, Mathf.Deg2Rad * y * RotateSpeed, Space.World);
        currentInteractable.transform.Rotate(myCam.transform.up, Mathf.Deg2Rad * x * RotateSpeed, Space.World);

    }
    










/*
        // ==========================================
        // 2. ESTADO NORMAL (OLHANDO PELO MUNDO)
        // ==========================================
        RaycastHit hit;
        Vector3 rayOrigin = myCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));

        if (Physics.Raycast(rayOrigin, myCam.transform.forward, out hit, rayDistance))
        {
            Interactables interactable = hit.collider.GetComponent<Interactables>();

            if (interactable != null)
            {
                UIManager.instance.SetHandCursor(true);
                
                // INSPECIONAR OBJETO (Botão Esquerdo)
                if(pressAction.WasPressedThisFrame()) 
                {
                    if(interactable.isMoving){
                        return;
                    }

                    OnView.Invoke();
                    currentInteractable = interactable;
                    isViewing = true;
                    Invoke("CanFinish", 1f);

                    if(currentInteractable.item.grabbable)
                    {
                        originPosition = currentInteractable.transform.position;
                        originRotation = currentInteractable.transform.rotation;
                        StartCoroutine(MovingObject(currentInteractable, objectViewer.position));
                    }
                }
                
                // PEGAR DIRETO DO CHÃO (Botão E)
                if (Input.GetKeyDown(KeyCode.E) && interactable.item.toInventory)
                {
                    bool guardouComSucesso = InventoryController.instance.AddItem(interactable.item);

                    if (guardouComSucesso)
                    {
                        Destroy(hit.transform.gameObject);
                        UIManager.instance.SetHandCursor(false);
                    }
                }
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
    


*/





}