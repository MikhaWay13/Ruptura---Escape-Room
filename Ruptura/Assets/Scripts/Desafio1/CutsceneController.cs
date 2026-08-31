using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
 
    [Header("Item que inicia a cutscene")]
    [SerializeField] private Item vidro;

    [Header("Timeline")]
    [SerializeField] private PlayableDirector playableDirector;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;
    [SerializeField] private Transform destinoFase2;

    private CharacterController characterController;
    private bool cutsceneIniciada;

    private void Awake()
    {
        if (playableDirector == null)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        if (playerController != null)
        {
            characterController = playerController.GetComponent<CharacterController>();
        }
    }

    public void TentarIniciar(Item itemColetado)
    {
        if (cutsceneIniciada || itemColetado != vidro)
        {
            return;
        }

        if (playableDirector == null)
        {
            Debug.LogWarning("Playable Director não configurado.", this);
            return;
        }

        cutsceneIniciada = true;

        if (playerController != null)
        {
            playerController.SetGameplayControlEnabled(false);
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }

        playableDirector.time = 0;
        playableDirector.Play();
    }

    public void TeleportarParaFase2()
    {
        if (playerController == null || destinoFase2 == null)
        {
            Debug.LogWarning("Player ou destino da fase 2 não configurado.", this);
            return;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        playerController.transform.SetPositionAndRotation(
            destinoFase2.position,
            destinoFase2.rotation
        );

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    public void FinalizarCutscene()
    {
        if (playerInteraction != null)
        {
            playerInteraction.enabled = true;
        }

        if (playerController != null)
        {
            playerController.SetGameplayControlEnabled(true);
        }
    }
}

