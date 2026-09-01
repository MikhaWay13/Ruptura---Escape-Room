using UnityEngine;
using System.Collections;
using System;

public class ControleSenha : MonoBehaviour
{
    private int[] result, correctCombination;

   
    private void Start()
    {
        result = new int[] {5,5,5,5,5,5,5,5};
        correctCombination = new int[] { 5, 1, 7, 2, 9, 9, 8, 9};
        RotacaoCofre.Rotated += CheckResults;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void CheckResults(string nomeRoda, int numero)
    {
        switch (nomeRoda)
        {
            case "Roda1":
                result[0] = numero;
                break;

            case "Roda2":
                result[1] = numero;
                break;

            case "Roda3":
                result[2] = numero;
                break;

             case "Roda4":
                result[3] = numero;
                break;

            case "Roda5":
                result[4] = numero;
                break;

            case "Roda6":
                result[5] = numero;
                break;

            case "Roda7":
                result[6] = numero;
                break;

            case "Roda8":
                result[7] = numero;
                break;
        }

        if(result[0] == correctCombination[0] && result[1] == correctCombination[1] && result[2] == correctCombination[2] && result[3] == correctCombination[3] && result[4] == correctCombination[4]  && result[5] == correctCombination[5] && result[6] == correctCombination[6] && result[7] == correctCombination[7])
        {
            print("Abriu");
        }
    }
}
