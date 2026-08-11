using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction2 : MonoBehaviour
{
    [Header("Input System")]
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference inspectRotateAction;

    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] private GameObject interactionUI;
    [SerializeField] private TMP_Text interactionText;

    [Header("Textos")]
    [SerializeField] private string defaultInteractionPrompt = "E - Interagir";

    private IInteractable currentInteractable;
    private InspectableItem currentInspectable;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        HideInteractionUI();
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
            interactAction.action.performed += OnInteract;
        }

        if (inspectRotateAction != null)
        {
            inspectRotateAction.action.Enable();
            inspectRotateAction.action.performed += OnInspectRotate;
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }

        if (inspectRotateAction != null)
        {
            inspectRotateAction.action.performed -= OnInspectRotate;
            inspectRotateAction.action.Disable();
        }
    }

    private void Update()
    {
        if (currentInspectable != null &&
            currentInspectable.IsInspecting)
        {
            UpdateInspectionUI();
            return;
        }

        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        currentInteractable = null;
        currentInspectable = null;

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueOpen)
        {
            HideInteractionUI();
            return;
        }

        if (playerCamera == null)
            return;

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (!Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactableLayer,
            QueryTriggerInteraction.Ignore))
        {
            HideInteractionUI();
            return;
        }

        currentInteractable =
            hit.collider.GetComponentInParent<IInteractable>();

        currentInspectable =
            hit.collider.GetComponentInParent<InspectableItem>();

        if (currentInteractable == null)
        {
            HideInteractionUI();
            return;
        }

        if (currentInspectable != null)
        {
            ShowInteractionUI(
                currentInspectable.InspectPrompt
            );
        }
        else
        {
            ShowInteractionUI(
                defaultInteractionPrompt
            );
        }
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        Debug.Log("E FOI APERTADO!");

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueOpen)
        {
            DialogueManager.Instance.ContinueDialogue();
            return;
        }

        if (currentInspectable != null &&
            currentInspectable.IsInspecting)
        {
            Debug.Log("Finalizando inspeção.");

            currentInspectable.FinishInspection();
            return;
        }

        if (currentInteractable != null)
        {
            Debug.Log(
                "Interagindo com: " +
                ((MonoBehaviour)currentInteractable).gameObject.name
            );

            currentInteractable.Interact();
        }
        else
        {
            Debug.Log("Nenhum objeto interativo encontrado.");
        }
    }

    private void OnInspectRotate(
        InputAction.CallbackContext context)
    {
        if (currentInspectable == null)
            return;

        if (!currentInspectable.IsInspecting)
            return;

        currentInspectable.Rotate(context);
    }

    private void UpdateInspectionUI()
    {
        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueOpen)
        {
            HideInteractionUI();
            return;
        }

        if (currentInspectable != null)
        {
            ShowInteractionUI(
                currentInspectable.FinishPrompt
            );
        }
    }

    private void ShowInteractionUI(string text)
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
        }

        if (interactionText != null)
        {
            interactionText.text = text;
        }
    }

    private void HideInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(false);
        }
    }
}