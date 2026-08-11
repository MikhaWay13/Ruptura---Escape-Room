using UnityEngine;
using UnityEngine.InputSystem;

public class InspectableItem : MonoBehaviour, IInteractable
{
    [Header("Inspeção")]
    [SerializeField] private Transform inspectionPoint;

    [SerializeField, Min(0.01f)]
    private float rotationSpeed = 0.2f;

    [Header("Inventário")]
    [SerializeField] private bool addToInventory;
    [SerializeField] private InventoryItemData inventoryItem;

    [Header("Diálogo")]
    [SerializeField] private DialogueData dialogue;

    [Header("Textos")]
    [SerializeField] private string inspectPrompt =
        "E - Inspecionar";

    [SerializeField] private string finishPrompt =
        "E - Devolver";

    [SerializeField] private string inventoryPrompt =
        "E - Guardar";

    private Transform originalParent;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private bool isInspecting;
    private bool dialogueFinished;

    public bool IsInspecting => isInspecting;

    public string InspectPrompt => inspectPrompt;

    public string FinishPrompt =>
        addToInventory
            ? inventoryPrompt
            : finishPrompt;

    private void Update()
    {
        if (!isInspecting)
            return;

        if (dialogueFinished)
            return;

        if (dialogue == null)
        {
            dialogueFinished = true;
            return;
        }

        if (DialogueManager.Instance == null)
            return;

        if (!DialogueManager.Instance.IsDialogueOpen)
        {
            dialogueFinished = true;
        }
    }

    public void Interact()
    {
        if (isInspecting)
            return;

        StartInspection();
    }

    private void StartInspection()
    {
        if (inspectionPoint == null)
        {
            Debug.LogWarning(
                $"'{gameObject.name}' não possui " +
                "um Inspection Point."
            );

            return;
        }

        originalParent = transform.parent;
        originalPosition = transform.position;
        originalRotation = transform.rotation;

        isInspecting = true;

        dialogueFinished = dialogue == null;

        transform.SetParent(inspectionPoint);

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        if (dialogue != null &&
            DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(dialogue);
        }
    }

    public void Rotate(
        InputAction.CallbackContext context)
    {
        if (!isInspecting)
            return;

        Vector2 mouseDelta =
            context.ReadValue<Vector2>();

        transform.Rotate(
            Vector3.up,
            -mouseDelta.x * rotationSpeed,
            Space.World
        );

        transform.Rotate(
            Vector3.right,
            mouseDelta.y * rotationSpeed,
            Space.Self
        );
    }

    public void FinishInspection()
    {
        if (!isInspecting)
            return;

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueOpen)
        {
            return;
        }

        if (!dialogueFinished)
            return;

        if (addToInventory)
        {
            AddToInventory();
        }
        else
        {
            ReturnToOriginalPosition();
        }
    }

    private void AddToInventory()
    {
        if (inventoryItem == null)
        {
            Debug.LogWarning(
                $"'{gameObject.name}' está configurado " +
                "para inventário, mas não possui " +
                "InventoryItemData."
            );

            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogWarning(
                "Nenhum Inventory foi encontrado na cena."
            );

            return;
        }

        bool added =
            Inventory.Instance.AddItem(
                inventoryItem
            );

        if (!added)
            return;

        isInspecting = false;

        transform.SetParent(null);

        gameObject.SetActive(false);
    }

    private void ReturnToOriginalPosition()
    {
        isInspecting = false;

        transform.SetParent(originalParent);

        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }
}