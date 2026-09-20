using System.Collections;
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

        [Header("Configuração da Fase/Cômodo")]
        public int indiceFase = -1;
        
        [Tooltip("Tempo exato (em segundos) da timeline em que a fase será ativada.")]
        public float tempoTrocaFase = 0f; 
    }

    [Header("Fases / Cômodos do Jogo")]
    [SerializeField] private GameObject[] fasesGame;

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
            playerController.TryGetComponent(out characterController);
        }

        AtivarFase(0);
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

    public void AtivarFase(int indiceFaseDesejada)
    {
        if (fasesGame == null || fasesGame.Length == 0) return;
        if (indiceFaseDesejada < 0 || indiceFaseDesejada >= fasesGame.Length) return;

        for (int i = 0; i < fasesGame.Length; i++)
        {
            if (fasesGame[i] != null)
            {
                fasesGame[i].SetActive(i == indiceFaseDesejada);
            }
        }

        Debug.Log($"Fase ativada com sucesso: {fasesGame[indiceFaseDesejada].name} (Índice {indiceFaseDesejada})", this);
    }

    public void TentarIniciar(Item itemColetado)
    {
        if (itemColetado == null) return;

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
        if (indice < 0 || indice >= cutscenes.Length) return;
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
    }

    private void IniciarCutscene(CutsceneData cutscene)
    {
        if (cutsceneEmAndamento || cutscene == null || cutscene.director == null) return;

        cutsceneAtual = cutscene;
        cutsceneEmAndamento = true;

        BloquearControles();

        cutsceneAtual.director.stopped -= AoFinalizarDirector;
        cutsceneAtual.director.stopped += AoFinalizarDirector;
        cutsceneAtual.director.time = 0;
        
        // Inicia a animação da timeline
        cutsceneAtual.director.Play();

        // Passa o diretor para a Coroutine poder vigiar o tempo dele
        if (cutsceneAtual.indiceFase >= 0)
        {
            if (cutsceneAtual.tempoTrocaFase > 0f)
            {
                StartCoroutine(AtrasarTrocaDeFase(cutsceneAtual.indiceFase, cutsceneAtual.tempoTrocaFase, cutsceneAtual.director));
            }
            else
            {
                AtivarFase(cutsceneAtual.indiceFase);
            }
        }
    }

    private IEnumerator AtrasarTrocaDeFase(int indiceFase, float tempoDeEspera, PlayableDirector director)
    {
        // Fica verificando frame a frame o relógio interno da cutscene
        while (director != null && director.time < tempoDeEspera)
        {
            yield return null; 
        }
        
        // Assim que passar da marca exata (ex: 3.5 segundos), ativa a fase
        AtivarFase(indiceFase);
    }

    private void BloquearControles()
    {
        if (playerController != null) playerController.SetGameplayControlEnabled(false);
        if (playerInteraction != null) playerInteraction.enabled = false;
    }

    private void LiberarControles()
    {
        if (playerInteraction != null) playerInteraction.enabled = true;
        if (playerController != null) playerController.SetGameplayControlEnabled(true);
    }

    public void Teleportar()
    {
        if (cutsceneAtual == null || cutsceneAtual.destinoTeleport == null || playerController == null) return;

        if (characterController != null) characterController.enabled = false;

        playerController.transform.SetPositionAndRotation(
            cutsceneAtual.destinoTeleport.position,
            cutsceneAtual.destinoTeleport.rotation
        );

        if (characterController != null) characterController.enabled = true;
    }

    public void Finalizar()
    {
        if (!cutsceneEmAndamento) return;
        if (cutsceneAtual != null && cutsceneAtual.director != null) cutsceneAtual.director.stopped -= AoFinalizarDirector;

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
        if (cutsceneAtual != null && cutsceneAtual.director != null) cutsceneAtual.director.stopped -= AoFinalizarDirector;
        if (cutsceneEmAndamento) LiberarControles();

        cutsceneAtual = null;
        cutsceneEmAndamento = false;
    }
}