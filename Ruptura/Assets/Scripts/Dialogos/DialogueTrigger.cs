using UnityEngine;

public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Diálogo")]
    [SerializeField] private DialogueData dialogue;

    public void Interact()
    {
        if (dialogue == null)
        {
            Debug.LogWarning(
                $"O objeto '{gameObject.name}' não possui um DialogueData."
            );

            return;
        }

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning(
                "Nenhum DialogueManager foi encontrado na cena."
            );

            return;
        }

        DialogueManager.Instance.StartDialogue(dialogue);
    }
}