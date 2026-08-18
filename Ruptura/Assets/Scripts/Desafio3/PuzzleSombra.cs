using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleSombra : MonoBehaviour, IRaycastInteractable
{
    [Header("Referências da cena")]
    [SerializeField]
    private Transform alvoRotacao;

    [SerializeField]
    private Transform estante;

    [SerializeField]
    private TMP_Text textoStatus;

    [SerializeField]
    private PlayerController jogador;

    [Header("Estado do projetor")]
    [SerializeField]
    private bool projetorLigadoNoInicio;

    [Header("Configuração da estatueta")]
    [SerializeField, Min(0.001f)]
    private float sensibilidadeMouse = 0.15f;

    [SerializeField, Range(0f, 89f)]
    private float limiteVertical = 80f;

    [SerializeField]
    [Range(0.1f, 30f)]
    private float toleranciaRotacao = 6f;

    [Header("Configuração da estante")]
    [SerializeField]
    private Vector3 deslocamentoEstante = new Vector3(2.5f, 0f, 0f);

    [SerializeField]
    private float velocidadeEstante = 2f;

    private bool puzzleConcluido;
    private bool puzzleAtivo;
    private bool projetorLigado;
    private float anguloHorizontal;
    private float anguloVertical;

    private Vector3 posicaoAbertaEstante;
    private InputAction lookAction;
    private InputAction backAction;

    public bool ProjetorLigado => projetorLigado;
    public bool Concluido => puzzleConcluido;

    private void Awake()
    {
        lookAction = InputSystem.actions.FindAction("Interaction/Look");
        backAction = InputSystem.actions.FindAction("Interaction/Back");
        if (estante != null)
        {
            posicaoAbertaEstante = estante.position + deslocamentoEstante;
        }

        projetorLigado = projetorLigadoNoInicio;
    }

    private void Start()
    {
        AtualizarTexto(
            projetorLigado
                ? "Aponte para a estatueta e pressione E"
                : "Conecte o cabo para ligar o projetor"
        );
    }

    private void Update()
    {
        if (puzzleConcluido && estante != null)
        {
            estante.position = Vector3.MoveTowards(
                estante.position,
                posicaoAbertaEstante,
                velocidadeEstante * Time.deltaTime
            );
            return;
        }

        if (!puzzleAtivo)
        {
            return;
        }

        if (backAction != null && backAction.WasPressedThisFrame())
        {
            EncerrarAjuste();
            return;
        }

        Vector2 entrada = lookAction != null
            ? lookAction.ReadValue<Vector2>()
            : Vector2.zero;

        anguloHorizontal += entrada.x * sensibilidadeMouse;
        anguloVertical = Mathf.Clamp(
            anguloVertical - entrada.y * sensibilidadeMouse,
            -limiteVertical,
            limiteVertical
        );

        transform.rotation = Quaternion.Euler(
            anguloVertical,
            anguloHorizontal,
            0f
        );

        float diferenca = Quaternion.Angle(
            transform.rotation,
            alvoRotacao.rotation
        );

        if (textoStatus != null)
        {
            textoStatus.SetText(
                "Mouse: girar  |  Botão direito: sair\nDiferença: {0:0} graus",
                diferenca
            );
        }

        if (diferenca <= toleranciaRotacao)
        {
            ConcluirPuzzle();
        }
    }

    public void Interact()
    {
        if (puzzleAtivo || puzzleConcluido)
        {
            return;
        }

        if (!projetorLigado)
        {
            AtualizarTexto("Conecte o cabo para ligar o projetor");
            return;
        }

        puzzleAtivo = true;
        anguloHorizontal = transform.eulerAngles.y;
        anguloVertical = NormalizarAngulo(transform.eulerAngles.x);
        if (jogador != null)
        {
            jogador.enabled = false;
        }

        AtualizarTexto("Mouse: girar  |  Botão direito: sair");
    }

    public void AtivarProjetor()
    {
        projetorLigado = true;

        if (!puzzleAtivo && !puzzleConcluido)
        {
            AtualizarTexto("Projetor ligado! Aponte para a estatueta e pressione E");
        }
    }

    private void EncerrarAjuste()
    {
        puzzleAtivo = false;
        if (jogador != null)
        {
            jogador.enabled = true;
        }
        AtualizarTexto("Aponte para a estatueta e pressione E");
    }

    private void ConcluirPuzzle()
    {
        puzzleConcluido = true;
        puzzleAtivo = false;
        transform.rotation = alvoRotacao.rotation;
        if (jogador != null)
        {
            jogador.enabled = true;
        }

        AtualizarTexto("Sombra correta!\nEstante destravada.");
    }

    private void OnDisable()
    {
        if (puzzleAtivo && jogador != null)
        {
            jogador.enabled = true;
            puzzleAtivo = false;
        }
    }

    private void AtualizarTexto(string mensagem)
    {
        if (textoStatus != null)
        {
            textoStatus.text = mensagem;
        }
    }

    private static float NormalizarAngulo(float angulo)
    {
        return angulo > 180f ? angulo - 360f : angulo;
    }
}
