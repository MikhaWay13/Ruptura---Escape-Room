using System.Collections;
using UnityEngine;

public class EletricPanelScript : MonoBehaviour, IRaycastInteractable
{
    [SerializeField]
    private float openAngle = 90f;

    [SerializeField]
    private float openSpeed = 2f;

    [SerializeField]
    private bool isOpen;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;

    public bool EstaAberto => isOpen && currentCoroutine == null;

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation =
            Quaternion.AngleAxis(openAngle, Vector3.up) * closedRotation;
    }

    // O PlayerInteraction chama este método quando o jogador
    // aponta para o painel e pressiona E.
    public void Interact()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(TogglePanel());
    }

    private IEnumerator TogglePanel()
    {
        Quaternion targetRotation = isOpen
            ? closedRotation
            : openRotation;

        isOpen = !isOpen;

        while (Quaternion.Angle(transform.rotation, targetRotation) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * openSpeed
            );
            yield return null;
        }

        transform.rotation = targetRotation;
        currentCoroutine = null;
    }
}
