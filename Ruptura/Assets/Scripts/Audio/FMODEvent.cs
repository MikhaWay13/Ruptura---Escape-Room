using UnityEngine;
using FMODUnity;

public class FMODEvent : MonoBehaviour
{
    [field: Header("Item Collect")]
    [field: SerializeField] public EventReference itemCollectEvent {get; private set; }

    [field: Header("Player Footsteps")]
    [field: SerializeField] public EventReference playerFootstepsEvent {get; private set; }


   public static FMODEvent instance { get; private set; }

    private void Awake()
    {
        if (instance != null)
        {
            Debug.Log(" foi encontrado mais de um FMOD Event na cena.");
            
        }   
        instance = this;
        
    }
}
