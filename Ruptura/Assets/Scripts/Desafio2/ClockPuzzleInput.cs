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

    [Header("Raycast")]
    [SerializeField] private float handRayDistance = 5f;

    [Header("Rotação")]
    [SerializeField] private float rotationSensitivity = 1f;

    private InputAction pressInput;
    private InputAction backInput;
    private InputAction inventoryInput;
    private InputAction confirmInput;

    private ClockHand selectedHand;
    private bool cursorUnlocked;

    private void Awake()
    {
        pressInput = InputSystem.actions.FindAction("Interaction/Press");
        backInput = InputSystem.actions.FindAction("Interaction/Back");
        inventoryInput = InputSystem.actions.FindAction("Player/Inventory");
        confirmInput = InputSystem.actions.FindAction("Interaction/ClockConfirm");
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

        RestoreCursor();
    }

    private void Update()
    {
        if (clockPuzzle == null || !clockPuzzle.IsOpen || clockPuzzle.IsSolved)
        {
            selectedHand = null;
            RestoreCursor();
            return;
        }

        UnlockCursor();
        HandleHandSelection();
        HandleHandRotation();
        HandleClockConfirmation();
    }

    private void UnlockCursor()
    {
        if (cursorUnlocked)
            return;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        cursorUnlocked = true;
    }

    private void RestoreCursor()
    {
        if (!cursorUnlocked)
            return;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        cursorUnlocked = false;
    }

    private void OnBackPerformed(InputAction.CallbackContext context)
    {
        if (clockPuzzle == null || !clockPuzzle.IsOpen || clockPuzzle.IsSolved)
            return;

        if (UIManager.instance != null && UIManager.instance.IsInventoryOpen)
        {
            UIManager.instance.SetInventory(false);
            return;
        }

        selectedHand = null;
        clockPuzzle.CloseClock();
    }

    private void OnInventoryPerformed(InputAction.CallbackContext context)
    {
        if (clockPuzzle == null || !clockPuzzle.IsOpen || clockPuzzle.IsSolved)
            return;

        if (UIManager.instance == null)
            return;

        UIManager.instance.SetInventory(!UIManager.instance.IsInventoryOpen);
    }

    private void HandleClockConfirmation()
    {
        if (confirmInput == null)
            return;

        if (!confirmInput.WasPressedThisFrame())
            return;

        clockPuzzle.CheckClock();
    }

    private void HandleHandSelection()
    {
        if (pressInput == null || clockCamera == null || Mouse.current == null)
            return;

        if (!pressInput.WasPressedThisFrame())
            return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = clockCamera.ScreenPointToRay(mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hourHit, handRayDistance, hourHandLayer, QueryTriggerInteraction.Ignore))
        {
            ClockHand hand = hourHit.collider.GetComponentInParent<ClockHand>();

            if (hand != null && clockPuzzle.CanSelectHand(hand))
            {
                selectedHand = hand;
                return;
            }
        }

        if (Physics.Raycast(ray, out RaycastHit minuteHit, handRayDistance, minuteHandLayer, QueryTriggerInteraction.Ignore))
        {
            ClockHand hand = minuteHit.collider.GetComponentInParent<ClockHand>();

            if (hand != null && clockPuzzle.CanSelectHand(hand))
            {
                selectedHand = hand;
                return;
            }
        }

        selectedHand = null;
    }

    private void HandleHandRotation()
    {
        if (selectedHand == null || pressInput == null || Mouse.current == null)
            return;

        if (!pressInput.IsPressed())
        {
            selectedHand = null;
            return;
        }

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        if (mouseDelta.sqrMagnitude < 0.001f)
            return;

        selectedHand.RotateFromMouse(mouseDelta.x * rotationSensitivity);
    }
}