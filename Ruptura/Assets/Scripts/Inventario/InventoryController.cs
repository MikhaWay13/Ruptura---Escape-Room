using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventoryController : MonoBehaviour
{
    public static InventoryController instance;

    [Header("Dados do Inventário")]
    public Item[] slots;
    public Image[] slotImages;          
    public int[] slotAmount;
    public TextMeshProUGUI[] slotTexts;

    [Header("Sistema de Opções")]
    public GameObject[] slotObjects;    
    public GameObject[] optionsSlots;   

    // Cor escura Hexadecimal: #151A1D
   private readonly Color corVazia = new Color32(0x15, 0x1A, 0x1D, 255);

    private void Awake()
    {
        instance = this;
        FecharTodasOpcoes();
    }

    private void Start()
    {
        // Atualiza a cor/sprite de cada slot no início
        for (int i = 0; i < slots.Length; i++)
        {
            AtualizarVisualSlot(i);
        }
    }

    public void OnSlotPointerEnter(int index)
    {
        // Se o slot estiver vazio (sem item), não abre as opções
        if (slots[index] == null || slotAmount[index] <= 0)
        {
            return;
        }

        FecharTodasOpcoes();

        if (index < optionsSlots.Length && optionsSlots[index] != null)
        {
            optionsSlots[index].SetActive(true);
        }
    }

    public void OnSlotPointerExit(int index)
    {
        if (index < optionsSlots.Length && optionsSlots[index] != null)
        {
            optionsSlots[index].SetActive(false);
        }
    }

    private void FecharTodasOpcoes()
    {
        if (optionsSlots == null)
        {
            return;
        }

        for (int i = 0; i < optionsSlots.Length; i++)
        {
            if (optionsSlots[i] != null)
            {
                optionsSlots[i].SetActive(false);
            }
        }
    }

    // ==========================================
    // GERENCIAMENTO DE ITENS
    // ==========================================

public bool AddItem(Item newItem)
    {
        if (newItem == null)
        {
            return false;
        }

        // 1. Tenta empilhar no item que já existe
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemName == newItem.itemName)
            {
                slotAmount[i]++;
                AtualizarVisualSlot(i);
                return true;
            }
        }

        // 2. Se não tem igual, acha o primeiro espaço vazio
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = newItem;
                slotAmount[i] = 1;
                AtualizarVisualSlot(i);
                return true;
            }
        }

        return false;
    }

    public bool HasItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemName == item.itemName && slotAmount[i] > 0)
            {
                return true;
            }
        }

        return false;
    }

    public bool RemoveItem(Item item)
    {
        if (item == null)
        {
            return false;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemName == item.itemName && slotAmount[i] > 0)
            {
                slotAmount[i]--;

                if (slotAmount[i] <= 0)
                {
                    slots[i] = null;
                    slotAmount[i] = 0;

                    if (i < optionsSlots.Length && optionsSlots[i] != null)
                    {
                        optionsSlots[i].SetActive(false);
                    }
                }

                AtualizarVisualSlot(i);
                return true;
            }
        }

        return false;
    }

  public void AtualizarVisualSlot(int index)
    {
        // 1. Atualiza a imagem/fundo
        if (index >= 0 && index < slotImages.Length && slotImages[index] != null)
        {
            if (slots[index] != null && slotAmount[index] > 0)
            {
                slotImages[index].sprite = slots[index].itemSprite;
                slotImages[index].color = Color.white;            // Fica branco para mostrar o item
                slotImages[index].enabled = true;
            }
            else
            {
                slotImages[index].sprite = null;
                slotImages[index].color = corVazia;               // Cor #151A1D quando vazio
                slotImages[index].enabled = true;
            }
        }

        // 2. Atualiza o texto do nome do item
        if (index >= 0 && index < slotTexts.Length && slotTexts[index] != null)
        {
            if (slots[index] != null && slotAmount[index] > 0)
            {
                slotTexts[index].text = slots[index].itemName;    // Escreve o nome do item
            }
            else
            {
                slotTexts[index].text = "";                       // Deixa vazio se não tiver item
            }
        }
    }

    public Item GetItemAtSlot(int index)
    {
        if (index < 0 || index >= slots.Length)
        {
            return null;
        }

        if (slotAmount[index] <= 0)
        {
            return null;
        }

        return slots[index];
    }

    // ==========================================
    // FUNÇÕES DOS BOTÕES (EQUIPAR / ROTACIONAR)
    // ==========================================
    public void EquiparSlot(int index)
    {
        Item item = GetItemAtSlot(index);
        
        if (item != null)
        {
            // Avisa o PlayerEquipment que este item foi equipado
            if (Player_Equipar.instance != null)
            {
                Player_Equipar.instance.EquiparItem(item);
            }

            // Fecha o menu de opções
            FecharTodasOpcoes();
        }
    }

    public void RotacionarSlot(int index)
    {
        Item item = GetItemAtSlot(index);
        
        if (item != null)
        {
            Debug.Log("Rotacionando item do slot " + index + ": " + item.itemName);
        }
    }
}


