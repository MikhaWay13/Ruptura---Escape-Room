using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class SistemaAlavancaPainel : MonoBehaviour, IRaycastInteractable
{
    private enum EstadoAlavanca
    {
        Ausente,
        InstaladaDesligada,
        Ligada
    }

    [Header("Referências")]
    [SerializeField]
    private EletricPanelScript painel;

    [SerializeField]
    private Item itemAlavanca;

    [SerializeField]
    private Transform alavancaInstalada;

    [SerializeField]
    private Renderer[] renderizadoresAlavanca;

    [SerializeField]
    private TMP_Text textoStatus;

    [Header("Rotação")]
    [SerializeField]
    private float anguloLigadoX = -38.058f;

    [SerializeField, Min(0.05f)]
    private float duracaoMovimento = 0.65f;

    [Header("Iluminação")]
    [SerializeField]
    private Light luzAmbiente;

    [SerializeField, Min(0f)]
    private float intensidadeAmbienteDesligada = 0.2f;

    [SerializeField, Min(0f)]
    private float intensidadeAmbienteLigada = 1f;

    [SerializeField]
    private Light[] luzesPrincipais;

    private EstadoAlavanca estado;
    private Quaternion rotacaoDesligada;
    private Quaternion rotacaoLigada;
    private Coroutine animacao;

    private void Awake()
    {
        if (alavancaInstalada != null)
        {
            rotacaoDesligada = alavancaInstalada.localRotation;

            Vector3 angulosLigados =
                alavancaInstalada.localEulerAngles;
            angulosLigados.x = anguloLigadoX;
            rotacaoLigada = Quaternion.Euler(angulosLigados);
        }

        estado = EstadoAlavanca.Ausente;
        DefinirAlavancaVisivel(false);
        DefinirIluminacao(false);
    }

    public void Interact()
    {
        if (animacao != null || estado == EstadoAlavanca.Ligada)
        {
            return;
        }

        if (painel == null || !painel.EstaAberto)
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

    private void InstalarAlavanca()
    {
        if (InventoryController.instance == null ||
            !InventoryController.instance.HasItem(itemAlavanca))
        {
            AtualizarTexto("Você precisa encontrar a alavanca.");
            return;
        }

        if (!InventoryController.instance.RemoveItem(itemAlavanca))
        {
            return;
        }

        if (alavancaInstalada != null)
        {
            alavancaInstalada.localRotation = rotacaoDesligada;
        }

        DefinirAlavancaVisivel(true);
        estado = EstadoAlavanca.InstaladaDesligada;
        AtualizarTexto("Alavanca instalada. Pressione E novamente.");
    }

    private IEnumerator LigarEnergia()
    {
        Quaternion inicio = alavancaInstalada != null
            ? alavancaInstalada.localRotation
            : Quaternion.identity;

        float tempo = 0f;

        while (tempo < duracaoMovimento)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracaoMovimento);
            progresso = Mathf.SmoothStep(0f, 1f, progresso);

            if (alavancaInstalada != null)
            {
                alavancaInstalada.localRotation = Quaternion.Slerp(
                    inicio,
                    rotacaoLigada,
                    progresso
                );
            }

            yield return null;
        }

        if (alavancaInstalada != null)
        {
            alavancaInstalada.localRotation = rotacaoLigada;
        }

        estado = EstadoAlavanca.Ligada;
        DefinirIluminacao(true);
        AtualizarTexto("Energia ligada!");
        animacao = null;
    }

    private void DefinirAlavancaVisivel(bool visivel)
    {
        if (renderizadoresAlavanca == null)
        {
            return;
        }

        foreach (Renderer renderizador in renderizadoresAlavanca)
        {
            if (renderizador != null)
            {
                renderizador.enabled = visivel;
            }
        }
    }

    private void DefinirIluminacao(bool ligada)
    {
        if (luzAmbiente != null)
        {
            luzAmbiente.intensity = ligada
                ? intensidadeAmbienteLigada
                : intensidadeAmbienteDesligada;
        }

        if (luzesPrincipais == null)
        {
            return;
        }

        foreach (Light luz in luzesPrincipais)
        {
            if (luz != null)
            {
                luz.enabled = ligada;
            }
        }
    }

    private void AtualizarTexto(string mensagem)
    {
        if (textoStatus != null)
        {
            textoStatus.text = mensagem;
        }

        Debug.Log(mensagem, this);
    }
}
