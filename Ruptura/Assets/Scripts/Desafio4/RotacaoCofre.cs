using UnityEngine;
using System.Collections;
using System;

public class RotacaoCofre : MonoBehaviour, IRaycastInteractable
{
    public static event Action<string, int> Rotated = delegate { };

    [SerializeField] private bool coroutineAllowed = true;
    [SerializeField] private int controleFor = 11;
    [SerializeField] private int numberShow = 0;
    [SerializeField] private float controleCorrou = 0.01f;
    [SerializeField] private int reiniciaCont = 9;
    [SerializeField] private Vector3 position = new Vector3(0f, 0f, -3f);

    public void Interact()
    {
        if (coroutineAllowed)
        {
            StartCoroutine("RotateWheel");
        }
    }

    private IEnumerator RotateWheel()
    {
        coroutineAllowed = false;

        for (int i = 0; i <= controleFor; i++)
        {
            transform.Rotate(position);
            yield return new WaitForSeconds(controleCorrou);
        }

        coroutineAllowed = true;

        numberShow += 1;

        if (numberShow > reiniciaCont)
        {
            numberShow = 0;
        }
        print(numberShow);

        Rotated(name, numberShow);

    }
}
