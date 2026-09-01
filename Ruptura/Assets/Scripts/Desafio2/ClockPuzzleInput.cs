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

    private InputAction backInput;
    private InputAction inventoryInput;
    private InputAction checkInput;

    private ClockHand selectedHand;

    private void Awake()
    {
        backInput = InputSystem.actions.FindAction("Interaction/Back");
        inventoryInput = InputSystem.actions.FindAction("Player/Inventory");
        checkInput = InputSystem.actions.FindAction("Interaction/Press");
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

        Cursor.lockState = CursorLockMode.Locked;
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

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        HandleHandSelection();
        HandleHandRotation();

        if (checkInput != null &&
            checkInput.WasPressedThisFrame())
        {
            clockPuzzle.CheckClock();
        }
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
            clockCamera.ScreenPointToRay(mousePosition);

        LayerMask combinedLayer =
            hourHandLayer | minuteHandLayer;

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

        Vector2 mousePosition =
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

        Vector2 direction =
            mousePosition - center;

        if (direction.sqrMagnitude < 4f)
            return;

        float mouseAngle =
            Mathf.Atan2(
                direction.y,
                direction.x
            ) * Mathf.Rad2Deg;

        float desiredAngle =
            mouseAngle - 90f;

        selectedHand.SetAngle(
            desiredAngle
        );
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