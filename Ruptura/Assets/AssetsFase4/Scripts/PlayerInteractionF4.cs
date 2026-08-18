using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using System.Collections;

public class PlayerInteractionF4 : MonoBehaviour
{
    public float rayDistance = 2f;
    public float RotateSpeed = 200f;

    public Transform objectViewer;

    public UnityEvent OnView;
    public UnityEvent OnFinishView;

    private Camera myCam;

    private InteractablesF4 currentInteractable;

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
            if (currentInteractable.item.grabbable && pressAction.IsPressed())
            {
                RotateObject();
            }

            if (canFinish && BackAction.WasPressedThisFrame())
            {
                FinishView();
            }

            if (InteractAction.WasPressedThisFrame() && currentInteractable.item.ToInventory)
            {
                bool verificate = InventarioF4.instance.AddItensF4(currentInteractable.item);

                if (verificate)
                {
                    isViewing = false;
                    canFinish = false;
                    UIF4.instance.SetBackImage(false);
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
            InteractablesF4 interactable = hit.collider.GetComponent<InteractablesF4>();

            if (interactable != null)
            {
                UIF4.instance.SetHandCursor(true);
                if (pressAction.WasPressedThisFrame())
                {
                    if (interactable.isMoving)
                    {
                        return;
                    }

                    OnView.Invoke();

                    currentInteractable = interactable;

                    isViewing = true;

                    Invoke("CanFinish", 1f);

                    // itens com UI própria abrem o painel genérico e não entram
                    // no fluxo de mover/segurar/girar o objeto físico
                    if (currentInteractable.item.hasReadableUI)
                    {
                        UIF4.instance.OpenItemUI(currentInteractable.item);
                        return;
                    }

                    if (currentInteractable.item.grabbable)
                    {
                        originPosition = currentInteractable.transform.position;
                        originRotation = currentInteractable.transform.rotation;
                        StartCoroutine(MovingObject(currentInteractable, objectViewer.position));
                    }
                    //mudar pro Input Manager
                }
            }
            else
            {
                UIF4.instance.SetHandCursor(false);
            }
        }
        else
        {
            UIF4.instance.SetHandCursor(false);
        }
    }

    void CanFinish()
    {
        canFinish = true;
        UIF4.instance.SetBackImage(true);
    }

    void FinishView()
    {
        canFinish = false;
        isViewing = false;
        UIF4.instance.SetBackImage(false);

        if (currentInteractable.item.hasReadableUI)
        {
            UIF4.instance.CloseItemUI();
        }
        else if (currentInteractable.item.grabbable)
        {
            currentInteractable.transform.rotation = originRotation;
            StartCoroutine(MovingObject(currentInteractable, originPosition));
        }

        OnFinishView.Invoke();
    }

    IEnumerator MovingObject(InteractablesF4 obj, Vector3 position)
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

    void RotateObject()
    {
        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        float x = mouseDelta.x;
        float y = mouseDelta.y;
        currentInteractable.transform.Rotate(myCam.transform.right, Mathf.Deg2Rad * y * RotateSpeed, Space.World);
        currentInteractable.transform.Rotate(myCam.transform.up, Mathf.Deg2Rad * x * RotateSpeed, Space.World);
    }
}
