using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;


public class TESTE_InventoryController : MonoBehaviour
{
    public static TESTE_InventoryController instance;

    public Item[] slots;
    public Image[] slotImage;
    public int[] slotAmount;

    public GameObject[] OptionsSlot;

    private void Awake()
    {
        for (int i = 0; i < OptionsSlot.Length; i++)
        {
            if (OptionsSlot[i] != null) // [MODIFICADO: proteção contra null]
                OptionsSlot[i].SetActive(false);
        }
        instance = this;
    }

    // ==========================================
    // [NOVO BLOCO]: Configuração automática do Mouse (Hover)
    // ==========================================
    private void Start()
    {
        // Garante que slots vazios comecem com a imagem desligada
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null && slotImage[i] != null)
            {
                slotImage[i].sprite = null;
                slotImage[i].enabled = false;
            }
        }

        RegistrarEventosDeHover(); // Injeta os eventos nos slots
    }

    // [NOVO]: Cria o EventTrigger via código em cada slot
    private void RegistrarEventosDeHover()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int index = i; // Obrigatório no C# para não bugar o índice no loop

            // Detecta hover na imagem do item
            if (slotImage[index] != null)
                AdicionarTriggers(slotImage[index].gameObject, index);

            // Detecta hover também no menu de opções (para não fechar ao passar o mouse nos botões)
            if (index < OptionsSlot.Length && OptionsSlot[index] != null)
                AdicionarTriggers(OptionsSlot[index], index);
        }
    }

    // [NOVO]: Conecta o PointerEnter e PointerExit ao EventTrigger da Unity
    private void AdicionarTriggers(GameObject targetObj, int index)
    {
        EventTrigger trigger = targetObj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = targetObj.AddComponent<EventTrigger>();

        // Quando o mouse ENTRA
        EventTrigger.Entry enter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enter.callback.AddListener((data) => { OnSlotPointerEnter(index); });
        trigger.triggers.Add(enter);

        // Quando o mouse SAI
        EventTrigger.Entry exit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exit.callback.AddListener((data) => { OnSlotPointerExit(index); });
        trigger.triggers.Add(exit);
    }

    // [NOVO]: Chamado quando o mouse entra no slot
    public void OnSlotPointerEnter(int index)
    {
        // Se o slot estiver vazio (null), não faz nada
        if (slots[index] == null) return;

        // Fecha outros e abre apenas o correspondente
        FecharTodasOpcoes();

        if (index < OptionsSlot.Length && OptionsSlot[index] != null)
            OptionsSlot[index].SetActive(true);
    }

    // [NOVO]: Chamado quando o mouse sai do slot
    public void OnSlotPointerExit(int index)
    {
        if (index < OptionsSlot.Length && OptionsSlot[index] != null)
            OptionsSlot[index].SetActive(false);
    }

    private void FecharTodasOpcoes()
    {
        for (int i = 0; i < OptionsSlot.Length; i++)
        {
            if (OptionsSlot[i] != null) OptionsSlot[i].SetActive(false);
        }
    }
    // ==========================================


    public bool AddItem(Item newItem)
    {
        if (newItem == null)
            return false;

        // [CORREÇÃO 1]: Primeiro tenta achar um slot existente para empilhar
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].itemName == newItem.itemName)
            {
                slotAmount[i]++;
                return true;
            }
        }

        // [CORREÇÃO 2]: Se não encontrou igual, coloca no primeiro slot vazio (null)
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null)
            {
                slots[i] = newItem;
                slotAmount[i] = 1;

                if (slotImage[i] != null)
                {
                    slotImage[i].sprite = newItem.itemSprite;
                    slotImage[i].enabled = true;
                    slotImage[i].raycastTarget = true; // [NOVO: Permite que o mouse detecte a imagem]
                }

                return true;
            }
        }

        return false;
    }

    public bool HasItem(Item item)
    {
        if (item == null)
            return false;

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
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null ||
                slots[i].itemName != item.itemName ||
                slotAmount[i] <= 0)
            {
                continue;
            }

            slotAmount[i]--;

            if (slotAmount[i] <= 0) // [MODIFICADO: <= 0 para segurança]
            {
                slots[i] = null;
                slotAmount[i] = 0;

                if (slotImage[i] != null)
                {
                    slotImage[i].sprite = null;
                    slotImage[i].enabled = false;
                    slotImage[i].raycastTarget = false; // [NOVO: Para de bloquear o mouse]
                }

                // [NOVO]: Se o item acabou, desativa o OptionsSlot imediatamente
                if (i < OptionsSlot.Length && OptionsSlot[i] != null)
                {
                    OptionsSlot[i].SetActive(false);
                }
            }

            return true;
        }

        return false;
    }

    public void Options_Slot(int i)
    {
        // [MODIFICADO]: Agora alterna o menu caso seja clicado diretamente
        if (slots[i] != null && OptionsSlot[i] != null)
        {
            OptionsSlot[i].SetActive(!OptionsSlot[i].activeSelf);
        }
    }
    





}