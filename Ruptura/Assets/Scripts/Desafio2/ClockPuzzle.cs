using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class ClockPuzzle : MonoBehaviour
{
    public static ClockPuzzle instance;

    [Header("Referências")]
    [SerializeField] private ClockCameraController cameraController;
    [SerializeField] private PlayerController playerController;

    [Header("Ponteiros")]
    [SerializeField] private ClockHand hourHand;
    [SerializeField] private ClockHand minuteHand;

    [Header("Itens do inventário")]
    [SerializeField] private Item hourItem;
    [SerializeField] private Item minuteItem;

    [Header("Horário correto")]
    [SerializeField] private Transform correctHour;
    [SerializeField] private Transform correctMinute;
    [SerializeField, Min(0f)] private float hourTolerance = 5f;
    [SerializeField, Min(0f)] private float minuteTolerance = 5f;

    [Header("Portinha")]
    [SerializeField] private Transform clockDoor;
    [SerializeField] private Vector3 doorOpenAxis = Vector3.right;
    [SerializeField] private float doorOpenAngle = 90f;
    [SerializeField, Min(0f)] private float doorOpenDuration = 0.5f;

    [Header("Recompensa")]
    [SerializeField] private Collider mirrorCollider;

    [Header("Feedback")]
    [SerializeField] private UnityEvent onSolved;

    private bool isOpen;
    private bool isSolved;

    private bool hourInserted;
    private bool minuteInserted;

    private Quaternion closedDoorLocalRotation;
    private Coroutine doorOpeningCoroutine;

    public bool IsOpen => isOpen;
    public bool IsSolved => isSolved;

    private void Awake()
    {
        instance = this;

        if (mirrorCollider != null)
            mirrorCollider.enabled = false;

        if (clockDoor != null)
            closedDoorLocalRotation = clockDoor.localRotation;
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
        if (isOpen && !isSolved && playerController != null)
            playerController.SetGameplayControlEnabled(false);
    }

    // =========================================================
    // ABRIR RELÓGIO
    // =========================================================

    public void OpenClock()
    {
        if (isOpen || isSolved || cameraController == null)
            return;

        isOpen = true;

        if (playerController != null)
            playerController.SetGameplayControlEnabled(false);

        cameraController.EnterClockView();
    }

    // =========================================================
    // FECHAR RELÓGIO
    // =========================================================

    public void CloseClock()
    {
        if (!isOpen)
            return;

        isOpen = false;

        if (UIManager.instance != null)
            UIManager.instance.SetInventory(false);

        if (cameraController != null)
            cameraController.ExitClockView();

        if (playerController != null)
            playerController.SetGameplayControlEnabled(true);
    }

    public void InsertItemFromSlot(int index)
    {
        if (!isOpen || isSolved || InventoryController.instance == null)
            return;

        Item item = InventoryController.instance.GetItemAtSlot(index);

        if (item == hourItem && !hourInserted)
        {
            InsertHand(hourHand, hourItem, true);
            return;
        }

        if (item == minuteItem && !minuteInserted)
            InsertHand(minuteHand, minuteItem, false);
    }

    private void InsertHand(ClockHand hand, Item item, bool isHourHand)
    {
        if (hand == null || item == null || InventoryController.instance == null)
            return;

        if (!InventoryController.instance.HasItem(item) ||
            !InventoryController.instance.RemoveItem(item))
            return;

        if (isHourHand)
            hourInserted = true;
        else
            minuteInserted = true;

        hand.ResetRotation();
        hand.gameObject.SetActive(true);

        if (UIManager.instance != null)
            UIManager.instance.SetInventory(false);
    }

    public bool CanSelectHand(ClockHand hand)
    {
        if (!isOpen || isSolved || hand == null)
            return false;

        return (hand == hourHand && hourInserted) ||
               (hand == minuteHand && minuteInserted);
    }


    public void ConfirmClock()
    {
        if (!isOpen || isSolved || !hourInserted || !minuteInserted)
            return;

        bool hourCorrect = hourHand != null &&
                           hourHand.IsAlignedWith(correctHour, hourTolerance);

        bool minuteCorrect = minuteHand != null &&
                             minuteHand.IsAlignedWith(correctMinute, minuteTolerance);

        if (hourCorrect &&
            minuteCorrect)
        {
            SolvePuzzle();
        }
    }

    private void SolvePuzzle()
    {
        if (isSolved)
            return;

        isSolved = true;

        onSolved?.Invoke();

        if (clockDoor != null && doorOpeningCoroutine == null)
            doorOpeningCoroutine = StartCoroutine(OpenClockDoor());

        if (mirrorCollider != null)
            mirrorCollider.enabled = true;
    }
    private IEnumerator OpenClockDoor()
    {
        Quaternion startRotation = clockDoor.localRotation;

        Quaternion targetRotation =
            Quaternion.AngleAxis(
                doorOpenAngle,
                doorOpenAxis.normalized
            ) * closedDoorLocalRotation;

        float timer = 0f;

        while (timer < doorOpenDuration)
        {
            timer += Time.deltaTime;

            float progress =
                Mathf.Clamp01(
                    timer / doorOpenDuration
                );

            progress =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    progress
                );

            clockDoor.localRotation =
                Quaternion.Slerp(
                    startRotation,
                    targetRotation,
                    progress
                );

            yield return null;
        }

        clockDoor.localRotation =
            targetRotation;

        doorOpeningCoroutine = null;
    }
}
