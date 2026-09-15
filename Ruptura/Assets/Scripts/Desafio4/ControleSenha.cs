using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class ControleSenha : MonoBehaviour
{
    [SerializeField] private Animator portaAnimator;
    [SerializeField] private CofreManager cofreManager;
    [SerializeField] private int[] result = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] correctCombination = new int[] { 5, 1, 7, 2, 9, 9, 8, 9 };
    private int indice = 0;
    private bool abriu = false;

    private void Start()
    {
        RotacaoCofre.Rotated += CheckResults;
    }

    private void CheckResults(string nomeRoda, int numero)
    {
         indice = nomeRoda[nomeRoda.Length - 1] - '1';

        result[indice] = numero;

        if (!abriu && result.SequenceEqual(correctCombination))
        {
            abriu = true;
            cofreManager.Back();
            portaAnimator.Play("Abrir", 0, 0f);
        }
    }
}
