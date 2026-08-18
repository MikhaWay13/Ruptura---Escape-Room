using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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

    [Header("Outline")]
    [SerializeField]
    private Color outlineColor = Color.yellow;

    [SerializeField, Range(0f, 10f)]
    private float outlineWidth = 4f;

    private Camera myCam;

    private Interactables currentInteractable;
    private Outline currentOutline;

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

        if (isViewing)
        {
            SetOutline(null);

            if (currentInteractable.item.grabbable && pressAction.IsPressed())
            {
                RotateObject();
            }


            if (canFinish && BackAction.WasPressedThisFrame())
            {
                FinishView();
            }


            if (canFinish && InteractAction.WasPressedThisFrame() && currentInteractable.item.ToInventory)
            {

                bool verificate = InventoryController.instance.AddItem(currentInteractable.item);

                if (verificate)
                {
                    isViewing = false;
                    canFinish = false;
                    UIManager.instance.SetBackImage(false);
                    //criar UI de pressionar E
                    OnFinishView.Invoke();

                    UIManager.instance.CloseItemUI();
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
            IRaycastInteractable directInteractable =
                hit.collider.GetComponentInParent<IRaycastInteractable>();

            Interactables interactable = hit.collider.GetComponentInParent<Interactables>();

            if (directInteractable is MonoBehaviour directComponent &&
                interactable != null &&
                HierarchyDistance(hit.collider.transform, interactable.transform) <=
                HierarchyDistance(hit.collider.transform, directComponent.transform))
            {
                // Um item dentro de outro objeto interagível (como a alavanca
                // dentro da gaveta) deve receber o foco antes do objeto-pai.
                directInteractable = null;
            }




            if (directInteractable != null || interactable != null)
            {
                UIManager.instance.SetHandCursor(true);
                SetOutline(
                    directInteractable is MonoBehaviour directBehaviour
                        ? directBehaviour.gameObject
                        : interactable.gameObject
                );

                if (directInteractable != null &&
                    InteractAction.WasPressedThisFrame())
                {
                    directInteractable.Interact();
                    return;
                }

                if (interactable != null &&
                    pressAction.WasPressedThisFrame())
                {

                    if (interactable.isMoving)
                    {
                        return;
                    }

                    OnView.Invoke();

                    currentInteractable = interactable;

                    isViewing = true;

                    Invoke("CanFinish", 1f);

                    if (currentInteractable.item.hasReadableUI)
                    {
                        UIManager.instance.OpenItemUI(currentInteractable.item);
                        return;
                    }

                    if (currentInteractable.item.grabbable)
                    {
                        originPosition = currentInteractable.transform.position;
                        originRotation = currentInteractable.transform.rotation;
                        StartCoroutine(MovingObject(currentInteractable, objectViewer.position));
                    }
                }

            }
            else
            {
                UIManager.instance.SetHandCursor(false);
                SetOutline(null);
            }
        }
        else
        {
            UIManager.instance.SetHandCursor(false);
            SetOutline(null);
        }

    }

    void CanFinish()
    {
        canFinish = true;
        UIManager.instance.SetBackImage(true);
    }

    void FinishView()
    {
        canFinish = false;
        isViewing = false;
        UIManager.instance.SetBackImage(false);

        if (currentInteractable.item.hasReadableUI)
        {
            UIManager.instance.CloseItemUI();
        }
        if (currentInteractable.item.hasReadableUI && currentInteractable.item.ToInventory)
        {
            UIManager.instance.CloseItemUI();
        }
        else if (currentInteractable.item.grabbable)
        {
            currentInteractable.transform.rotation = originRotation;
            StartCoroutine(MovingObject(currentInteractable, originPosition));
        }
        OnFinishView.Invoke();
    }

    IEnumerator MovingObject(Interactables obj, Vector3 position)
    {
        obj.isMoving = true;
        float timer = 0;
        while (timer < 1)
        {
            obj.transform.position = Vector3.Lerp(obj.transform.position, position, Time.deltaTime * 5);
            timer += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = position;
        obj.isMoving = false;
    }

    void SetHandCursor(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetHandCursor(state);
        }
    }

    void SetBackImage(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetBackImage(state);
        }
    }

    void SetOutline(GameObject target)
    {
        Outline nextOutline = null;

        if (target != null)
        {
            nextOutline = target.GetComponent<Outline>();

            if (nextOutline == null)
            {
                nextOutline = target.AddComponent<Outline>();
            }
        }

        if (currentOutline == nextOutline)
        {
            return;
        }

        if (currentOutline != null)
        {
            currentOutline.enabled = false;
        }

        currentOutline = nextOutline;

        if (currentOutline != null)
        {
            currentOutline.OutlineMode = Outline.Mode.OutlineVisible;
            currentOutline.OutlineColor = outlineColor;
            currentOutline.OutlineWidth = outlineWidth;
            currentOutline.enabled = true;
        }
    }

    private void OnDisable()
    {
        SetOutline(null);
    }

    void RotateObject()
    {
        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        float x = mouseDelta.x;
        float y = mouseDelta.y;
        currentInteractable.transform.Rotate(myCam.transform.right, Mathf.Deg2Rad * y * RotateSpeed, Space.World);
        currentInteractable.transform.Rotate(myCam.transform.up, Mathf.Deg2Rad * x * RotateSpeed, Space.World);

    }

    private static int HierarchyDistance(Transform origin, Transform ancestor)
    {
        int distance = 0;

        for (Transform current = origin;
             current != null;
             current = current.parent)
        {
            if (current == ancestor)
            {
                return distance;
            }

            distance++;
        }

        return int.MaxValue;
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
