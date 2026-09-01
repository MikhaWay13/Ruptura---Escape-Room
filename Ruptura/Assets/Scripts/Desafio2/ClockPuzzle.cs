using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


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


    private bool isOpen;
    private bool isSolved;
    private bool hourInserted;
    private bool minuteInserted;


    public bool IsOpen => isOpen;
    public bool IsSolved => isSolved;


    private void Awake()
    {
        instance = this;


        if (mirrorCollider != null)
            mirrorCollider.enabled = false;
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
    }


    public void OpenClock()
    {
        if (isOpen || isSolved)
            return;


        if (cameraController == null)
            return;


        InputActionMap interactionMap = InputSystem.actions.FindActionMap("Interaction");


        if (interactionMap != null)
            interactionMap.Enable();


        isOpen = true;


        if (playerController != null)
            playerController.SetGameplayControlEnabled(false);


        cameraController.EnterClockView();
    }


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


    public bool CanSelectHand(ClockHand hand)
    {
        if (hand == null)
            return false;


        if (hand == hourHand && hourInserted)
            return true;


        if (hand == minuteHand && minuteInserted)
            return true;


        return false;
    }


    public void InsertItemFromSlot(int index)
    {
        if (!isOpen || isSolved)
            return;


        if (InventoryController.instance == null)
            return;


        Item item = InventoryController.instance.GetItemAtSlot(index);


        if (item == null)
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
        hourHand.SetAngle(0f);
        hourHand.gameObject.SetActive(true);


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
        minuteHand.SetAngle(0f);
        minuteHand.gameObject.SetActive(true);


        CloseInventory();
    }


    private void CloseInventory()
    {
        if (UIManager.instance != null)
            UIManager.instance.SetInventory(false);
    }

    public void CheckClock()
{
    if (!isOpen || isSolved)
        return;

    if (!hourInserted || !minuteInserted)
        return;

    if (hourHand == null || minuteHand == null)
        return;

    int actualMinute = minuteHand.GetMinute();
    float actualHour = hourHand.GetHourValue();

    float targetHour = (this.targetHour % 12) + (this.targetMinute / 60f);

    float hourDifference = Mathf.Abs(Mathf.DeltaAngle(actualHour * 30f, targetHour * 30f));
    float minuteDifference = Mathf.Abs(Mathf.DeltaAngle(actualMinute * 6f, this.targetMinute * 6f));

    bool hourCorrect = hourDifference <= hourTolerance * 30f;
    bool minuteCorrect = minuteDifference <= minuteTolerance * 6f;

    if (hourCorrect && minuteCorrect)
        SolvePuzzle();
}

    private void SolvePuzzle()
    {
        if (isSolved)
            return;


        isSolved = true;


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
