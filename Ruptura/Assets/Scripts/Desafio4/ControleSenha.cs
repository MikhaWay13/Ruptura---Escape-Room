using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class ControleSenha : MonoBehaviour
{
    private int[] result, correctCombination;

   
    private void Start()
    {
        result = new int[] {5,5,5};
        correctCombination = new int[] { 3, 7, 9 };
        RotacaoCofre.Rotated += CheckResults();
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
        }

        if(result[0] == correctCombination[0] && result[1] == correctCombination[1] && result[2] == correctCombination[2]  )
        {
            print("Abriu");
        }
    }
}
