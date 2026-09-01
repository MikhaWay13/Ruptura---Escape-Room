using UnityEngine;
using System.Collections;
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
        print("Ola");
    }
}
