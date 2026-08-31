using UnityEngine;

public class PuzzleMacacos : MonoBehaviour
{
    [Header("--- ARRASTE OS 3 PEDESTAIS DA CENA ---")]
    public MonumentoPuzzle pedestalOculos;     // Espera o Macaco Cego
    public MonumentoPuzzle pedestalGramofone;  // Espera o Macaco Surdo
    public MonumentoPuzzle pedestalMicrofone;  // Espera o Macaco Mudo

    [Header("Gaveta do puzzle")]
    [SerializeField] private GavetaPuzzle gavetaPuzzle;

    private bool puzzleJaResolvido = false;

    // Função que checa se os 3 pedestais estão com os itens corretos
    public void ChecarPuzzle()
    {
        if (puzzleJaResolvido)
        {
            return;
        }

        if (pedestalOculos == null || pedestalGramofone == null || pedestalMicrofone == null)
        {
            return;
        }

        if (pedestalOculos.EstaCorreto() && pedestalGramofone.EstaCorreto() && pedestalMicrofone.EstaCorreto())
        {
            puzzleJaResolvido = true;

             if (gavetaPuzzle != null)
        {
            gavetaPuzzle.Abrir();
        }
        else
        {
            Debug.LogWarning("A GavetaPuzzle não foi configurada.", this);
        }

        }



    }
}