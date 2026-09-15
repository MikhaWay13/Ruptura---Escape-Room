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
    [SerializeField] private float rotationSensitivity = 1f;
    private ClockHand selectedHand;

    private void OnEnable()
    {
        Subscribe(selectAction, OnSelectPerformed);
        Subscribe(rotateAction, OnRotatePerformed);
        Subscribe(confirmAction, OnConfirmPerformed);
        Subscribe(backAction, OnBackPerformed);
        Subscribe(inventoryAction, OnInventoryPerformed);

        EnableAction(selectAction);
        EnableAction(pointerPositionAction);
        EnableAction(rotateAction);
        EnableAction(confirmAction);
        EnableAction(backAction);
        EnableAction(inventoryAction);
    }

    private void OnDisable()
    {
        Unsubscribe(selectAction, OnSelectPerformed);
        Unsubscribe(rotateAction, OnRotatePerformed);
        Unsubscribe(confirmAction, OnConfirmPerformed);
        Unsubscribe(backAction, OnBackPerformed);
        Unsubscribe(inventoryAction, OnInventoryPerformed);

        SetSelectedHand(null);

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }

   private void Update()
{
    // Se o inventário estiver aberto, não interfere no estado do cursor
    if (UIManager.instance != null && UIManager.instance.IsInventoryOpen)
        return;

    bool isUsingClock = CanUseClockInput();

    if (isUsingClock)
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    else
    {
        SetSelectedHand(null);
    }
}

    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        if (!CanUseClockInput() || clockCamera == null || pointerPositionAction == null)
            return;

        Vector2 pointerPosition = pointerPositionAction.action.ReadValue<Vector2>();
        Ray ray = clockCamera.ScreenPointToRay(pointerPosition);

        LayerMask combinedLayer =
            hourHandLayer |
            minuteHandLayer;

        RaycastHit[] hits = Physics.RaycastAll(
            ray,
            handRayDistance,
            combinedLayer,
            QueryTriggerInteraction.Ignore
        );

        SetSelectedHand(GetSelectableHand(hits));
    }

    private void OnRotatePerformed(InputAction.CallbackContext context)
    {
        if (!CanUseClockInput() || selectedHand == null)
            return;

        Vector2 rotationInput = context.ReadValue<Vector2>();
        selectedHand.Rotate(rotationInput.x * rotationSensitivity);
    }

    private void OnConfirmPerformed(InputAction.CallbackContext context)
    {
        if (CanUseClockInput())
            clockPuzzle.ConfirmClock();
    }

    private void OnBackPerformed(
        InputAction.CallbackContext context)
    {
        // Sair do puzzle deve continuar possível depois da solução. As demais
        // ações ficam bloqueadas por CanUseClockInput quando o relógio resolve.
        if (!IsClockOpen())
            return;

        if (UIManager.instance != null &&
            UIManager.instance.IsInventoryOpen)
        {
            UIManager.instance.SetInventory(false);
            return;
        }

        SetSelectedHand(null);

        clockPuzzle.CloseClock();
    }

    private void OnInventoryPerformed(
        InputAction.CallbackContext context)
    {
        if (!CanUseClockInput() || UIManager.instance == null)
            return;

        UIManager.instance.SetInventory(
            !UIManager.instance.IsInventoryOpen
        );
    }

    private bool CanUseClockInput()
    {
        return IsClockOpen() && !clockPuzzle.IsSolved;
    }

    private bool IsClockOpen()
    {
        return clockPuzzle != null && clockPuzzle.IsOpen;
    }

    private void SetSelectedHand(ClockHand hand)
    {
        if (selectedHand == hand)
            return;

        if (selectedHand != null)
            selectedHand.SetSelected(false);

        selectedHand = hand;

        if (selectedHand != null)
            selectedHand.SetSelected(true);
    }

    private ClockHand GetSelectableHand(RaycastHit[] hits)
    {
        ClockHand firstSelectableHand = null;

        foreach (RaycastHit hit in hits)
        {
            ClockHand hand = hit.collider.GetComponentInParent<ClockHand>();

            if (!clockPuzzle.CanSelectHand(hand))
                continue;

            if (firstSelectableHand == null)
                firstSelectableHand = hand;

            // Quando os colliders se sobrepõem na visão da câmera, clicar
            // novamente alterna para o outro ponteiro disponível.
            if (hand != selectedHand)
                return hand;
        }

        return firstSelectableHand;
    }

    private static void Subscribe(InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference != null)
            actionReference.action.performed += callback;
    }

    private static void Unsubscribe(InputActionReference actionReference,
        System.Action<InputAction.CallbackContext> callback)
    {
        if (actionReference != null)
            actionReference.action.performed -= callback;
    }

    private static void EnableAction(InputActionReference actionReference)
    {
        if (actionReference != null && !actionReference.action.enabled)
            actionReference.action.Enable();
    }
}
