using UnityEngine;
using UnityEngine.InputSystem;

public class ClockPuzzleInput : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ClockPuzzle clockPuzzle;
    [SerializeField] private Camera clockCamera;

    [Header("Layers dos ponteiros")]
    [SerializeField] private LayerMask hourHandLayer;
    [SerializeField] private LayerMask minuteHandLayer;

    [Header("Layer do relógio")]
    [SerializeField] private LayerMask clockLayer;

    [Header("Raycast")]
    [SerializeField] private float handRayDistance = 5f;

    [Header("Rotação")]
    [SerializeField] private float rotationSensitivity = 1f;
    [SerializeField] private bool invertRotation = false;

    private InputAction backInput;
    private InputAction inventoryInput;
    private InputAction confirmInput;

    private ClockHand selectedHand;

    private Vector2 previousMousePosition;

    private void Awake()
    {
        backInput =
            InputSystem.actions.FindAction(
                "Interaction/Back"
            );

        inventoryInput =
            InputSystem.actions.FindAction(
                "Player/Inventory"
            );

        confirmInput =
            InputSystem.actions.FindAction(
                "Interaction/Confirm"
            );
    }

    private void OnEnable()
    {
        if (backInput != null)
            backInput.performed += OnBackPerformed;

        if (inventoryInput != null)
            inventoryInput.performed += OnInventoryPerformed;
    }

    private void OnDisable()
    {
        if (backInput != null)
            backInput.performed -= OnBackPerformed;

        if (inventoryInput != null)
            inventoryInput.performed -= OnInventoryPerformed;

        selectedHand = null;

        Cursor.lockState =
            CursorLockMode.Locked;

        Cursor.visible = false;
    }

    private void Update()
    {
        if (clockPuzzle == null ||
            !clockPuzzle.IsOpen ||
            clockPuzzle.IsSolved)
        {
            selectedHand = null;
            return;
        }

        Cursor.lockState =
            CursorLockMode.None;

        Cursor.visible = true;

        HandleHandSelection();
        HandleHandRotation();

        // Q = confirmar
        if (confirmInput != null &&
            confirmInput.WasPressedThisFrame())
        {
            clockPuzzle.ConfirmClock();
        }
        HandleClockConfirmation();
    }

    private void HandleHandSelection()
    {
        if (clockCamera == null ||
            Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            clockCamera.ScreenPointToRay(
                mousePosition
            );

        LayerMask combinedLayer =
            hourHandLayer |
            minuteHandLayer;

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            handRayDistance,
            combinedLayer,
            QueryTriggerInteraction.Ignore))
        {
            selectedHand = null;
            return;
        }

        ClockHand hand =
            hit.collider.GetComponentInParent<ClockHand>();

        if (hand == null)
        {
            selectedHand = null;
            return;
        }

        if (!clockPuzzle.CanSelectHand(hand))
        {
            selectedHand = null;
            return;
        }

        selectedHand = hand;

        previousMousePosition =
            mousePosition;
    }

    private void HandleHandRotation()
    {
        if (selectedHand == null ||
            Mouse.current == null ||
            clockCamera == null)
            return;

        if (!Mouse.current.leftButton.isPressed)
        {
            selectedHand = null;
            return;
        }

        Vector2 currentMousePosition =
            Mouse.current.position.ReadValue();

        Vector3 handScreenPosition =
            clockCamera.WorldToScreenPoint(
                selectedHand.transform.position
            );

        Vector2 center =
            new Vector2(
                handScreenPosition.x,
                handScreenPosition.y
            );

        Vector2 previousDirection =
            previousMousePosition - center;

        Vector2 currentDirection =
            currentMousePosition - center;

        if (previousDirection.sqrMagnitude < 25f ||
            currentDirection.sqrMagnitude < 25f)
        {
            previousMousePosition =
                currentMousePosition;

            return;
        }

        float previousAngle =
            Mathf.Atan2(
                previousDirection.y,
                previousDirection.x
            ) * Mathf.Rad2Deg;

        float currentAngle =
            Mathf.Atan2(
                currentDirection.y,
                currentDirection.x
            ) * Mathf.Rad2Deg;

        float angleDelta =
            Mathf.DeltaAngle(
                previousAngle,
                currentAngle
            );

        if (invertRotation)
            angleDelta = -angleDelta;

        angleDelta *= rotationSensitivity;

        selectedHand.RotateByAngle(
            angleDelta
        );

        previousMousePosition =
            currentMousePosition;
    }

    private void HandleClockConfirmation()
    {
        if (clockCamera == null ||
            Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        Vector2 mousePosition =
            Mouse.current.position.ReadValue();

        Ray ray =
            clockCamera.ScreenPointToRay(
                mousePosition
            );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            handRayDistance,
            clockLayer,
            QueryTriggerInteraction.Ignore))
        {
            return;
        }

        ClockInteraction clock =
            hit.collider.GetComponentInParent<ClockInteraction>();

        if (clock != null)
        {
            clockPuzzle.ConfirmClock();
        }
    }

    private void OnBackPerformed(
        InputAction.CallbackContext context)
    {
        if (clockPuzzle == null ||
            !clockPuzzle.IsOpen ||
            clockPuzzle.IsSolved)
            return;

        if (UIManager.instance != null &&
            UIManager.instance.IsInventoryOpen)
        {
            UIManager.instance.SetInventory(false);
            return;
        }

        selectedHand = null;

        clockPuzzle.CloseClock();
    }

    private void OnInventoryPerformed(
        InputAction.CallbackContext context)
    {
        if (clockPuzzle == null ||
            !clockPuzzle.IsOpen ||
            clockPuzzle.IsSolved)
            return;

        if (UIManager.instance == null)
            return;

        UIManager.instance.SetInventory(
            !UIManager.instance.IsInventoryOpen
        );
    }
}