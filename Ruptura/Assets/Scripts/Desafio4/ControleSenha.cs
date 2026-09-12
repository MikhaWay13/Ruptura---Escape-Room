using UnityEngine;
using System.Collections;
using System.Linq;
using System;

public class ControleSenha : MonoBehaviour
{
    [SerializeField] private int[] result = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
    private int[] correctCombination = new int[] { 5, 1, 7, 2, 9, 9, 8, 9 };
    private int indice = 0;
   
    private void Start()
    {
        RotacaoCofre.Rotated += CheckResults;
    }

    private void CheckResults(string nomeRoda, int numero)
    {
         indice = nomeRoda[nomeRoda.Length - 1] - '1';

        result[indice] = numero;

        if (result.SequenceEqual(correctCombination))
        {
            Debug.Log("Abriu");
        }
    }
}
