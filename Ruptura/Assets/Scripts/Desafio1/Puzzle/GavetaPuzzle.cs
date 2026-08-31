using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GavetaPuzzle : MonoBehaviour
{
    [Header("Referências")]
    [SerializeField] private Transform gavetaMovel;

    [Header("Movimento")]
    [SerializeField] private Vector3 deslocamentoAberta = new Vector3(-0.7f, 0f, 0f);
    [SerializeField] private float duracaoMovimento = 0.55f;

    public bool EstaAberta => aberta && animacao == null;

    private Vector3 posicaoFechada;
    private Vector3 posicaoAberta;
    private bool aberta;
    private Coroutine animacao;

    private void Awake()
    {
        if (gavetaMovel == null)
        {
            Debug.LogError("GavetaPuzzle: a referência da gaveta móvel não foi configurada.", this);
            enabled = false;
            return;
        }

        posicaoFechada = gavetaMovel.localPosition;
        posicaoAberta = posicaoFechada + deslocamentoAberta;
    }

    public void Abrir()
    {
        if (aberta || animacao != null)
        {
            return;
        }

        aberta = true;
        animacao = StartCoroutine(MoverGaveta(posicaoAberta));
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

            gavetaMovel.localPosition = Vector3.Lerp(origem, destino, progresso);

            yield return null;
        }

        gavetaMovel.localPosition = destino;
        animacao = null;
    }
}