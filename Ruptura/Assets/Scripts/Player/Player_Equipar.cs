using UnityEngine;

public class PlayerEquipar : MonoBehaviour
{
    // =========================================================================
    // 1. SINGLETON
    // =========================================================================
    public static PlayerEquipar instance;

    // =========================================================================
    // 2. REFERÊNCIAS E CONFIGURAÇÕES NO INSPECTOR
    // =========================================================================
    [Header("--- MÃO E ITEM EQUIPADO ---")]
    [Tooltip("Objeto vazio (Empty) filho do Player onde o 3D do item vai ficar")]
    public Transform Hand;

    [Tooltip("Item que o jogador está segurando no momento")]
    public Item itemEquipado;

    [Tooltip("Número do slot que está equipado (-1 = nenhum equipado)")]
    public int slotEquipadoIndex = -1;

    [Header("--- CORES DE SELEÇÃO NO INVENTÁRIO ---")]
    [Tooltip("Cor aplicada ao slot quando o item é equipado")]
    public Color corEquipado = new Color32(255, 220, 100, 255); // Amarelo/Dourado

    // Guarda a referência do objeto 3D gerado na mão
    private GameObject objetoNaMao;

    // =========================================================================
    // 3. INICIALIZAÇÃO
    // =========================================================================
    private void Awake()
    {
        instance = this;
        RemoverObjetoMao();
    }

    // =========================================================================
    // 4. FUNÇÕES PÚBLICAS (AÇÕES DO JOGADOR)
    // =========================================================================

    public void Equipar(int index)
    {
        Item itemDoSlot = InventoryController.instance.GetItemAtSlot(index);

        if (itemDoSlot == null)
        {
            return;
        }

        // 1. TOGGLE: Se clicou no item que JÁ está equipado -> Desequipa
        if (slotEquipadoIndex == index)
        {
            Desequipar();
            return;
        }

        // 2. Limpa o visual anterior
        RestaurarCorSlotAnterior();
        RemoverObjetoMao();

        // 3. Salva o novo item e o índice
        slotEquipadoIndex = index;
        itemEquipado = itemDoSlot;

        // 4. Pinta o slot atual com a cor selecionada
        PintarSlotAtual(index);

        // 5. Cria o modelo 3D na mão do Player
        AddObjetoMao();

        Debug.Log(">> ITEM EQUIPADO: " + itemEquipado.itemName);
    }

    public void Desequipar()
    {
        if (itemEquipado != null)
        {
            Debug.Log(">> Desequipou: " + itemEquipado.itemName + " (Mão vazia).");

            RestaurarCorSlotAnterior();
            RemoverObjetoMao();

            itemEquipado = null;
            slotEquipadoIndex = -1;
        }
    }

    public bool TemItemEquipado()
    {
        if (itemEquipado != null)
        {
            return true;
        }

        return false;
    }

    // =========================================================================
    // 5. FUNÇÕES PRIVADAS (VISUAL 3D E CORES)
    // =========================================================================

    private void AddObjetoMao()
    {
        RemoverObjetoMao();

        if (itemEquipado == null || Hand == null)
        {
            return;
        }

        // Carrega o prefab 3D pelo nome do item
        GameObject prefabDoItem = Resources.Load<GameObject>("Itens/" + itemEquipado.itemName);

        if (prefabDoItem != null)
        {
            objetoNaMao = Instantiate(prefabDoItem, Hand);

            // Alinha na mão
            objetoNaMao.transform.localPosition = Vector3.zero;
            objetoNaMao.transform.localRotation = Quaternion.identity;

            // Desativa a física se houver
            DesativarFisica(objetoNaMao);
        }
    }

    private void RemoverObjetoMao()
    {
        if (Hand != null)
        {
            foreach (Transform filho in Hand)
            {
                Destroy(filho.gameObject);
            }
        }

        objetoNaMao = null;
    }

    private void PintarSlotAtual(int index)
    {
        if (index >= 0 && index < InventoryController.instance.slotImages.Length)
        {
            if (InventoryController.instance.slotImages[index] != null)
            {
                InventoryController.instance.slotImages[index].color = corEquipado;
            }
        }
    }

    private void RestaurarCorSlotAnterior()
    {
        if (slotEquipadoIndex >= 0 && slotEquipadoIndex < InventoryController.instance.slotImages.Length)
        {
            if (InventoryController.instance.slotImages[slotEquipadoIndex] != null)
            {
                InventoryController.instance.slotImages[slotEquipadoIndex].color = Color.white;
            }
        }
    }

    // Desativa a física apenas se o item possuir os componentes
    private void DesativarFisica(GameObject objeto)
    {
        // 1. CHECAGEM DO RIGIDBODY: Se o item TIVER Rigidbody, desativa
        Rigidbody rb = objeto.GetComponentInChildren<Rigidbody>();
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        // 2. CHECAGEM DOS COLLIDERS: Se o item TIVER colisor, desativa
        Collider[] colisores = objeto.GetComponentsInChildren<Collider>();

        for (int i = 0; i < colisores.Length; i++)
        {
            if (colisores[i] != null)
            {
                colisores[i].enabled = false;
            }
        }
    }
}