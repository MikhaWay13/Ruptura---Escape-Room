using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class GavetaInterativa : MonoBehaviour, IRaycastInteractable
{
    [Header("Referências")]
    [SerializeField] private Transform gavetaMovel;
    [SerializeField] private TMP_Text textoStatus;

    [Header("Movimento")]
    [SerializeField] private Vector3 deslocamentoAberta = new Vector3(-0.7f, 0f, 0f);
    [SerializeField, Min(0.05f)] private float duracaoMovimento = 0.55f;

    public bool EstaAberta => aberta && animacao == null;

    private Vector3 posicaoFechada;
    private Vector3 posicaoAberta;
    private bool aberta;
    private Coroutine animacao;

    private void Awake()
    {
        posicaoFechada = gavetaMovel.localPosition;
        posicaoAberta = posicaoFechada + deslocamentoAberta;
    }

    public void Interact()
    {
        if (animacao != null)
        {
            return;
        }

        aberta = !aberta;
        animacao = StartCoroutine(MoverGaveta(
            aberta ? posicaoAberta : posicaoFechada
        ));
    }

    private IEnumerator MoverGaveta(Vector3 destino)
    {
        Vector3 origem = gavetaMovel.localPosition;
        float tempo = 0f;

        while (tempo < duracaoMovimento)
        {
            tempo += Time.deltaTime;
            float progresso = Mathf.Clamp01(tempo / duracaoMovimento);
            progresso = Mathf.SmoothStep(0f, 1f, progresso);

            gavetaMovel.localPosition = Vector3.Lerp(
                origem,
                destino,
                progresso
            );

            yield return null;
        }

        gavetaMovel.localPosition = destino;
        animacao = null;
        AtualizarTexto();
    }

    private void AtualizarTexto()
    {
        textoStatus.text = aberta
            ? "Alavanca encontrada! Clique esquerdo para inspecionar."
            : "Encontre a alavanca dentro da gaveta.";
    }
}
