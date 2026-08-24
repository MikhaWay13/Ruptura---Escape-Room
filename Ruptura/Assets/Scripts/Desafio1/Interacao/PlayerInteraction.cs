using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interação")]
    public float rayDistance = 2f;
    public float RotateSpeed = 200f;
    public Transform objectViewer;

    [Header("Objeto Movimentável")]
    [SerializeField] private float movableSpeed = 8f;
    [SerializeField] private float collisionPadding = 0.05f;

    [Header("Outline")]
    [SerializeField] private Color outlineColor = Color.yellow;
    [SerializeField, Range(0f, 10f)] private float outlineWidth = 4f;

    public UnityEvent OnView;
    public UnityEvent OnFinishView;

    private Camera myCam;

    private Interactables currentInteractable;
    private Interactables currentMovableObject;

    private Rigidbody movableRb;
    private Outline currentOutline;

    private Vector3 originPosition;
    private Quaternion originRotation;

    private bool isViewing;
    private bool canFinish;

    private bool originalGravity;
    private bool originalKinematic;

    private InputAction InteractAction;
    private InputAction pressAction;
    private InputAction BackAction;
    private InputAction lookAction;


    private void Awake()
    {
        pressAction =
            InputSystem.actions.FindAction("Interaction/Press");

        InteractAction =
            InputSystem.actions.FindAction("Interaction/Interact");

        BackAction =
            InputSystem.actions.FindAction("Interaction/Back");

        lookAction =
            InputSystem.actions.FindAction("Interaction/Look");
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
            MoveMovableObject();
    }


    // =====================================================
    // CONTROLE PRINCIPAL
    // =====================================================

    private void CheckInteractables()
    {
        if (currentMovableObject != null)
        {
            HandleMovableObject();
            return;
        }

        if (isViewing)
        {
            HandleInspection();
            return;
        }

        CheckRaycast();
    }


    // =====================================================
    // OBJETO MOVIMENTÁVEL
    // =====================================================

    private void HandleMovableObject()
    {
        SetOutline(null);
        SetHandCursor(false);

        // Segurar botão esquerdo = girar
        if (pressAction.IsPressed())
            RotateObject(currentMovableObject);

        // E = soltar
        if (InteractAction.WasPressedThisFrame())
            DropMovableObject();
    }


    private void PickUpMovableObject(Interactables obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning(
                $"{obj.name} está marcado como Movimentável, " +
                "mas não possui Rigidbody."
            );

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
        if (objectViewer == null || movableRb == null)
            return;

        Vector3 movement =
            objectViewer.position - movableRb.position;

        float distance = movement.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 direction = movement.normalized;

        float step = Mathf.Min(
            distance,
            movableSpeed * Time.fixedDeltaTime
        );

        // Impede o objeto de atravessar paredes
        if (movableRb.SweepTest(
            direction,
            out RaycastHit hit,
            step,
            QueryTriggerInteraction.Ignore))
        {
            step = Mathf.Max(
                0f,
                hit.distance - collisionPadding
            );
        }

        movableRb.MovePosition(
            movableRb.position +
            direction * step
        );
    }


    private void DropMovableObject()
    {
        if (movableRb != null)
        {
            movableRb.linearVelocity = Vector3.zero;
            movableRb.angularVelocity = Vector3.zero;

            movableRb.isKinematic = originalKinematic;
            movableRb.useGravity = originalGravity;
        }

        currentMovableObject.isMoving = false;

        currentMovableObject = null;
        movableRb = null;
    }


    // =====================================================
    // INSPEÇÃO
    // =====================================================

    private void HandleInspection()
    {
        SetOutline(null);

        if (currentInteractable == null ||
            currentInteractable.item == null)
        {
            isViewing = false;
            return;
        }

        Item item = currentInteractable.item;

        // Rotação durante inspeção
        if (item.grabbable && pressAction.IsPressed())
            RotateObject(currentInteractable);

        // Voltar
        if (canFinish &&
            BackAction.WasPressedThisFrame())
        {
            FinishView();
            return;
        }

        // Adicionar ao inventário
        if (canFinish &&
            item.ToInventory &&
            InteractAction.WasPressedThisFrame())
        {
            AddCurrentItemToInventory();
        }
    }


    private void StartInspection(Interactables obj)
    {
        if (obj.isMoving || obj.item == null)
            return;

        Item item = obj.item;

        // Não entra em inspeção se não houver
        // nenhuma interação desse tipo.
        if (!item.grabbable &&
            !item.hasReadableUI &&
            !item.ToInventory)
        {
            return;
        }

        currentInteractable = obj;
        isViewing = true;
        canFinish = false;

        OnView.Invoke();
        Invoke(nameof(CanFinish), 1f);

        if (item.hasReadableUI)
        {
            UIManager.instance.OpenItemUI(item);
            return;
        }

        if (item.grabbable)
        {
            originPosition = obj.transform.position;
            originRotation = obj.transform.rotation;

            StartCoroutine(
                MovingObject(
                    obj,
                    objectViewer.position
                )
            );
        }
    }


    private void FinishView()
    {
        Item item = currentInteractable.item;
        Interactables obj = currentInteractable;

        isViewing = false;
        canFinish = false;

        SetBackImage(false);

        if (item.hasReadableUI)
        {
            UIManager.instance.CloseItemUI();
        }
        else if (item.grabbable)
        {
            obj.transform.rotation = originRotation;

            StartCoroutine(
                MovingObject(
                    obj,
                    originPosition
                )
            );
        }

        currentInteractable = null;

        OnFinishView.Invoke();
    }


    private void AddCurrentItemToInventory()
    {
        if (!InventoryController.instance.AddItem(
            currentInteractable.item))
        {
            return;
        }

        Interactables obj = currentInteractable;

        if (obj.item.hasReadableUI)
            UIManager.instance.CloseItemUI();

        isViewing = false;
        canFinish = false;
        currentInteractable = null;

        SetBackImage(false);

        OnFinishView.Invoke();

        Destroy(obj.gameObject);
    }


    private void CanFinish()
    {
        canFinish = true;
        SetBackImage(true);
    }


    // =====================================================
    // RAYCAST
    // =====================================================

    private void CheckRaycast()
    {
        Vector3 rayOrigin =
            myCam.ViewportToWorldPoint(
                new Vector3(0.5f, 0.5f, 0.5f)
            );

        if (!Physics.Raycast(
            rayOrigin,
            myCam.transform.forward,
            out RaycastHit hit,
            rayDistance))
        {
            ClearFocus();
            return;
        }

        IRaycastInteractable directInteractable =
            hit.collider.GetComponentInParent
            <IRaycastInteractable>();

        Interactables interactable =
            hit.collider.GetComponentInParent
            <Interactables>();


        // Mantém a prioridade entre objetos
        // filhos e objetos-pai.
        if (directInteractable is MonoBehaviour direct &&
            interactable != null &&
            HierarchyDistance(
                hit.collider.transform,
                interactable.transform)
            <=
            HierarchyDistance(
                hit.collider.transform,
                direct.transform))
        {
            directInteractable = null;
        }


        if (directInteractable == null &&
            interactable == null)
        {
            ClearFocus();
            return;
        }


        SetHandCursor(true);

        SetOutline(
            directInteractable is MonoBehaviour behaviour
                ? behaviour.gameObject
                : interactable.gameObject
        );


        // Interações especiais:
        // gaveta, alavanca etc.
        if (directInteractable != null &&
            InteractAction.WasPressedThisFrame())
        {
            directInteractable.Interact();
            return;
        }


        if (interactable == null ||
            interactable.item == null)
        {
            return;
        }


        // E = pegar objeto movimentável
        if (interactable.item.movable &&
            InteractAction.WasPressedThisFrame())
        {
            PickUpMovableObject(interactable);
            return;
        }


        // Botão esquerdo = inspeção
        if (pressAction.WasPressedThisFrame())
        {
            StartInspection(interactable);
        }
    }


    // =====================================================
    // MOVIMENTO DE INSPEÇÃO
    // =====================================================

    private IEnumerator MovingObject(
        Interactables obj,
        Vector3 target)
    {
        obj.isMoving = true;

        float timer = 0f;

        while (timer < 1f)
        {
            obj.transform.position =
                Vector3.Lerp(
                    obj.transform.position,
                    target,
                    Time.deltaTime * 5f
                );

            timer += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = target;
        obj.isMoving = false;
    }


    // =====================================================
    // ROTAÇÃO
    // =====================================================

    private void RotateObject(Interactables obj)
    {
        Vector2 mouseDelta =
            lookAction.ReadValue<Vector2>();

        obj.transform.Rotate(
            myCam.transform.right,
            Mathf.Deg2Rad *
            mouseDelta.y *
            RotateSpeed,
            Space.World
        );

        obj.transform.Rotate(
            myCam.transform.up,
            Mathf.Deg2Rad *
            mouseDelta.x *
            RotateSpeed,
            Space.World
        );
    }


    // =====================================================
    // UI / OUTLINE
    // =====================================================

    private void ClearFocus()
    {
        SetHandCursor(false);
        SetOutline(null);
    }


    private void SetHandCursor(bool state)
    {
        if (UIManager.instance != null)
            UIManager.instance.SetHandCursor(state);
    }


    private void SetBackImage(bool state)
    {
        if (UIManager.instance != null)
            UIManager.instance.SetBackImage(state);
    }


    void SetOutline(
        GameObject target)
    {
        Outline nextOutline =
            null;


        if (target != null)
        {
            nextOutline =
                target.GetComponent<Outline>();


            if (nextOutline == null)
            {
                nextOutline =
                    target.AddComponent<Outline>();
            }
        }


        if (currentOutline ==
            nextOutline)
        {
            return;
        }


        if (currentOutline != null)
        {
            currentOutline.enabled =
                false;
        }


        currentOutline =
            nextOutline;


        if (currentOutline != null)
        {
            currentOutline.OutlineMode =
                Outline.Mode.OutlineVisible;


            currentOutline.OutlineColor =
                outlineColor;


            currentOutline.OutlineWidth =
                outlineWidth;


            currentOutline.enabled =
                true;
        }
    }

    private void OnDisable()
    {
        SetOutline(null);

        if (currentMovableObject != null)
            DropMovableObject();
    }


    private static int HierarchyDistance(
        Transform origin,
        Transform ancestor)
    {
        int distance = 0;

        for (Transform current = origin;
             current != null;
             current = current.parent)
        {
            if (current == ancestor)
                return distance;

            distance++;
        }

        return int.MaxValue;
    }
}