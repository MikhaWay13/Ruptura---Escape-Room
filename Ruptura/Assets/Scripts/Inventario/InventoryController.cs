using UnityEngine;
using UnityEngine.InputSystem;
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


    /*
    // ANTIGO:
    // Usado para descobrir qual slot estava com o mouse em cima.
    private int slotSobMouse = -1;
    */


    // Cor escura Hexadecimal: #151A1D
    private readonly Color corVazia =
        new Color32(0x15, 0x1A, 0x1D, 255);


    private void Awake()
    {
        instance = this;

        FecharTodasOpcoes();
    }


    private void Start()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            AtualizarVisualSlot(i);
        }
    }


    /*
    // =========================================================
    // ANTIGO SISTEMA DE HOVER PELO MOUSE
    // =========================================================

    private void LateUpdate()
    {
        if (!UIManager.instance.painelInventory.activeInHierarchy)
        {
            slotSobMouse = -1;
            return;
        }


        Vector2 posicaoMouse =
            Mouse.current.position.ReadValue();

        int novoSlotSobMouse = -1;


        for (int i = 0; i < slotObjects.Length; i++)
        {
            RectTransform slot =
                slotObjects[i].GetComponent<RectTransform>();


            if (RectTransformUtility.RectangleContainsScreenPoint(
                slot,
                posicaoMouse))
            {
                novoSlotSobMouse = i;
                break;
            }
        }


        if (novoSlotSobMouse == slotSobMouse &&
            (novoSlotSobMouse < 0 ||
            optionsSlots[novoSlotSobMouse].activeSelf))
        {
            return;
        }


        FecharTodasOpcoes();

        slotSobMouse = novoSlotSobMouse;


        if (slotSobMouse >= 0 &&
            slots[slotSobMouse] != null &&
            slotAmount[slotSobMouse] > 0)
        {
            optionsSlots[slotSobMouse].SetActive(true);
        }
    }
    */


    /*
    // =========================================================
    // ANTIGO POINTER ENTER
    // =========================================================

    public void OnSlotPointerEnter(int index)
    {
        if (slots[index] == null ||
            slotAmount[index] <= 0)
        {
            return;
        }


        FecharTodasOpcoes();


        if (index < optionsSlots.Length &&
            optionsSlots[index] != null)
        {
            optionsSlots[index].SetActive(true);
        }
    }
    */


    /*
    // =========================================================
    // ANTIGO POINTER EXIT
    // =========================================================

    public void OnSlotPointerExit(int index)
    {
        if (index < optionsSlots.Length &&
            optionsSlots[index] != null)
        {
            optionsSlots[index].SetActive(false);
        }
    }
    */


    // =========================================================
    // NOVO SISTEMA:
    // CLIQUE NO PC / TOUCH NO MOBILE
    // =========================================================

    public void OnSlotClick(int index)
    {
        if (index < 0 || index >= slots.Length)
        {
            return;
        }


        // Slot vazio
        if (slots[index] == null ||
            slotAmount[index] <= 0)
        {
            FecharTodasOpcoes();

            return;
        }


        // Verifica se as opções desse slot já estavam abertas
        bool estavaAberto =
            index < optionsSlots.Length &&
            optionsSlots[index] != null &&
            optionsSlots[index].activeSelf;


        // Fecha qualquer outro menu aberto
        FecharTodasOpcoes();


        // Se já estava aberto:
        // fica fechado.
        //
        // Se estava fechado:
        // abre.
        if (!estavaAberto &&
            index < optionsSlots.Length &&
            optionsSlots[index] != null)
        {
            optionsSlots[index].SetActive(true);
        }
    }


    // =========================================================
    // FECHAR TODAS AS OPÇÕES
    // =========================================================

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


    // =========================================================
    // GERENCIAMENTO DE ITENS
    // =========================================================

    public bool AddItem(Item newItem)
    {
        if (newItem == null)
        {
            return false;
        }


        // Procura se já existe o mesmo item
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null &&
                slots[i].itemName == newItem.itemName)
            {
                slotAmount[i]++;

                AtualizarVisualSlot(i);

                return true;
            }
        }


        // Procura um slot vazio
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


        // Inventário cheio
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
            if (slots[i] != null &&
                slots[i].itemName == item.itemName &&
                slotAmount[i] > 0)
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
            if (slots[i] != null &&
                slots[i].itemName == item.itemName &&
                slotAmount[i] > 0)
            {
                slotAmount[i]--;


                if (slotAmount[i] <= 0)
                {
                    // Se o item que acabou estava equipado,
                    // desequipa.
                    if (PlayerEquipar.instance != null &&
                        PlayerEquipar.instance.slotEquipadoIndex == i)
                    {
                        PlayerEquipar.instance.Desequipar();
                    }


                    slots[i] = null;
                    slotAmount[i] = 0;


                    if (i < optionsSlots.Length &&
                        optionsSlots[i] != null)
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


    // =========================================================
    // ATUALIZAÇÃO VISUAL
    // =========================================================

    public void AtualizarVisualSlot(int index)
    {
        if (index >= 0 &&
            index < slotImages.Length &&
            slotImages[index] != null)
        {
            if (slots[index] != null &&
                slotAmount[index] > 0)
            {
                slotImages[index].sprite =
                    slots[index].itemSprite;

                slotImages[index].color =
                    Color.white;

                slotImages[index].enabled =
                    true;
            }
            else
            {
                slotImages[index].sprite =
                    null;

                slotImages[index].color =
                    corVazia;

                slotImages[index].enabled =
                    true;
            }
        }


        if (index >= 0 &&
            index < slotTexts.Length &&
            slotTexts[index] != null)
        {
            if (slots[index] != null &&
                slotAmount[index] > 0)
            {
                slotTexts[index].text =
                    slots[index].itemName;
            }
            else
            {
                slotTexts[index].text = "";
            }
        }
    }


    // =========================================================
    // PEGAR ITEM DE UM SLOT
    // =========================================================

    public Item GetItemAtSlot(int index)
    {
        if (index < 0 ||
            index >= slots.Length)
        {
            return null;
        }


        if (slotAmount[index] <= 0)
        {
            return null;
        }


        return slots[index];
    }


    // =========================================================
    // EQUIPAR / USAR SLOT
    // =========================================================

    public void EquiparSlot(int index)
    {
        // Se estiver no puzzle do relógio,
        // manda o item para o relógio.
        if (ClockPuzzle.instance != null &&
            ClockPuzzle.instance.IsOpen)
        {
            ClockPuzzle.instance.InsertItemFromSlot(index);

            FecharTodasOpcoes();

            return;
        }


        // Gameplay normal
        if (PlayerEquipar.instance != null)
        {
            PlayerEquipar.instance.Equipar(index);
        }


        FecharTodasOpcoes();
    }


    // =========================================================
    // ROTACIONAR ITEM
    // =========================================================

    public void RotacionarSlot(int index)
    {
        Item item =
            GetItemAtSlot(index);


        if (item != null)
        {
            Debug.Log(
                "Rotacionando item do slot " +
                index +
                ": " +
                item.itemName
            );
        }
    }
}