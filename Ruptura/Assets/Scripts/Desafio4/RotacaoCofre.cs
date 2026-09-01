using UnityEngine;
using System.Collections;
using System;
using UnityEngine.InputSystem;
using UnityEngine.Events;

public class RotacaoCofre : MonoBehaviour
{
    public static event Action<string, int> Rotated = delegate { };

    private bool coroutineAllowed;

    private int numberShow;

    private InputAction pressAction;

    private void Awake()
    {
        pressAction = InputSystem.actions.FindAction("Interaction/Press");
    }

    private void Update()
    {
        if (pressAction.WasPressedThisFrame())
        {
            Press();
        }
    }

    void Start()
    {
        coroutineAllowed = true;
        numberShow = 5;
    }

    private void Press()
    {
        print("Ola");
    }
}
