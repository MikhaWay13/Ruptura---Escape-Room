using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Diálogo")]
    [SerializeField] private DialogueData dialogue;

    public void TriggerDialogue()
    {
        if (dialogue == null)
        {
            Debug.LogWarning(
                $"DialogueTrigger no objeto '{gameObject.name}' não possui um DialogueData."
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