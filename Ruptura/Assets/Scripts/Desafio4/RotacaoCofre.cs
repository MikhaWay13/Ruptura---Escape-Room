using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;

public class RotacaoCofre : MonoBehaviour
{
    public static event Action<string, int> Rotated = delegate { };

    private bool coroutineAllowed;

    private int numberShow;

    void Start()
    {
        coroutineAllowed = true;
        numberShow = 5;
    }

    public void Press()
    {
        if (coroutineAllowed)
        {
            StartCoroutine("RotateWheel");
        }
    }

    private IEnumerator RotateWheel()
    {
        coroutineAllowed = false;

        for (int i = 0; i <= 11; i++)
        {
            transform.Rotate(0f, 0f, -3f);
            yield return new WaitForSeconds(0.01f);
        }

        coroutineAllowed = true;

        numberShow += 1;
        print(numberShow);

        if (numberShow >= 9)
        {
            numberShow = -1;
        }

        Rotated(name, numberShow);

            }
}
