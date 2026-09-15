using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class ControleSenha : MonoBehaviour
{
    [SerializeField] private CofreManager cofreManager;
    [SerializeField] private int[] result = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] correctCombination = new int[] { 5, 1, 7, 2, 9, 9, 8, 9 };
    private int indice = 0;
    private bool abriu = false;
    [SerializeField] private float openAngle = 90f;
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private bool isOpen;

    private Quaternion closedRotation;
    private Quaternion openRotation;
    private Coroutine currentCoroutine;

    public bool EstaAberto => isOpen && currentCoroutine == null;


    private void Start()
    {
        RotacaoCofre.Rotated += CheckResults;
        closedRotation = transform.rotation;
        openRotation =
            Quaternion.AngleAxis(openAngle, Vector3.up) * closedRotation;
    }

    private void CheckResults(string nomeRoda, int numero)
    {
        indice = nomeRoda[nomeRoda.Length - 1] - '1';

        result[indice] = numero;

        if (!abriu && result.SequenceEqual(correctCombination))
        {
            cofreManager.Back();
            if (currentCoroutine != null)
            {
                StopCoroutine(currentCoroutine);
            }

            isOpen = !isOpen;
            Quaternion destino = isOpen ? openRotation : closedRotation;
            currentCoroutine = StartCoroutine(GirarPainel(destino));
            abriu = true;
        }
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
