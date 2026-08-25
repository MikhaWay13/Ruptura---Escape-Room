using UnityEngine;
using UnityEngine.InputSystem;

public class ClockPuzzle : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ClockCameraController cameraController;
    [SerializeField] private PlayerController playerController;

    [Header("Ponteiros do relógio")]
    [SerializeField] private ClockHand hourHand;
    [SerializeField] private ClockHand minuteHand;

    [Header("Itens do inventário")]
    [SerializeField] private Item hourItem;
    [SerializeField] private Item minuteItem;

    [Header("Inventário")]
    [SerializeField] private InventoryItemSelection itemSelection;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference backAction;
    [SerializeField] private InputActionReference lookAction;

    [Header("Portinha")]
    [SerializeField] private GameObject clockDoor;
    [SerializeField] private Animator clockDoorAnimator;
    [SerializeField] private string openTrigger = "Open";

    public bool IsOpen => isOpen;
    public bool IsSolved => isSolved;

    private InputAction backInput;
    private InputAction lookInput;

    private bool isOpen;
    private bool isSolved;

    private bool hourInserted;
    private bool minuteInserted;

    private ClockHand selectedHand;

    private void Awake()
    {
        if (backAction != null)
            backInput = backAction.action;

        if (lookAction != null)
            lookInput = lookAction.action;
    }

    private void OnEnable()
    {
        if (backInput != null)
            backInput.Enable();

        if (lookInput != null)
            lookInput.Enable();

        if (itemSelection != null)
        {
            itemSelection.OnItemSelected += HandleItemSelected;
        }
    }

    private void OnDisable()
    {
        if (backInput != null)
            backInput.Disable();

        if (lookInput != null)
            lookInput.Disable();

        if (itemSelection != null)
        {
            itemSelection.OnItemSelected -= HandleItemSelected;
        }
    }

    private void Start()
    {
        // Os ponteiros começam escondidos.
        if (hourHand != null)
            hourHand.gameObject.SetActive(false);

        if (minuteHand != null)
            minuteHand.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen || isSolved)
            return;

        // Enquanto estamos no puzzle,
        // o PlayerController não pode reassumir o controle.
        if (playerController != null)
            playerController.SetGameplayControlEnabled(false);

        if (backInput != null &&
            backInput.WasPressedThisFrame())
        {
            CloseClock();
            return;
        }

        HandleClockRotation();
    }

    private void HandleItemSelected(Item item)
    {
        if (!isOpen ||
            isSolved ||
            item == null)
        {
            return;
        }

        TryInsertItem(item);
    }

    public void OpenClock()
    {
        if (isOpen || isSolved)
            return;

        if (cameraController == null)
        {
            Debug.LogWarning(
                "ClockPuzzle: ClockCameraController não configurado."
            );

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

    private void HandleClockRotation()
    {
        if (selectedHand == null ||
            lookInput == null)
        {
            return;
        }

        Vector2 input =
            lookInput.ReadValue<Vector2>();

        if (Mathf.Abs(input.x) < 0.01f)
            return;

        float rotation =
            input.x * 60f * Time.deltaTime;

        selectedHand.Rotate(rotation);

        CheckPuzzle();
    }

    private bool TryInsertItem(Item item)
    {
        if (InventoryController.instance == null)
            return false;

        if (item == hourItem &&
            !hourInserted)
        {
            if (!InventoryController.instance.HasItem(hourItem))
                return false;

            if (!InventoryController.instance.RemoveItem(hourItem))
                return false;

            hourInserted = true;

            hourHand.gameObject.SetActive(true);

            selectedHand = hourHand;

            Debug.Log(
                "Ponteiro de horas colocado no relógio."
            );

            return true;
        }

        if (item == minuteItem &&
            !minuteInserted)
        {
            if (!InventoryController.instance.HasItem(minuteItem))
                return false;

            if (!InventoryController.instance.RemoveItem(minuteItem))
                return false;

            minuteInserted = true;

            minuteHand.gameObject.SetActive(true);

            selectedHand = minuteHand;

            Debug.Log(
                "Ponteiro de minutos colocado no relógio."
            );

            return true;
        }

        return false;
    }

    private void CheckPuzzle()
    {
        if (isSolved ||
            !hourInserted ||
            !minuteInserted)
        {
            return;
        }

        if (hourHand == null ||
            minuteHand == null)
        {
            return;
        }

        int hour =
            hourHand.GetHour();

        int minute =
            minuteHand.GetMinute();

        if (hour == 10 &&
            minute == 35)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        if (isSolved)
            return;

        isSolved = true;
        selectedHand = null;

        Debug.Log(
            "RELÓGIO RESOLVIDO: 10:35"
        );

        if (clockDoorAnimator != null)
        {
            clockDoorAnimator.SetTrigger(
                openTrigger
            );
        }
        else if (clockDoor != null)
        {
            clockDoor.SetActive(false);
        }
    }
}