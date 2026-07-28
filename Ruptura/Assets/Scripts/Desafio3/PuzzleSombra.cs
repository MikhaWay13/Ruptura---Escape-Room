using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PuzzleSombra : MonoBehaviour
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
    [SerializeField]
    private float velocidadeRotacao = 90f;

    [SerializeField]
    [Range(0.1f, 30f)]
    private float toleranciaRotacao = 6f;

    [Header("Configuração da estante")]
    [SerializeField]
    private Vector3 deslocamentoEstante = new Vector3(2.5f, 0f, 0f);

    [SerializeField]
    private float velocidadeEstante = 2f;

    private bool puzzleConcluido;

    private Vector3 posicaoFechadaEstante;
    private Vector3 posicaoAbertaEstante;

    private void Awake()
    {
            posicaoFechadaEstante = estante.position;
            posicaoAbertaEstante =
                posicaoFechadaEstante + deslocamentoEstante;
    
    }

    private void OnEnable()
    {
            rotateAction.action.Enable();
        
    }

    private void OnDisable()
    {

            rotateAction.action.Disable();
        
    }

    private void Start()
    {
        AtualizarTexto("Use A e D para ajustar a sombra");
    }

    private void Update()
    {
        if (puzzleConcluido)
        {
            AbrirEstante();
            return;
        }

        GirarEstatueta();
        VerificarRotacao();
    }

    private void GirarEstatueta()
    {
        // A retorna -1, D retorna 1 e nenhuma tecla retorna 0.
        float entrada = rotateAction.action.ReadValue<float>();

        float quantidadeRotacao =
            entrada * velocidadeRotacao * Time.deltaTime;

        transform.Rotate(
            Vector3.up,
            quantidadeRotacao,
            Space.Self
        );
    }

    private void VerificarRotacao()
    {

        // Calcula a diferença entre o ângulo atual
        // e o ângulo considerado correto.
        float diferenca = Quaternion.Angle(
            transform.rotation,
            alvoRotacao.rotation
        );

        if (textoStatus != null)
        {
            textoStatus.SetText(
                "Use A e D para girar\nDiferença: {0:0} graus",
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

        // Encaixa exatamente na rotação correta.
        transform.rotation = alvoRotacao.rotation;

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
         textoStatus.text = mensagem;
    }
}
