using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class ClockPuzzle : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ClockCameraController cameraController;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private ClockInventoryBridge inventoryBridge;

    [Header("Ponteiros")]
    [SerializeField] private ClockHand hourHand;
    [SerializeField] private ClockHand minuteHand;

    [Header("Itens do inventário")]
    [SerializeField] private Item hourItem;
    [SerializeField] private Item minuteItem;

    [Header("Câmera")]
    [SerializeField] private Camera clockCamera;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference pressAction;
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private InputActionReference lookAction;

    [Header("Raycast dos ponteiros")]
    [SerializeField] private float handRayDistance = 3f;
    [SerializeField] private LayerMask handLayer = ~0;

    [Header("Horário correto")]
    [SerializeField, Range(1, 12)] private int targetHour = 10;
    [SerializeField, Range(0, 59)] private int targetMinute = 35;

    [Header("Tolerância")]
    [SerializeField, Range(0f, 10f)] private float hourTolerance = 0.5f;
    [SerializeField, Range(0f, 5f)] private float minuteTolerance = 0.5f;

    [Header("Portinha")]
    [SerializeField] private Transform clockDoor;
    [SerializeField] private Vector3 doorOpenAxis = Vector3.right;
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField] private float doorOpenDuration = 0.5f;

    [Header("Espelho")]
    [SerializeField] private Collider mirrorCollider;

    private InputAction pressInput;
    private InputAction backInput;
    private InputAction lookInput;

    private bool isOpen;
    private bool isSolved;
    private bool hourInserted;
    private bool minuteInserted;

    private ClockHand selectedHand;

    public bool IsOpen => isOpen;
    public bool IsSolved => isSolved;

    private void Awake()
    {
        if (pressAction != null)
            pressInput = pressAction.action;

        if (backAction != null)
            backInput = backAction.action;

        if (lookAction != null)
            lookInput = lookAction.action;

        if (mirrorCollider != null)
            mirrorCollider.enabled = false;
    }

    private void OnEnable()
    {
        if (pressInput != null)
            pressInput.Enable();

        if (backInput != null)
            backInput.Enable();

        if (lookInput != null)
            lookInput.Enable();

        if (inventoryBridge != null)
            inventoryBridge.OnItemSelected += HandleItemSelected;
    }

    private void OnDisable()
    {
        if (pressInput != null)
            pressInput.Disable();

        if (backInput != null)
            backInput.Disable();

        if (lookInput != null)
            lookInput.Disable();

        if (inventoryBridge != null)
            inventoryBridge.OnItemSelected -= HandleItemSelected;
    }

    private void Start()
    {
        if (hourHand != null)
            hourHand.gameObject.SetActive(false);

        if (minuteHand != null)
            minuteHand.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen || isSolved)
            return;

        if (playerController != null)
            playerController.SetGameplayControlEnabled(false);

        if (backInput != null && backInput.WasPressedThisFrame())
        {
            CloseClock();
            return;
        }

        HandleHandSelection();
        HandleHandRotation();
        CheckPuzzle();
    }

    public void OpenClock()
    {
        if (isOpen || isSolved)
            return;

        if (cameraController == null)
        {
            Debug.LogWarning("ClockPuzzle: ClockCameraController não configurado.");
            return;
        }

        isOpen = true;

        if (playerController != null)
            playerController.SetGameplayControlEnabled(false);

        cameraController.EnterClockView();

        Debug.Log("Relógio aberto.");
    }

    public void CloseClock()
    {
        if (!isOpen)
            return;

        isOpen = false;
        selectedHand = null;

        if (cameraController != null)
            cameraController.ExitClockView();

        if (playerController != null)
            playerController.SetGameplayControlEnabled(true);
    }

    private void HandleItemSelected(Item item)
    {
        if (!isOpen || isSolved || item == null)
            return;

        if (item == hourItem && !hourInserted)
        {
            InsertHourHand();
            return;
        }

        if (item == minuteItem && !minuteInserted)
            InsertMinuteHand();
    }

    private void InsertHourHand()
    {
        if (hourHand == null || InventoryController.instance == null)
            return;

        if (!InventoryController.instance.HasItem(hourItem))
            return;

        if (!InventoryController.instance.RemoveItem(hourItem))
            return;

        hourInserted = true;
        hourHand.gameObject.SetActive(true);
        hourHand.SetAngle(0f);
        selectedHand = hourHand;

        CloseInventory();
    }

    private void InsertMinuteHand()
    {
        if (minuteHand == null || InventoryController.instance == null)
            return;

        if (!InventoryController.instance.HasItem(minuteItem))
            return;

        if (!InventoryController.instance.RemoveItem(minuteItem))
            return;

        minuteInserted = true;
        minuteHand.gameObject.SetActive(true);
        minuteHand.SetAngle(0f);
        selectedHand = minuteHand;

        CloseInventory();
    }

    private void CloseInventory()
    {
        if (UIManager.instance != null)
            UIManager.instance.SetInventory(false);
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

        if (hand == hourHand && hourInserted)
        {
            selectedHand = hourHand;
            return;
        }

        if (hand == minuteHand && minuteInserted)
        {
            selectedHand = minuteHand;
            return;
        }

        selectedHand = null;
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

    private void CheckPuzzle()
    {
        if (isSolved || !hourInserted || !minuteInserted)
            return;

        if (hourHand == null || minuteHand == null)
            return;

        float actualHour = hourHand.GetHourValue();
        float actualMinute = minuteHand.GetMinute();
        float targetHourValue = (targetHour % 12) + (targetMinute / 60f);
        float actualHourAngle = actualHour * 30f;
        float targetHourAngle = targetHourValue * 30f;
        float minuteAngle = actualMinute * 6f;
        float targetMinuteAngle = targetMinute * 6f;

        float hourDifference = Mathf.Abs(Mathf.DeltaAngle(actualHourAngle, targetHourAngle));
        float minuteDifference = Mathf.Abs(Mathf.DeltaAngle(minuteAngle, targetMinuteAngle));

        if (hourDifference <= hourTolerance * 30f && minuteDifference <= minuteTolerance * 6f)
            SolvePuzzle();
    }

    private void SolvePuzzle()
    {
        if (isSolved)
            return;

        isSolved = true;
        selectedHand = null;

        Debug.Log("RELÓGIO RESOLVIDO: 10:35");

        if (clockDoor != null)
            StartCoroutine(OpenClockDoor());

        if (mirrorCollider != null)
            mirrorCollider.enabled = true;


    }
    private IEnumerator OpenClockDoor()
    {
        Quaternion startRotation = clockDoor.localRotation;
        Quaternion targetRotation = startRotation * Quaternion.AngleAxis(doorOpenAngle, doorOpenAxis.normalized);
        float timer = 0f;

        while (timer < doorOpenDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Clamp01(timer / doorOpenDuration);
            progress = Mathf.SmoothStep(0f, 1f, progress);

            clockDoor.localRotation = Quaternion.Slerp(startRotation, targetRotation, progress);

            yield return null;
        }

        clockDoor.localRotation = targetRotation;
    }
}