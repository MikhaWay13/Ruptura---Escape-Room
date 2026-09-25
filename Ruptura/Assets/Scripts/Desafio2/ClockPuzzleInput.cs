using UnityEngine;
using UnityEngine.InputSystem;

public class ClockPuzzleInput : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ClockPuzzle clockPuzzle;
    [SerializeField] private Camera clockCamera;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference selectAction;
    [SerializeField] private InputActionReference pointerPositionAction;
    [SerializeField] private InputActionReference rotateAction;
    [SerializeField] private InputActionReference confirmAction;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private InputActionReference inventoryAction;

    [Header("Layers dos ponteiros")]
    [SerializeField] private LayerMask hourHandLayer;
    [SerializeField] private LayerMask minuteHandLayer;

    [Header("Raycast")]
    [SerializeField] private float handRayDistance = 5f;

    [Header("Rotação")]
    [SerializeField] private float rotationSensitivity = 0.5f;

    private ClockHand selectedHand;
    private bool isDragging;


    private void OnEnable()
    {
        if (selectAction != null)
        {
            selectAction.action.started += OnSelectStarted;
            selectAction.action.canceled += OnSelectCanceled;

            selectAction.action.Enable();
        }

        if (pointerPositionAction != null)
        {
            pointerPositionAction.action.Enable();
        }

        if (rotateAction != null)
        {
            rotateAction.action.Enable();
        }

        if (confirmAction != null)
        {
            confirmAction.action.performed += OnConfirmPerformed;
            confirmAction.action.Enable();
        }

        if (backAction != null)
        {
            backAction.action.performed += OnBackPerformed;
            backAction.action.Enable();
        }

        if (inventoryAction != null)
        {
            inventoryAction.action.performed += OnInventoryPerformed;
            inventoryAction.action.Enable();
        }
    }


    private void OnDisable()
    {
        if (selectAction != null)
        {
            selectAction.action.started -= OnSelectStarted;
            selectAction.action.canceled -= OnSelectCanceled;
        }

        if (confirmAction != null)
        {
            confirmAction.action.performed -= OnConfirmPerformed;
        }

        if (backAction != null)
        {
            backAction.action.performed -= OnBackPerformed;
        }

        if (inventoryAction != null)
        {
            inventoryAction.action.performed -= OnInventoryPerformed;
        }

        isDragging = false;

        SetSelectedHand(null);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        // Se o inventário estiver aberto,
        // não deixa o relógio girar.
        if (UIManager.instance != null &&
            UIManager.instance.IsInventoryOpen)
        {
            return;
        }

        if (!CanUseClockInput())
        {
            isDragging = false;

            SetSelectedHand(null);

            return;
        }

        // Enquanto estiver no relógio,
        // o mouse fica livre.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Só gira se estiver segurando o botão
        // em cima de um ponteiro.
        if (isDragging && selectedHand != null)
        {
            RotateSelectedHand();
        }
    }


    // =========================================================
    // CLICOU NO PONTEIRO
    // =========================================================

    private void OnSelectStarted(
        InputAction.CallbackContext context)
    {
        if (!CanUseClockInput())
        {
            return;
        }

        if (clockCamera == null ||
            pointerPositionAction == null)
        {
            return;
        }

        Vector2 pointerPosition =
            pointerPositionAction.action.ReadValue<Vector2>();

        Ray ray =
            clockCamera.ScreenPointToRay(pointerPosition);

        LayerMask combinedLayer =
            hourHandLayer | minuteHandLayer;

        RaycastHit[] hits =
            Physics.RaycastAll(
                ray,
                handRayDistance,
                combinedLayer,
                QueryTriggerInteraction.Collide
            );

        ClockHand hand =
            GetSelectableHand(hits);

        // Clicou fora dos ponteiros
        if (hand == null)
        {
            SetSelectedHand(null);

            isDragging = false;

            return;
        }

        SetSelectedHand(hand);

        isDragging = true;
    }


    // =========================================================
    // SOLTOU O BOTÃO
    // =========================================================

    private void OnSelectCanceled(
        InputAction.CallbackContext context)
    {
        isDragging = false;

        SetSelectedHand(null);
    }


    // =========================================================
    // ROTAÇÃO
    // =========================================================

    private void RotateSelectedHand()
    {
        if (selectedHand == null ||
            rotateAction == null)
        {
            return;
        }

        Vector2 mouseDelta =
            rotateAction.action.ReadValue<Vector2>();

        float rotationAmount =
            mouseDelta.x * rotationSensitivity;

        selectedHand.Rotate(rotationAmount);
    }


    // =========================================================
    // CONFIRMAR RELÓGIO
    // =========================================================

   private void OnConfirmPerformed(InputAction.CallbackContext context)
{
    Debug.Log("E chegou no ClockPuzzleInput");

    if (CanUseClockInput())
    {
        Debug.Log("Tentando confirmar relógio");
        clockPuzzle.ConfirmClock();
    }
}


    // =========================================================
    // VOLTAR
    // =========================================================

    private void OnBackPerformed(
        InputAction.CallbackContext context)
    {
        if (!IsClockOpen())
        {
            return;
        }

        if (UIManager.instance != null &&
            UIManager.instance.IsInventoryOpen)
        {
            UIManager.instance.SetInventory(false);

            return;
        }

        isDragging = false;

        SetSelectedHand(null);

        clockPuzzle.CloseClock();
    }


    // =========================================================
    // INVENTÁRIO
    // =========================================================

    private void OnInventoryPerformed(
        InputAction.CallbackContext context)
    {
        if (!CanUseClockInput() ||
            UIManager.instance == null)
        {
            return;
        }

        UIManager.instance.SetInventory(
            !UIManager.instance.IsInventoryOpen
        );
    }


    // =========================================================
    // VERIFICAÇÕES
    // =========================================================

    private bool CanUseClockInput()
    {
        return IsClockOpen() &&
               !clockPuzzle.IsSolved;
    }


    private bool IsClockOpen()
    {
        return clockPuzzle != null &&
               clockPuzzle.IsOpen;
    }


    // =========================================================
    // SELEÇÃO DO PONTEIRO
    // =========================================================

    private void SetSelectedHand(
        ClockHand hand)
    {
        if (selectedHand == hand)
        {
            return;
        }

        if (selectedHand != null)
        {
            selectedHand.SetSelected(false);
        }

        selectedHand = hand;

        if (selectedHand != null)
        {
            selectedHand.SetSelected(true);
        }
    }


    // =========================================================
    // RAYCAST DOS PONTEIROS
    // =========================================================

    private ClockHand GetSelectableHand(
        RaycastHit[] hits)
    {
        if (hits == null ||
            hits.Length == 0)
        {
            return null;
        }

        foreach (RaycastHit hit in hits)
        {
            ClockHand hand =
                hit.collider.GetComponentInParent<ClockHand>();

            if (hand == null)
            {
                continue;
            }

            if (!clockPuzzle.CanSelectHand(hand))
            {
                continue;
            }

            return hand;
        }

        return null;
    }
}