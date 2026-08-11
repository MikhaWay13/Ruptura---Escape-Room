using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction2 : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float interactionDistance = 3f;

    [Header("Interação")]
    [SerializeField] private LayerMask interactableLayer;

    [Header("UI")]
    [SerializeField] private GameObject interactionUI;

    private IInteractable currentInteractable;

    private void Awake()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        HideInteractionUI();
    }

    private void Update()
    {
        CheckForInteractable();
    }

    private void CheckForInteractable()
    {
        currentInteractable = null;

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueOpen)
        {
            HideInteractionUI();
            return;
        }

        Ray ray = new Ray(
            playerCamera.transform.position,
            playerCamera.transform.forward
        );

        if (Physics.Raycast(
            ray,
            out RaycastHit hit,
            interactionDistance,
            interactableLayer))
        {
            IInteractable interactable =
                hit.collider.GetComponentInParent<IInteractable>();

            if (interactable != null)
            {
                currentInteractable = interactable;

                ShowInteractionUI();
                return;
            }
        }

        HideInteractionUI();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed)
            return;

        if (DialogueManager.Instance != null &&
            DialogueManager.Instance.IsDialogueOpen)
        {
            DialogueManager.Instance.ContinueDialogue();
            return;
        }

        if (currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    private void ShowInteractionUI()
    {
        if (interactionUI != null)
        {
            interactionUI.SetActive(true);
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