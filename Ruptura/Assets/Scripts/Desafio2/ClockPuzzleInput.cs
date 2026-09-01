using UnityEngine;
using UnityEngine.InputSystem;


public class ClockPuzzleInput : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ClockPuzzle clockPuzzle;
    [SerializeField] private Camera clockCamera;


    [Header("Raycast dos ponteiros")]
    [SerializeField] private float handRayDistance = 3f;
    [SerializeField] private LayerMask handLayer = ~0;


    private InputAction pressInput;
    private InputAction lookInput;
    private InputAction backInput;
    private InputAction inventoryInput;


    private ClockHand selectedHand;


    private void Awake()
    {
        pressInput = InputSystem.actions.FindAction("Interaction/Press");
        lookInput = InputSystem.actions.FindAction("Interaction/Look");
        backInput = InputSystem.actions.FindAction("Interaction/Back");
        inventoryInput = InputSystem.actions.FindAction("Player/Inventory");
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
    }


    private void Update()
    {
        if (clockPuzzle == null || !clockPuzzle.IsOpen || clockPuzzle.IsSolved)
            return;


        HandleHandSelection();
        HandleHandRotation();
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


    private void HandleHandSelection()
    {
        if (pressInput == null || clockCamera == null)
            return;


        if (!pressInput.WasPressedThisFrame())
            return;


        Ray ray = new Ray(clockCamera.transform.position, clockCamera.transform.forward);


        if (!Physics.Raycast(ray, out RaycastHit hit, handRayDistance, handLayer, QueryTriggerInteraction.Ignore))
        {
            selectedHand = null;
            return;
        }


        ClockHand hand = hit.collider.GetComponentInParent<ClockHand>();


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
        if (selectedHand == null || pressInput == null || lookInput == null)
            return;


        if (!pressInput.IsPressed())
            return;


        Vector2 mouseDelta = lookInput.ReadValue<Vector2>();


        if (Mathf.Abs(mouseDelta.x) < 0.001f)
            return;


        selectedHand.RotateFromMouse(mouseDelta.x);
    }
}
