using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleSombra : MonoBehaviour, IRaycastInteractable
{
    [Header("Referências da cena")]
    [SerializeField] private Transform alvoRotacao;
    [SerializeField] private Transform estante;
    [SerializeField] private TMP_Text textoStatus;
    [SerializeField] private PlayerController jogador;
    [SerializeField] private GameObject referenciaSilhueta;

    [Header("Estado do projetor")]
    [SerializeField] private bool projetorLigadoNoInicio;

    [Header("Configuração da estatueta")]
    [SerializeField, Min(0.001f)] private float sensibilidadeMouse = 0.15f;
    [SerializeField, Range(0f, 89f)] private float limiteVertical = 80f;
    [SerializeField, Range(0.1f, 30f)] private float toleranciaRotacao = 6f;

    [Header("Configuração da estante")]
    [SerializeField] private Vector3 deslocamentoEstante = new Vector3(2.5f, 0f, 0f);
    [SerializeField] private float velocidadeEstante = 2f;

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

        posicaoAbertaEstante = estante.position + deslocamentoEstante;

        projetorLigado = projetorLigadoNoInicio;
        MostrarReferencia(projetorLigado);
    }

    private void Start()
    {
        AtualizarTexto(
            projetorLigado
                ? "Alinhe a sombra\nAponte para o macaco e pressione E."
                : "Ligue o projetor\nConecte o cabo à tomada."
        );
    }

    private void Update()
    {
        if (puzzleConcluido)
        {
            AbrirEstante();
            return;
        }

        if (!puzzleAtivo)
        {
            return;
        }

        if (JogadorPediuParaSair())
        {
            EncerrarAjuste();
            return;
        }

        GirarEstatueta();
        VerificarSombra();
    }

    public void Interact()
    {
        if (puzzleAtivo || puzzleConcluido)
        {
            return;
        }

        if (!projetorLigado)
        {
            AtualizarTexto("Ligue o projetor\nConecte o cabo à tomada.");
            return;
        }

        IniciarAjuste();
    }

    public void AtivarProjetor()
    {
        projetorLigado = true;
        MostrarReferencia(true);

        if (!puzzleAtivo && !puzzleConcluido)
        {
            AtualizarTexto(
                "Alinhe a sombra\nAponte para o macaco e pressione E."
            );
        }
    }

    private void AbrirEstante()
    {
        estante.position = Vector3.MoveTowards(
            estante.position,
            posicaoAbertaEstante,
            velocidadeEstante * Time.deltaTime
        );
    }

    private bool JogadorPediuParaSair()
    {
        return backAction.WasPressedThisFrame();
    }

    private void GirarEstatueta()
    {
        Vector2 entrada = lookAction.ReadValue<Vector2>();

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
    }

    private void VerificarSombra()
    {
        float diferenca = Quaternion.Angle(
            transform.rotation,
            alvoRotacao.rotation
        );

        AtualizarDicaDeAlinhamento(diferenca);

        if (diferenca <= toleranciaRotacao)
        {
            ConcluirPuzzle();
        }
    }

    private void IniciarAjuste()
    {
        puzzleAtivo = true;
        anguloHorizontal = transform.eulerAngles.y;
        anguloVertical = NormalizarAngulo(transform.eulerAngles.x);
        DefinirControleDoJogador(false);
        AtualizarTexto(
            "Faça a sombra cobrir a silhueta\n" +
            "Mova o mouse  •  Botão direito: sair"
        );
    }

    private void EncerrarAjuste()
    {
        puzzleAtivo = false;
        DefinirControleDoJogador(true);
        AtualizarTexto(
            "Alinhe a sombra\nAponte para o macaco e pressione E."
        );
    }

    private void ConcluirPuzzle()
    {
        puzzleConcluido = true;
        puzzleAtivo = false;
        transform.rotation = alvoRotacao.rotation;
        DefinirControleDoJogador(true);
        MostrarReferencia(false);
        AtualizarTexto("Sombra alinhada!\nA estante foi destravada.");
    }

    private void OnDisable()
    {
        if (puzzleAtivo)
        {
            puzzleAtivo = false;
            DefinirControleDoJogador(true);
        }
    }

    private void DefinirControleDoJogador(bool ativo)
    {
        jogador.enabled = ativo;
    }

    private void AtualizarTexto(string mensagem)
    {
        textoStatus.text = mensagem;
    }

    private void AtualizarDicaDeAlinhamento(float diferenca)
    {
        if (diferenca <= toleranciaRotacao * 2f)
        {
            AtualizarTexto(
                "Quase! Faça um ajuste pequeno\n" +
                "Mova o mouse  •  Botão direito: sair"
            );
        }
        else if (diferenca <= 45f)
        {
            AtualizarTexto(
                "A sombra está se aproximando\n" +
                "Continue movendo o mouse"
            );
        }
        else
        {
            AtualizarTexto(
                "Faça a sombra cobrir a silhueta\n" +
                "Mova o mouse  •  Botão direito: sair"
            );
        }
    }

    private void MostrarReferencia(bool mostrar)
    {
        referenciaSilhueta.SetActive(mostrar);
    }

    private static float NormalizarAngulo(float angulo)
    {
        return angulo > 180f ? angulo - 360f : angulo;
    }
}
