using UnityEngine;
using UnityEngine.InputSystem;

public class ClockPuzzle : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private ClockCameraController cameraController;
    [SerializeField] private PlayerController playerController;

    [Header("Ponteiros")]
    [SerializeField] private ClockHand hourHand;
    [SerializeField] private ClockHand minuteHand;

    [Header("Itens")]
    [SerializeField] private Item hourItem;
    [SerializeField] private Item minuteItem;

    [Header("Visuais dos ponteiros")]
    [SerializeField] private GameObject hourVisualPrefab;
    [SerializeField] private GameObject minuteVisualPrefab;

    [Header("Pontos de encaixe")]
    [SerializeField] private Transform hourPivot;
    [SerializeField] private Transform minutePivot;

    [Header("Câmera / Saída")]
    [SerializeField] private InputActionReference backAction;

    [Header("Resposta")]
    [SerializeField] private GameObject clockDoor;

    [SerializeField] private Animator clockDoorAnimator;
    [SerializeField] private string openTrigger = "Open";

    [Header("Inventário")]
    [SerializeField] private InventoryItemSelection itemSelection;
    public bool IsOpen => isOpen;

    private InputAction backInput;

    private bool isOpen;
    private bool hourInserted;
    private bool minuteInserted;

    private GameObject hourVisual;
    private GameObject minuteVisual;


    private void Awake()
    {
        if (backAction != null)
            backInput = backAction.action;
    }

    private void OnEnable()
    {
        if (backInput != null)
        {
            backInput.Enable();
        }

        if (itemSelection != null)
        {
            itemSelection.OnItemSelected += HandleItemSelected;
        }
    }

    private void OnDisable()
    {
        if (backInput != null)
        {
            backInput.Disable();
        }

        if (itemSelection != null)
        {
            itemSelection.OnItemSelected -= HandleItemSelected;
        }
    }

    private void HandleItemSelected(Item item)
    {
        if (!isOpen)
            return;

        if (item == null)
            return;

        TryInsertItem(item);
    }

    private void Update()
    {
        if (!isOpen)
            return;

        if (backInput != null &&
            backInput.WasPressedThisFrame())
        {
            CloseClock();
        }

        HandleClockRotation();
    }

    public void OpenClock()
    {
        if (isOpen)
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

        cameraController.ExitClockView();

        if (playerController != null)
            playerController.SetGameplayControlEnabled(true);
    }

    private void HandleClockRotation()
    {
        if (hourInserted == false &&
            minuteInserted == false)
            return;

        InputAction lookAction =
            InputSystem.actions.FindAction(
                "Interaction/Look"
            );

        if (lookAction == null)
            return;

        Vector2 input =
            lookAction.ReadValue<Vector2>();

        if (Mathf.Abs(input.x) < 0.01f)
            return;

        float rotation =
            input.x * 60f * Time.deltaTime;

        if (minuteInserted)
            minuteHand.Rotate(rotation);

        CheckPuzzle();
    }

    public bool TryInsertItem(Item item)
    {
        if (item == null)
            return false;

        if (item == hourItem &&
            !hourInserted)
        {
            if (!InventoryController.instance.HasItem(hourItem))
                return false;

            if (!InventoryController.instance.RemoveItem(hourItem))
                return false;

            InsertHourHand();

            return true;
        }

        if (item == minuteItem &&
            !minuteInserted)
        {
            if (!InventoryController.instance.HasItem(minuteItem))
                return false;

            if (!InventoryController.instance.RemoveItem(minuteItem))
                return false;

            InsertMinuteHand();

            return true;
        }

        return false;
    }

    private void InsertHourHand()
    {
        hourInserted = true;

        if (hourVisualPrefab != null &&
            hourPivot != null)
        {
            hourVisual =
                Instantiate(
                    hourVisualPrefab,
                    hourPivot
                );

            hourVisual.transform.localPosition =
                Vector3.zero;

            hourVisual.transform.localRotation =
                Quaternion.identity;
        }
    }

    private void InsertMinuteHand()
    {
        minuteInserted = true;

        if (minuteVisualPrefab != null &&
            minutePivot != null)
        {
            minuteVisual =
                Instantiate(
                    minuteVisualPrefab,
                    minutePivot
                );

            minuteVisual.transform.localPosition =
                Vector3.zero;

            minuteVisual.transform.localRotation =
                Quaternion.identity;
        }
    }

    private void CheckPuzzle()
    {
        if (!hourInserted ||
            !minuteInserted)
            return;

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
        Debug.Log("RELÓGIO RESOLVIDO: 10:35");

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