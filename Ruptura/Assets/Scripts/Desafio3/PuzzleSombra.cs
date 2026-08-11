using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleSombra : MonoBehaviour, IRaycastInteractable
{
    [Header("Input System")]
    [SerializeField]
    private InputActionReference rotateAction;

    [Header("Referências da cena")]
    [SerializeField]
    private Transform alvoRotacao;

    [SerializeField]
    private Transform estante;

    [SerializeField]
    private TMP_Text textoStatus;

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
    private bool referenciasValidas;
    private PlayerController jogadorAtivo;
    private float anguloHorizontal;
    private float anguloVertical;

    private Vector3 posicaoFechadaEstante;
    private Vector3 posicaoAbertaEstante;

    private void Awake()
    {
        referenciasValidas =
            rotateAction != null &&
            rotateAction.action != null &&
            alvoRotacao != null &&
            estante != null;

        if (!referenciasValidas)
        {
            Debug.LogError(
                "PuzzleSombra está sem Rotate Action, Alvo Rotação ou Estante.",
                this
            );
            enabled = false;
            return;
        }

        posicaoFechadaEstante = estante.position;
        posicaoAbertaEstante =
            posicaoFechadaEstante + deslocamentoEstante;
    }

    private void OnDisable()
    {
        if (rotateAction != null && rotateAction.action != null)
        {
            rotateAction.action.Disable();
        }

        LiberarJogador();
    }

    private void Start()
    {
        AtualizarTexto("Aponte para a estatueta e pressione E");
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

        GirarEstatueta();
        VerificarRotacao();
    }

    public void Interact(PlayerController player)
    {
        if (!referenciasValidas || puzzleConcluido)
        {
            return;
        }

        if (puzzleAtivo)
        {
            EncerrarAjuste();
        }
        else
        {
            IniciarAjuste(player);
        }
    }

    private void IniciarAjuste(PlayerController player)
    {
        puzzleAtivo = true;
        jogadorAtivo = player;
        anguloHorizontal = transform.eulerAngles.y;
        anguloVertical = NormalizarAngulo(transform.eulerAngles.x);
        jogadorAtivo.SetGameplayControlEnabled(false);
        rotateAction.action.Enable();

        AtualizarTexto("Mouse: girar  |  E: sair");
    }

    private void EncerrarAjuste()
    {
        puzzleAtivo = false;
        rotateAction.action.Disable();
        LiberarJogador();

        AtualizarTexto("Aponte para a estatueta e pressione E");
    }

    private void GirarEstatueta()
    {
        Vector2 entrada = rotateAction.action.ReadValue<Vector2>();

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

    private void VerificarRotacao()
    {
        float diferenca = Quaternion.Angle(
            transform.rotation,
            alvoRotacao.rotation
        );

        if (textoStatus != null)
        {
            textoStatus.SetText(
                "Mouse: girar  |  E: sair\nDiferença: {0:0} graus",
                diferenca
            );
        }

        if (diferenca <= toleranciaRotacao)
        {
            ConcluirPuzzle();
        }
    }

    private void ConcluirPuzzle()
    {
        puzzleConcluido = true;
        puzzleAtivo = false;
        rotateAction.action.Disable();

        transform.rotation = alvoRotacao.rotation;
        LiberarJogador();

        AtualizarTexto(
            "Sombra correta!\nEstante destravada."
        );
    }

    private void AbrirEstante()
    {
        estante.position = Vector3.MoveTowards(
            estante.position,
            posicaoAbertaEstante,
            velocidadeEstante * Time.deltaTime
        );
    }

    private void AtualizarTexto(string mensagem)
    {
        if (textoStatus != null)
        {
            textoStatus.text = mensagem;
        }
    }

    private void LiberarJogador()
    {
        if (jogadorAtivo != null)
        {
            jogadorAtivo.SetGameplayControlEnabled(true);
            jogadorAtivo = null;
        }
    }

    private static float NormalizarAngulo(float angulo)
    {
        return angulo > 180f ? angulo - 360f : angulo;
    }
}
