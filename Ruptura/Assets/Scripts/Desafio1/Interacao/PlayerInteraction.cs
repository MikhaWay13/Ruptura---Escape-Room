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

    [Header("Objeto Movimentável")]
    [SerializeField] private float movableSpeed = 8f;
    [SerializeField] private float collisionPadding = 0.05f;

    private Camera myCam;

    private Interactables currentInteractable;
    private Outline currentOutline;

    // NOVO: objeto sendo carregado
    private Interactables currentMovableObject;
    private Rigidbody movableRb;

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
        pressAction =
            InputSystem.actions.FindAction("Interaction/Press");

        InteractAction =
            InputSystem.actions.FindAction("Interaction/Interact");

        BackAction =
            InputSystem.actions.FindAction("Interaction/Back");

        lookAction =
            InputSystem.actions.FindAction("Interaction/Look");
    }


    void Start()
    {
        myCam = Camera.main;
    }


    void Update()
    {
        CheckInteractables();
    }


    // NOVO: somente para acompanhar o jogador
    private void FixedUpdate()
    {
        if (currentMovableObject != null)
            MoveMovableObject();
    }


    void CheckInteractables()
    {
        // =================================================
        // NOVO: OBJETO MOVIMENTÁVEL SENDO CARREGADO
        // =================================================

        if (currentMovableObject != null)
        {
            SetOutline(null);
            UIManager.instance.SetHandCursor(false);

            // Segurar botão esquerdo para girar
            if (pressAction.IsPressed())
                RotateObject(currentMovableObject);

            // E novamente para soltar
            if (InteractAction.WasPressedThisFrame())
                DropMovableObject();

            return;
        }


        // =================================================
        // CÓDIGO ANTIGO DE INSPEÇÃO
        // =================================================

        if (isViewing)
        {
            SetOutline(null);

            if (currentInteractable.item.grabbable &&
                pressAction.IsPressed())
            {
                RotateObject();
            }


            if (canFinish &&
                BackAction.WasPressedThisFrame())
            {
                FinishView();
            }


            if (canFinish &&
                InteractAction.WasPressedThisFrame() &&
                currentInteractable.item.ToInventory)
            {
                bool verificate =
                    InventoryController.instance.AddItem(
                        currentInteractable.item
                    );

                if (verificate)
                {
                    isViewing = false;
                    canFinish = false;

                    UIManager.instance.SetBackImage(false);

                    OnFinishView.Invoke();

                    UIManager.instance.CloseItemUI();

                    Destroy(
                        currentInteractable.gameObject
                    );

                    return;
                }
            }

            return;
        }


        // =================================================
        // CÓDIGO ANTIGO DO RAYCAST
        // =================================================

        RaycastHit hit;

        Vector3 rayOrigin =
            myCam.ViewportToWorldPoint(
                new Vector3(0.5f, 0.5f, 0.5f)
            );


        if (Physics.Raycast(
            rayOrigin,
            myCam.transform.forward,
            out hit,
            rayDistance))
        {
            IRaycastInteractable directInteractable =
                hit.collider.GetComponentInParent
                <IRaycastInteractable>();


            Interactables interactable =
                hit.collider.GetComponentInParent
                <Interactables>();


            if (directInteractable
                    is MonoBehaviour directComponent &&
                interactable != null &&
                HierarchyDistance(
                    hit.collider.transform,
                    interactable.transform
                )
                <=
                HierarchyDistance(
                    hit.collider.transform,
                    directComponent.transform
                ))
            {
                directInteractable = null;
            }


            if (directInteractable != null ||
                interactable != null)
            {
                UIManager.instance.SetHandCursor(true);

                // EXATAMENTE O SISTEMA ANTIGO DE OUTLINE
                SetOutline(
                    directInteractable
                        is MonoBehaviour directBehaviour
                            ? directBehaviour.gameObject
                            : interactable.gameObject
                );


                // =========================================
                // INTERAÇÕES ESPECIAIS ANTIGAS
                // =========================================

                if (directInteractable != null &&
                    InteractAction.WasPressedThisFrame())
                {
                    directInteractable.Interact();
                    return;
                }


                // =========================================
                // NOVO: ITEM MOVIMENTÁVEL
                // =========================================

                if (interactable != null &&
                    interactable.item != null &&
                    interactable.item.movable &&
                    InteractAction.WasPressedThisFrame())
                {
                    PickUpMovableObject(
                        interactable
                    );

                    return;
                }


                // =========================================
                // INSPEÇÃO ANTIGA
                // =========================================

                if (interactable != null &&
                    pressAction.WasPressedThisFrame())
                {
                    if (interactable.isMoving)
                    {
                        return;
                    }

                    OnView.Invoke();

                    currentInteractable =
                        interactable;

                    isViewing = true;

                    Invoke(
                        "CanFinish",
                        1f
                    );


                    if (currentInteractable
                        .item.hasReadableUI)
                    {
                        UIManager.instance.OpenItemUI(
                            currentInteractable.item
                        );

                        return;
                    }


                    if (currentInteractable
                        .item.grabbable)
                    {
                        originPosition =
                            currentInteractable
                            .transform.position;

                        originRotation =
                            currentInteractable
                            .transform.rotation;

                        StartCoroutine(
                            MovingObject(
                                currentInteractable,
                                objectViewer.position
                            )
                        );
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


    // =====================================================
    // NOVO: MOVIMENTÁVEL
    // =====================================================

    private void PickUpMovableObject(
        Interactables obj)
    {
        Rigidbody rb =
            obj.GetComponent<Rigidbody>();

        if (rb == null)
        {
            Debug.LogWarning(
                obj.name +
                " é Movimentável mas não possui Rigidbody."
            );

            return;
        }

        currentMovableObject = obj;
        movableRb = rb;

        originalGravity =
            rb.useGravity;

        originalKinematic =
            rb.isKinematic;

        rb.linearVelocity =
            Vector3.zero;

        rb.angularVelocity =
            Vector3.zero;

        rb.useGravity = false;
        rb.isKinematic = true;

        obj.isMoving = true;
    }


    private void MoveMovableObject()
    {
        Vector3 movement =
            objectViewer.position -
            movableRb.position;

        float distance =
            movement.magnitude;

        if (distance < 0.01f)
            return;

        Vector3 direction =
            movement.normalized;

        float step =
            Mathf.Min(
                distance,
                movableSpeed *
                Time.fixedDeltaTime
            );


        // Evita atravessar paredes
        if (movableRb.SweepTest(
            direction,
            out RaycastHit hit,
            step,
            QueryTriggerInteraction.Ignore))
        {
            step =
                Mathf.Max(
                    0f,
                    hit.distance -
                    collisionPadding
                );
        }


        movableRb.MovePosition(
            movableRb.position +
            direction * step
        );
    }


    private void DropMovableObject()
    {
        movableRb.linearVelocity =
            Vector3.zero;

        movableRb.angularVelocity =
            Vector3.zero;

        movableRb.isKinematic =
            originalKinematic;

        movableRb.useGravity =
            originalGravity;

        currentMovableObject.isMoving =
            false;

        currentMovableObject = null;
        movableRb = null;
    }


    // =====================================================
    // TODO ABAIXO É O SISTEMA ANTIGO
    // =====================================================

    void CanFinish()
    {
        canFinish = true;

        UIManager.instance.SetBackImage(
            true
        );
    }


    void FinishView()
    {
        canFinish = false;
        isViewing = false;

        UIManager.instance.SetBackImage(
            false
        );


        if (currentInteractable
            .item.hasReadableUI)
        {
            UIManager.instance.CloseItemUI();
        }
        else if (currentInteractable
            .item.grabbable)
        {
            currentInteractable
                .transform.rotation =
                originRotation;

            StartCoroutine(
                MovingObject(
                    currentInteractable,
                    originPosition
                )
            );
        }

        OnFinishView.Invoke();
    }


    IEnumerator MovingObject(
        Interactables obj,
        Vector3 position)
    {
        obj.isMoving = true;

        float timer = 0;


        while (timer < 1)
        {
            obj.transform.position =
                Vector3.Lerp(
                    obj.transform.position,
                    position,
                    Time.deltaTime * 5
                );

            timer += Time.deltaTime;

            yield return null;
        }


        obj.transform.position =
            position;

        obj.isMoving =
            false;
    }


    void SetHandCursor(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetHandCursor(
                state
            );
        }
    }


    void SetBackImage(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetBackImage(
                state
            );
        }
    }


    // =====================================================
    // OUTLINE ANTIGO — NÃO ALTERADO
    // =====================================================

    void SetOutline(GameObject target)
    {
        Outline nextOutline = null;

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


    // =====================================================
    // MESMA ROTAÇÃO, AGORA ACEITANDO OPCIONALMENTE UM OBJETO
    // =====================================================

    void RotateObject(
        Interactables obj = null)
    {
        Interactables target =
            obj != null
                ? obj
                : currentInteractable;


        Vector2 mouseDelta =
            lookAction.ReadValue<Vector2>();

        float x =
            mouseDelta.x;

        float y =
            mouseDelta.y;


        target.transform.Rotate(
            myCam.transform.right,
            Mathf.Deg2Rad *
            y *
            RotateSpeed,
            Space.World
        );


        target.transform.Rotate(
            myCam.transform.up,
            Mathf.Deg2Rad *
            x *
            RotateSpeed,
            Space.World
        );
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
            {
                return distance;
            }

            distance++;
        }

        return int.MaxValue;
    }
}