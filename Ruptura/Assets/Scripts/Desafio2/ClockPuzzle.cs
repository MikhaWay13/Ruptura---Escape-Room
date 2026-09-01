using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClockPuzzle : MonoBehaviour
{
    public static ClockPuzzle instance;

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

    [Header("Horário correto - Rotação Z")]
    [SerializeField] private float hourMinZ = -144.399f;
    [SerializeField] private float hourMaxZ = -126.156f;

    [SerializeField] private float minuteMinZ = -57.964f;
    [SerializeField] private float minuteMaxZ = -40.264f;

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

    private ClockHand selectedHand;

    public bool IsOpen => isOpen;
    public bool IsSolved => isSolved;

    private void Awake()
    {
        instance = this;

        if (mirrorCollider != null)
            mirrorCollider.enabled = false;
    }

    private void OnEnable()
    {
        if (inventoryBridge != null)
            inventoryBridge.OnItemSelected += HandleItemSelected;
    }

    private void OnDisable()
    {
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
    }

    // =========================================================
    // ABRIR RELÓGIO
    // =========================================================

    public void OpenClock()
    {
        if (isOpen || isSolved)
            return;

        if (cameraController == null)
            return;

        InputActionMap interactionMap =
            InputSystem.actions.FindActionMap("Interaction");

        if (interactionMap != null)
            interactionMap.Enable();

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

        selectedHand = null;

        if (UIManager.instance != null)
            UIManager.instance.SetInventory(false);

        if (cameraController != null)
            cameraController.ExitClockView();

        if (playerController != null)
            playerController.SetGameplayControlEnabled(true);
    }


    private void HandleItemSelected(Item item)
    {
        if (!isOpen ||
            isSolved ||
            item == null)
            return;

        if (item == hourItem &&
            !hourInserted)
        {
            InsertHourHand();
            return;
        }

        if (item == minuteItem &&
            !minuteInserted)
        {
            InsertMinuteHand();
        }
    }

    private void InsertHourHand()
    {
        if (hourHand == null)
            return;

        if (InventoryController.instance == null)
            return;

        if (!InventoryController.instance.HasItem(hourItem))
            return;

        if (!InventoryController.instance.RemoveItem(hourItem))
            return;

        hourInserted = true;

        hourHand.SetAngle(0f);
        hourHand.gameObject.SetActive(true);

        selectedHand = hourHand;

        CloseInventory();
    }

    private void InsertMinuteHand()
    {
        if (minuteHand == null)
            return;

        if (InventoryController.instance == null)
            return;

        if (!InventoryController.instance.HasItem(minuteItem))
            return;

        if (!InventoryController.instance.RemoveItem(minuteItem))
            return;

        minuteInserted = true;

        minuteHand.SetAngle(0f);
        minuteHand.gameObject.SetActive(true);

        selectedHand = minuteHand;

        CloseInventory();
    }

    private void CloseInventory()
    {
        if (UIManager.instance != null)
            UIManager.instance.SetInventory(false);
    }

    public void InsertItemFromSlot(int index)
    {
        if (!isOpen ||
            isSolved)
            return;

        if (InventoryController.instance == null)
            return;

        Item item =
            InventoryController.instance.GetItemAtSlot(index);

        if (item == null)
            return;

        if (item == hourItem &&
            !hourInserted)
        {
            InsertHourHand();
            return;
        }

        if (item == minuteItem &&
            !minuteInserted)
        {
            InsertMinuteHand();
        }
    }

    public bool CanSelectHand(ClockHand hand)
    {
        if (!isOpen ||
            isSolved ||
            hand == null)
            return false;

        if (hand == hourHand &&
            hourInserted)
            return true;

        if (hand == minuteHand &&
            minuteInserted)
            return true;

        return false;
    }


    public void ConfirmClock()
    {
        if (!isOpen ||
            isSolved)
            return;

        if (!hourInserted ||
            !minuteInserted)
            return;

        if (hourHand == null ||
            minuteHand == null)
            return;

        float hourZ =
            NormalizeZ(
                hourHand.transform.localEulerAngles.z
            );

        float minuteZ =
            NormalizeZ(
                minuteHand.transform.localEulerAngles.z
            );

        bool hourCorrect =
            hourZ >= hourMinZ &&
            hourZ <= hourMaxZ;

        bool minuteCorrect =
            minuteZ >= minuteMinZ &&
            minuteZ <= minuteMaxZ;

        if (hourCorrect &&
            minuteCorrect)
        {
            SolvePuzzle();
        }
    }

    private float NormalizeZ(float z)
    {
        if (z > 180f)
            z -= 360f;

        return z;
    }



    private void SolvePuzzle()
    {
        if (isSolved)
            return;

        isSolved = true;

        selectedHand = null;

        if (clockDoor != null)
            StartCoroutine(OpenClockDoor());

        if (mirrorCollider != null)
            mirrorCollider.enabled = true;
    }
    private IEnumerator OpenClockDoor()
    {
        Quaternion startRotation =
            clockDoor.localRotation;

        Quaternion targetRotation =
            startRotation *
            Quaternion.AngleAxis(
                doorOpenAngle,
                doorOpenAxis.normalized
            );

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
    }
}