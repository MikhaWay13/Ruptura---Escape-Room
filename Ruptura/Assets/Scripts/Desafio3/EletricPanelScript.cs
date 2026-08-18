using System.Collections;
using UnityEngine;

public class EletricPanelScript : MonoBehaviour, IRaycastInteractable
{
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool isOpen;

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

    public void Interact()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        isOpen = !isOpen;
        Quaternion destino = isOpen ? openRotation : closedRotation;
        currentCoroutine = StartCoroutine(GirarPainel(destino));
    }

    private IEnumerator GirarPainel(Quaternion destino)
    {
        while (Quaternion.Angle(transform.rotation, destino) > 0.01f)
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                destino,
                Time.deltaTime * openSpeed
            );
            yield return null;
        }

        transform.rotation = destino;
        currentCoroutine = null;
    }
}
