using UnityEngine;

public class MonumentoPuzzle : MonoBehaviour, IRaycastInteractable
{
    // =========================================================================
    // 1. CONFIGURAÇÕES NO INSPECTOR
    // =========================================================================
    [Header("--- ITEM CORRETO DESTE PEDESTAL ---")]
    [Tooltip("Nome exato do item certo para este suporte (ex: Cego, Surdo ou Mudo)")]
    public string nomeItemCorreto;

    [Header("--- GERENCIADOR DO PUZZLE ---")]
    [Tooltip("Arraste o objeto PuzzleManager da cena aqui")]
    public PuzzleMacacos puzzleManager;

    [Header("--- MODELOS 3D DO PEDESTAL (Filhos) ---")]
    [Tooltip("Arraste os 3 modelos 3D posicionados neste suporte")]
    public GameObject modeloCego;
    public GameObject modeloSurdo;
    public GameObject modeloMudo; // Ou Quieto

    [Header("--- ESTADO ATUAL ---")]
    [Tooltip("Mostra qual item está atualmente colocado neste suporte")]
    public Item itemColocado;

    // =========================================================================
    // 2. INICIALIZAÇÃO
    // =========================================================================
    private void Awake()
    {
        EsconderTodosOsModelos();
    }

    // =========================================================================
    // 3. INTERAÇÃO (Chamada automaticamente pelo PlayerInteraction com tecla 'E')
    // =========================================================================
    public void Interact()
    {
        // ---------------------------------------------------------------------
        // CASO 1: O PEDESTAL ESTÁ VAZIO -> Coloca o item que está na mão
        // ---------------------------------------------------------------------
        if (itemColocado == null)
        {
            if (PlayerEquipar.instance != null && PlayerEquipar.instance.TemItemEquipado())
            {
                Item itemParaColocar = PlayerEquipar.instance.itemEquipado;

                // 1. Guarda o item neste monumento
                itemColocado = itemParaColocar;

                // 2. Acende o modelo 3D correspondente em cima do suporte
                MostrarModelo(itemColocado.itemName);

                // 3. Remove o item do inventário
                if (InventoryController.instance != null)
                {
                    InventoryController.instance.RemoveItem(itemParaColocar);
                }

                // 4. Esvazia a mão do Player
                PlayerEquipar.instance.Desequipar();

                Debug.Log(">> Colocou " + itemColocado.itemName + " no monumento.");

                // 5. Avisa o gerenciador para checar a vitória
                if (puzzleManager != null)
                {
                    puzzleManager.ChecarPuzzle();
                }
            }
            else
            {
                Debug.Log("Você precisa equipar um item no inventário primeiro!");
            }
        }
        // ---------------------------------------------------------------------
        // CASO 2: O PEDESTAL JÁ TEM UM ITEM -> Pega de volta para o inventário
        // ---------------------------------------------------------------------
        else
        {
            if (InventoryController.instance != null)
            {
                // 1. Tenta devolver o item para o inventário
                bool guardouComSucesso = InventoryController.instance.AddItem(itemColocado);

                if (guardouComSucesso)
                {
                    Debug.Log(">> Pegou de volta para o inventário: " + itemColocado.itemName);

                    // 2. Esvazia o monumento e apaga a estátua 3D do suporte
                    itemColocado = null;
                    EsconderTodosOsModelos();

                    // 3. Avisa o gerenciador que o estado mudou
                    if (puzzleManager != null)
                    {
                        puzzleManager.ChecarPuzzle();
                    }
                }
            }
        }
    }

    // =========================================================================
    // 4. CONTROLE DOS MODELOS 3D
    // =========================================================================
    private void MostrarModelo(string nomeDoItem)
    {
        EsconderTodosOsModelos();

        if (nomeDoItem == "Cego" || nomeDoItem == "macaco_cego")
        {
            if (modeloCego != null)
            {
                modeloCego.SetActive(true);
            }
        }
        else if (nomeDoItem == "Surdo" || nomeDoItem == "macaco_surdo")
        {
            if (modeloSurdo != null)
            {
                modeloSurdo.SetActive(true);
            }
        }
        else if (nomeDoItem == "Mudo" || nomeDoItem == "Quieto" || nomeDoItem == "macaco_mudo")
        {
            if (modeloMudo != null)
            {
                modeloMudo.SetActive(true);
            }
        }
    }

    private void EsconderTodosOsModelos()
    {
        if (modeloCego != null)
        {
            modeloCego.SetActive(false);
        }

        if (modeloSurdo != null)
        {
            modeloSurdo.SetActive(false);
        }

        if (modeloMudo != null)
        {
            modeloMudo.SetActive(false);
        }
    }

    // =========================================================================
    // 5. VERIFICAÇÃO DE SUCESSO
    // =========================================================================
    public bool EstaCorreto()
    {
        if (itemColocado != null && itemColocado.itemName == nomeItemCorreto)
        {
            return true;
        }

        return false;
    }
}