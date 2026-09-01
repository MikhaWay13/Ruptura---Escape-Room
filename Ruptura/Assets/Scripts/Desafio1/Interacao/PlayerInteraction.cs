using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Configurações do Raycast")]
    public float rayDistance = 2f;
    public float RotateSpeed = 200f;

    public Transform objectViewer;
    public Transform objectViewer2;

    public UnityEvent OnView;
    public UnityEvent OnFinishView;

    [Header("Outline")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField, Range(0f, 10f)] private float outlineWidth = 4f;

    [Header("Objeto Movimentável")]
    [SerializeField] private float movableSpeed = 8f;
    [SerializeField] private float collisionPadding = 0.05f;

    [SerializeField] private CutsceneController cutsceneVidro;

    private Camera myCam;
    private Interactables currentInteractable;
    private Outline currentOutline;
    private Interactables currentMovableObject;
    private Rigidbody movableRb;
    private Coroutine movingObjectCoroutine;


    private bool originalGravity;
    private bool originalKinematic;
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

    private void Start()
    {
        myCam = Camera.main;
    }

    private void Update()
    {
        CheckInteractables();
    }

    private void FixedUpdate()
    {
        if (currentMovableObject != null)
        {
            MoveMovableObject();
        }
    }

    private void CheckInteractables()
    {
        // -------------------------------------------------------------
        // 1. OBJETO MOVIMENTÁVEL SENDO CARREGADO
        // -------------------------------------------------------------
        if (currentMovableObject != null)
        {
            SetOutline(null);
            SetHandCursor(false);

            if (pressAction != null && pressAction.IsPressed())
            {
                RotateObject(currentMovableObject);
            }

            if (InteractAction != null && InteractAction.WasPressedThisFrame())
            {
                DropMovableObject();
            }

            return;
        }

        // -------------------------------------------------------------
        // 2. MODO DE INSPEÇÃO DE ITEM
        // -------------------------------------------------------------
        if (isViewing)
        {
            SetOutline(null);

            if (currentInteractable != null && currentInteractable.item != null)
            {
                if (currentInteractable.item.grabbable && pressAction != null && pressAction.IsPressed())
                {
                    RotateObject();
                }

                if (canFinish && BackAction != null && BackAction.WasPressedThisFrame())
                {
                    FinishView();
                }

                if (canFinish && InteractAction != null && InteractAction.WasPressedThisFrame() && currentInteractable.item.ToInventory)
                {
                    TocarSomDoItem(currentInteractable.transform.position);

                    if (InventoryController.instance != null)
                    {
                        

                        bool verificate = InventoryController.instance.AddItem(currentInteractable.item);

                        if (verificate)
                        {
                            Interactables objetoColetado = currentInteractable;
                            Item itemColetado = objetoColetado.item;

                            if(movingObjectCoroutine != null)
                            {
                                StopCoroutine(movingObjectCoroutine);
                                movingObjectCoroutine = null;
                            }

                            isViewing = false;
                            canFinish = false;

                            SetBackImage(false);
                            SetPressE(false);

                            if (OnFinishView != null)
                            {
                                OnFinishView.Invoke();
                            }

                            if (UIManager.instance != null)
                            {
                                UIManager.instance.CloseItemUI();
                            }

                            SetOutline(null);
                            Destroy(currentInteractable.gameObject);
                            currentInteractable = null;


                             if (cutsceneVidro != null)
                            {
                                cutsceneVidro.TentarIniciar(itemColetado);
                            }

                            return;
                        }
                    }
                }
            }

            return;
        }

        // -------------------------------------------------------------
        // 3. RAYCAST DE MIRA
        // -------------------------------------------------------------
        RaycastHit hit;
        Vector3 rayOrigin = myCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));

        if (Physics.Raycast(rayOrigin, myCam.transform.forward, out hit, rayDistance))
        {
            IRaycastInteractable directInteractable = hit.collider.GetComponentInParent<IRaycastInteractable>();
            Interactables interactable = hit.collider.GetComponentInParent<Interactables>();
            RotacaoCofre rotacaoCofre = hit.collider.GetComponentInParent<RotacaoCofre>();

            // Prioriza o interactable mais próximo na hierarquia caso haja sobreposição
            if (directInteractable is MonoBehaviour directComponent && interactable != null)
            {
                if (HierarchyDistance(hit.collider.transform, interactable.transform) <= HierarchyDistance(hit.collider.transform, directComponent.transform))
                {
                    directInteractable = null;
                }
            }

            if (directInteractable != null || interactable != null)
            {
                SetHandCursor(true);
                

                SetOutline(directInteractable is MonoBehaviour directBehaviour ? directBehaviour.gameObject : interactable.gameObject);



                bool ShowPressEinteract = false;

                if(directInteractable != null)
                {
                    ShowPressEinteract = true;
                }
                else if(interactable != null && interactable.item != null&&!interactable.item.hasReadableUI)
                {
                    if( !interactable.item.grabbable || interactable.item.movable )
                    {
                        ShowPressEinteract = true;
                    }
                    
                }

                SetPressEInteracao(ShowPressEinteract);

                if (rotacaoCofre != null && pressAction != null && pressAction.WasPressedThisFrame())
                {
                    rotacaoCofre.Press();
                    return;
                }




                // INTERAÇÃO DIRETA (MONUMENTOS, ALAVANCAS, PORTAS)
                if (directInteractable != null && InteractAction != null && InteractAction.WasPressedThisFrame())
                {
                    directInteractable.Interact();
                    return;
                }

                // ITEM MOVIMENTÁVEL (PROTEGIDO CONTRA NULL)
                if (interactable != null && interactable.item != null && interactable.item.movable && InteractAction != null && InteractAction.WasPressedThisFrame())
                {
                    PickUpMovableObject(interactable);
                    return;
                }


                

                // INSPEÇÃO DE ITEM (PROTEGIDO CONTRA NULL)
                if (interactable != null && interactable.item != null && !interactable.item.movable && pressAction != null && pressAction.WasPressedThisFrame())
                {

                    if (interactable.isMoving)
                    {
                        return;
                    }

                    if (OnView != null)
                    {
                        OnView.Invoke();
                    }

                    currentInteractable = interactable;
                    isViewing = true;

                    Invoke("CanFinish", 1f);

                    if (currentInteractable.item.hasReadableUI)
                    {
                        if (UIManager.instance != null)
                        {
                            UIManager.instance.OpenItemUI(currentInteractable.item);

                            TocarSomDoItem(currentInteractable.transform.position);
                        }
                        return;
                    }

                    if (currentInteractable.item.grabbable)
                    {
                        originPosition = currentInteractable.transform.position;
                        originRotation = currentInteractable.transform.rotation;
                        movingObjectCoroutine = StartCoroutine(MovingObject(currentInteractable, objectViewer.position));
                    }
                    
                    TocarSomDoItem(currentInteractable.transform.position);
                }
            }
            else
            {
                SetHandCursor(false);
                SetOutline(null);
                SetAvisoEquipar(false);
                SetPressEInteracao(false);
            }
        }
        else
        {
            SetHandCursor(false);
            SetOutline(null);
            SetAvisoEquipar(false);
            SetPressEInteracao(false);
        }
    }

    private void PickUpMovableObject(Interactables obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning(obj.name + " é Movimentável mas não possui Rigidbody.");
            return;
        }

        currentMovableObject = obj;
        movableRb = rb;

        originalGravity = rb.useGravity;
        originalKinematic = rb.isKinematic;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;

        obj.isMoving = true;
    }

    private void MoveMovableObject()
    {
        Vector3 movement = objectViewer2.position - movableRb.position;
        float distance = movement.magnitude;

        if (distance < 0.01f)
        {
            return;
        }

        Vector3 direction = movement.normalized;
        float step = Mathf.Min(distance, movableSpeed * Time.fixedDeltaTime);

        if (movableRb.SweepTest(direction, out RaycastHit hit, step, QueryTriggerInteraction.Ignore))
        {
            step = Mathf.Max(0f, hit.distance - collisionPadding);
        }

        movableRb.MovePosition(movableRb.position + direction * step);
    }

    private void DropMovableObject()
    {
        movableRb.linearVelocity = Vector3.zero;
        movableRb.angularVelocity = Vector3.zero;
        movableRb.isKinematic = originalKinematic;
        movableRb.useGravity = originalGravity;

        currentMovableObject.isMoving = false;
        currentMovableObject = null;
        movableRb = null;
    }

    private void CanFinish()
    {
        canFinish = true;
        if (currentInteractable != null &&
        currentInteractable.item != null &&
        currentInteractable.item.hasReadableUI)
    {
        SetPressE(false);
    }
    else
    {
        SetPressE(true);
    }

        SetBackImage(true);
    }

    private void FinishView()
    {
        canFinish = false;
        isViewing = false;
        SetPressE(false);
        SetBackImage(false);

        if (currentInteractable != null && currentInteractable.item != null)
        {
            if (currentInteractable.item.hasReadableUI)
            {
                if (UIManager.instance != null)
                {
                    UIManager.instance.CloseItemUI();

                    TocarSomDoItem(currentInteractable.transform.position);
                }
            }
            else if (currentInteractable.item.grabbable)
            {
                currentInteractable.transform.rotation = originRotation;
                movingObjectCoroutine = StartCoroutine(MovingObject(currentInteractable, originPosition));
            }
        }

        if (OnFinishView != null)
        {
            OnFinishView.Invoke();
        }
    }

    private IEnumerator MovingObject(Interactables obj, Vector3 position)
    {
        if (obj == null)
    {
        movingObjectCoroutine = null;
        yield break;
    }

    obj.isMoving = true;
    float timer = 0f;

    while (timer < 1f)
    {
        if (obj == null)
        {
            movingObjectCoroutine = null;
            yield break;
        }

        obj.transform.position = Vector3.Lerp(obj.transform.position, position, Time.deltaTime * 5f);
        timer += Time.deltaTime;
        yield return null;
    }

    if (obj != null)
    {
        obj.transform.position = position;
        obj.isMoving = false;
    }

    movingObjectCoroutine = null;
    }

    private void TocarSomDoItem(Vector3 posicao)
    {
        if (AudioManager.instance != null && FMODEvent.instance != null)
        {
            AudioManager.instance.PlayOneShot(
                FMODEvent.instance.itemCollectEvent,
                posicao
            );
        }
    }

    // =========================================================================
    // FUNÇÕES DE UI SEGURAS (PROTEGIDAS CONTRA NULL)
    // =========================================================================
    private void SetHandCursor(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetHandCursor(state);
        }
    }

    private void SetPressEInteracao(bool state)
{
    if (UIManager.instance != null)
    {
        UIManager.instance.SetPressEInteracao(state);
    }
}

private void SetAvisoEquipar(bool state)
{
    if (UIManager.instance != null)
    {
        UIManager.instance.SetAvisoEquipar(state);
    }
}

    private void SetBackImage(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetBackImage(state);
        }
    }

    private void SetPressE(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetPressE(state);
        }
    }

    // =========================================================================
    // CONTROLE DE OUTLINE
    // =========================================================================
    private void SetOutline(GameObject target)
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
        if (movingObjectCoroutine != null)
    {
        StopCoroutine(movingObjectCoroutine);
        movingObjectCoroutine = null;
    }

    SetOutline(null);

    if (currentMovableObject != null)
    {
        DropMovableObject();
    }
    }

    // =========================================================================
    // ROTAÇÃO DE OBJETOS
    // =========================================================================
    private void RotateObject(Interactables obj = null)
    {
        Interactables target = obj != null ? obj : currentInteractable;

        if (target == null || lookAction == null)
        {
            return;
        }

        Vector2 mouseDelta = lookAction.ReadValue<Vector2>();
        float x = Mathf.Deg2Rad * mouseDelta.x * RotateSpeed;
        float y = Mathf.Deg2Rad * mouseDelta.y * RotateSpeed;

        if (obj == null)
        {
            target.transform.Rotate(myCam.transform.right, y, Space.World);
            target.transform.Rotate(myCam.transform.up, x, Space.World);
            return;
        }

        RotateWithCollision(target, myCam.transform.right, y);
        RotateWithCollision(target, myCam.transform.up, x);
    }

    private void RotateWithCollision(Interactables obj, Vector3 axis, float angle)
    {
        float maxStep = 2f;
        int steps = Mathf.Max(1, Mathf.CeilToInt(Mathf.Abs(angle) / maxStep));
        float step = angle / steps;

        for (int i = 0; i < steps; i++)
        {
            Quaternion previousRotation = obj.transform.rotation;
            obj.transform.Rotate(axis, step, Space.World);
            Physics.SyncTransforms();

            if (IsOverlapping(obj))
            {
                obj.transform.rotation = previousRotation;
                Physics.SyncTransforms();
                break;
            }
        }
    }

    private bool IsOverlapping(Interactables obj)
    {
        Collider[] colliders = obj.GetComponentsInChildren<Collider>();
        Physics.SyncTransforms();

        foreach (Collider col in colliders)
        {
            Collider[] hits = Physics.OverlapBox(col.bounds.center, col.bounds.extents, Quaternion.identity, ~0, QueryTriggerInteraction.Ignore);

            foreach (Collider hit in hits)
            {
                if (!hit.transform.IsChildOf(obj.transform))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static int HierarchyDistance(Transform origin, Transform ancestor)
    {
        int distance = 0;

        for (Transform current = origin; current != null; current = current.parent)
        {
            if (current == ancestor)
            {
                return distance;
            }

            distance++;
        }

        return int.MaxValue;
    }
}
