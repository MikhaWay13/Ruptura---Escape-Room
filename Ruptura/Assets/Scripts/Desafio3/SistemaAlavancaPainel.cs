using System.Collections;
using TMPro;
using UnityEngine;

[DefaultExecutionOrder(100)]
[DisallowMultipleComponent]
public class SistemaAlavancaPainel : MonoBehaviour, IRaycastInteractable
{
    // A interação avança sempre nesta ordem:
    // Ausente -> InstaladaDesligada -> Ligada.
    private enum EstadoAlavanca
    {
        Ausente,
        InstaladaDesligada,
        Ligada
    }

    [Header("Referências")]
    [SerializeField] private EletricPanelScript painel;
    [SerializeField] private Item itemAlavanca;
    [SerializeField] private Transform alavancaInstalada;
    [SerializeField] private Renderer[] renderizadoresAlavanca;
    [SerializeField] private TMP_Text textoStatus;
    [SerializeField] private CaboProjetor caboProjetor;

    [Header("Rotação")]
    [SerializeField] private float anguloLigadoX = -38.058f;
    [SerializeField, Min(0.05f)] private float duracaoMovimento = 0.65f;

    [Header("Iluminação")]
    [SerializeField] private Light luzAmbiente;
    [SerializeField, Min(0f)] private float intensidadeAmbienteDesligada = 0.2f;
    [SerializeField, Min(0f)] private float intensidadeAmbienteLigada = 1f;
    [SerializeField] private Light[] luzesPrincipais;

    private EstadoAlavanca estado;
    private Quaternion rotacaoDesligada;
    private Quaternion rotacaoLigada;
    private Coroutine animacao;

    public bool AlavancaInstalada => estado != EstadoAlavanca.Ausente;
    public bool EnergiaLigada => estado == EstadoAlavanca.Ligada;

    private void Awake()
    {
        ConfigurarRotacoes();
        estado = EstadoAlavanca.Ausente;
        DefinirAlavancaVisivel(false);
        DefinirIluminacao(false);
    }

    private void Start()
    {
        AtualizarTexto("Encontre a alavanca dentro da gaveta.");
    }

    public void Interact()
    {
        if (animacao != null || estado == EstadoAlavanca.Ligada)
        {
            return;
        }

        if (!painel.EstaAberto)
        {
            AtualizarTexto("Abra completamente o painel elétrico.");
            return;
        }

        if (estado == EstadoAlavanca.Ausente)
        {
            InstalarAlavanca();
            return;
        }

        animacao = StartCoroutine(LigarEnergia());
    }

    private void ConfigurarRotacoes()
    {
        rotacaoDesligada = alavancaInstalada.localRotation;

        Vector3 angulosLigados = alavancaInstalada.localEulerAngles;
        angulosLigados.x = anguloLigadoX;
        rotacaoLigada = Quaternion.Euler(angulosLigados);
    }

    private void InstalarAlavanca()
    {
        if (!InventoryController.instance.HasItem(itemAlavanca))
        {
            AtualizarTexto("Você precisa encontrar a alavanca.");
            return;
        }

        if (!InventoryController.instance.RemoveItem(itemAlavanca))
        {
            return;
        }

        DefinirRotacao(rotacaoDesligada);
        DefinirAlavancaVisivel(true);
        estado = EstadoAlavanca.InstaladaDesligada;
        AtualizarTexto("Alavanca instalada. Pressione E novamente.");
    }

    private IEnumerator LigarEnergia()
    {
        Quaternion inicio = alavancaInstalada.localRotation;

        float tempo = 0f;

        while (tempo < duracaoMovimento)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracaoMovimento);
            progresso = Mathf.SmoothStep(0f, 1f, progresso);

            Quaternion rotacaoAtual = Quaternion.Slerp(
                inicio,
                rotacaoLigada,
                progresso
            );
            DefinirRotacao(rotacaoAtual);

            yield return null;
        }

        DefinirRotacao(rotacaoLigada);
        estado = EstadoAlavanca.Ligada;
        DefinirIluminacao(true);

        caboProjetor.LiberarCabo();

        AtualizarTexto("Energia ligada! Pegue o cabo do projetor.");
        animacao = null;
    }

    private void DefinirRotacao(Quaternion rotacao)
    {
        alavancaInstalada.localRotation = rotacao;
    }

    private void DefinirAlavancaVisivel(bool visivel)
    {
        foreach (Renderer renderizador in renderizadoresAlavanca)
        {
            renderizador.enabled = visivel;
        }
    }

    private void DefinirIluminacao(bool ligada)
    {
        luzAmbiente.intensity = ligada
            ? intensidadeAmbienteLigada
            : intensidadeAmbienteDesligada;

        foreach (Light luz in luzesPrincipais)
        {
            luz.enabled = ligada;
        }
    }

    private void AtualizarTexto(string mensagem)
    {
        textoStatus.text = mensagem;
        Debug.Log(mensagem, this);
    }
}
