using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    [Header("Interação")]
    [SerializeField] private string interactionName = "Interagir";

    public string InteractionName => interactionName;

    public virtual void Interact()
    {
        Debug.Log($"Interagindo com {gameObject.name}");
    }
}