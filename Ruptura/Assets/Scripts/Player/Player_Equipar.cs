using UnityEngine;

public class Player_Equipar : MonoBehaviour
{
  public static Player_Equipar instance; //singleton

  [Header("Equipamento do Jogador")]
  public Item itemEquipado; // Item atualmente equipado

  private void Awake()
  {
    instance = this; //singleton
  }

  public void EquiparItem(Item item)
  {
    if (item == null)
      return;

    itemEquipado = item;
    print("Item equipado: " + item.itemName);
  }

  public void Desequipar()
  {
    itemEquipado = null;
    print("Item desequipado");
  }

  public bool isItemEquipped(Item item)
  {
    return itemEquipado == item;
  }
  
}
