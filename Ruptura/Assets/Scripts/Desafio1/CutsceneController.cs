using UnityEngine;
using UnityEngine.Playables;

public class CutsceneController : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneData
    {
        public string nome;
        public PlayableDirector director;
        public bool iniciarAutomaticamente;
        public Item itemGatilho;
        public Transform destinoTeleport;
    }

    [Header("Cutscenes do jogo")]
    [SerializeField] private CutsceneData[] cutscenes;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerInteraction playerInteraction;

    private CutsceneData cutsceneAtual;
    private CharacterController characterController;
    private bool cutsceneEmAndamento;

    private void Awake()
    {
        if (playerController != null)
        {
            characterController = playerController.GetComponent<CharacterController>();
        }
    }

    private void Start()
    {
        for (int i = 0; i < cutscenes.Length; i++)
        {
            if (cutscenes[i] != null && cutscenes[i].iniciarAutomaticamente)
            {
                IniciarCutscene(cutscenes[i]);
                break;
            }
        }
    }

    public void TentarIniciar(Item itemColetado)
    {
        if (itemColetado == null)
        {
            return;
        }

        for (int i = 0; i < cutscenes.Length; i++)
        {
            CutsceneData cutscene = cutscenes[i];

            if (cutscene != null && cutscene.itemGatilho == itemColetado)
            {
                IniciarCutscene(cutscene);
                return;
            }
        }
    }

    public void IniciarPorIndice(int indice)
    {
        if (indice < 0 || indice >= cutscenes.Length)
        {
            Debug.LogWarning("Índice de cutscene inválido: " + indice, this);
            return;
        }

        IniciarCutscene(cutscenes[indice]);
    }

    public void IniciarPorNome(string nome)
    {
        for (int i = 0; i < cutscenes.Length; i++)
        {
            if (cutscenes[i] != null && cutscenes[i].nome == nome)
            {
                IniciarCutscene(cutscenes[i]);
                return;
            }
        }

        Debug.LogWarning("Cutscene não encontrada: " + nome, this);
    }

    private void IniciarCutscene(CutsceneData cutscene)
    {
        if (cutsceneEmAndamento)
        {
            Debug.LogWarning("Já existe uma cutscene em andamento.", this);
            return;
        }

        if (cutscene == null || cutscene.director == null)
        {
            Debug.LogWarning("Cutscene sem Playable Director configurado.", this);
            return;
        }

        cutsceneAtual = cutscene;
        cutsceneEmAndamento = true;

        BloquearControles();

        cutsceneAtual.director.stopped += AoFinalizarDirector;
        cutsceneAtual.director.time = 0;
        cutsceneAtual.director.Play();
    }

    private void BloquearControles()
    {
        if (playerController != null)
        {
            playerController.SetGameplayControlEnabled(false);
        }

        if (playerInteraction != null)
        {
            playerInteraction.enabled = false;
        }
    }

    private void LiberarControles()
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

    public void Teleportar()
    {
        if (cutsceneAtual == null || cutsceneAtual.destinoTeleport == null)
        {
            Debug.LogWarning("A cutscene atual não possui destino de teleporte.", this);
            return;
        }

        if (playerController == null)
        {
            Debug.LogWarning("Player Controller não configurado.", this);
            return;
        }

        if (characterController != null)
        {
            characterController.enabled = false;
        }

        playerController.transform.SetPositionAndRotation(
            cutsceneAtual.destinoTeleport.position,
            cutsceneAtual.destinoTeleport.rotation
        );

        if (characterController != null)
        {
            characterController.enabled = true;
        }
    }

    public void Finalizar()
    {
        if (!cutsceneEmAndamento)
        {
            return;
        }

        if (cutsceneAtual != null && cutsceneAtual.director != null)
        {
            cutsceneAtual.director.stopped -= AoFinalizarDirector;
        }

        LiberarControles();

        cutsceneAtual = null;
        cutsceneEmAndamento = false;
    }

    private void AoFinalizarDirector(PlayableDirector director)
    {
        Finalizar();
    }

    private void OnDisable()
    {
        if (cutsceneAtual != null && cutsceneAtual.director != null)
        {
            cutsceneAtual.director.stopped -= AoFinalizarDirector;
        }

        if (cutsceneEmAndamento)
        {
            LiberarControles();
        }

        cutsceneAtual = null;
        cutsceneEmAndamento = false;
    }
}