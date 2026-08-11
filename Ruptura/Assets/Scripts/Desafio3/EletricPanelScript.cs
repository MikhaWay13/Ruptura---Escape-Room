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

    private void Start()
    {
        closedRotation = transform.rotation;
        openRotation =
            Quaternion.AngleAxis(openAngle, Vector3.up) * closedRotation;
    }

    // O Input System é lido pelo PlayerController. Este método só é
    // chamado quando o raycast central da câmera acerta este painel.
    public void Interact(PlayerController player)
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
