using UnityEngine;

public class MonumentoPuzzle : MonoBehaviour, IRaycastInteractable
{
    [Header("Itens do puzzle")]
    [SerializeField] private Item itemCorreto;
    [SerializeField] private Item itemCego;
    [SerializeField] private Item itemSurdo;
    [SerializeField] private Item itemMudo;

    [Header("Gerenciador do puzzle")]
    [SerializeField] private PuzzleMacacos puzzleManager;

    [Header("Modelos 3D do pedestal")]
    [SerializeField] private GameObject modeloCego;
    [SerializeField] private GameObject modeloSurdo;
    [SerializeField] private GameObject modeloMudo;

    [Header("Estado atual")]
    [SerializeField] private Item itemColocado;

    private void Awake()
    {
        EsconderTodosOsModelos();
    }

    public void Interact()
    {
        if (itemColocado == null)
        {
            ColocarItem();
        }
        else
        {
            RetirarItem();
        }
    }

    private void ColocarItem()
    {
        if (PlayerEquipar.instance == null || !PlayerEquipar.instance.TemItemEquipado())
        {
            Debug.Log("Você precisa equipar um item no inventário primeiro.");
            SetAvisoEquipar(true);
            return;
        }

        Item itemParaColocar = PlayerEquipar.instance.itemEquipado;

        if (!EhItemDoPuzzle(itemParaColocar))
        {
            Debug.Log("Este item não pertence ao puzzle dos macacos.");
            return;
        }

        if (InventoryController.instance == null)
        {
            Debug.LogWarning("InventoryController não encontrado.", this);
            return;
        }

        if (!InventoryController.instance.RemoveItem(itemParaColocar))
        {
            Debug.LogWarning("Não foi possível remover o item do inventário.", this);
            return;
        }

        itemColocado = itemParaColocar;
        MostrarModelo(itemColocado);

        PlayerEquipar.instance.Desequipar();

        Debug.Log("Colocou " + itemColocado.itemName + " no monumento.");
        ChecarPuzzle();
    }

    private void RetirarItem()
    {
        if (InventoryController.instance == null)
        {
            Debug.LogWarning("InventoryController não encontrado.", this);
            return;
        }

        if (!InventoryController.instance.AddItem(itemColocado))
        {
            Debug.Log("Inventário cheio.");
            return;
        }

        Debug.Log("Pegou " + itemColocado.itemName + " de volta para o inventário.");

        itemColocado = null;
        EsconderTodosOsModelos();

        ChecarPuzzle();
    }

    private void ChecarPuzzle()
    {
        if (puzzleManager != null)
        {
            puzzleManager.ChecarPuzzle();
        }
    }

    private bool EhItemDoPuzzle(Item item)
    {
        return item == itemCego || item == itemSurdo || item == itemMudo;
    }

    private void MostrarModelo(Item item)
    {
        EsconderTodosOsModelos();

        if (item == itemCego && modeloCego != null)
        {
            modeloCego.SetActive(true);
        }
        else if (item == itemSurdo && modeloSurdo != null)
        {
            modeloSurdo.SetActive(true);
        }
        else if (item == itemMudo && modeloMudo != null)
        {
            modeloMudo.SetActive(true);
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

    public bool EstaCorreto()
    {
        return itemColocado != null && itemColocado == itemCorreto;
    }

    private void SetAvisoEquipar(bool state)
    {
        if (UIManager.instance != null)
        {
            UIManager.instance.SetAvisoEquipar(state);
        }
    }
}