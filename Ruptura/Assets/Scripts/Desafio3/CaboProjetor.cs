using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class CaboProjetor : MonoBehaviour, IRaycastInteractable
{
    [Header("Referências")]
    [SerializeField] private Transform pontoProjetor;
    [SerializeField] private Transform pontoMao;
    [SerializeField] private Rigidbody corpo;
    [SerializeField] private SpringJoint junta;
    [SerializeField] private BoxCollider colisao;
    [SerializeField] private LineRenderer linha;

    [Header("Disponibilidade")]
    [SerializeField] private bool liberadoNoInicio = true;

    [Header("Física")]
    [SerializeField, Min(0.1f)] private float comprimentoMaximo = 4f;
    [SerializeField, Min(0f)] private float forcaSeguir = 60f;
    [SerializeField, Min(0f)] private float amortecimento = 10f;

    [Header("Visual do cabo")]
    [SerializeField, Min(0f)] private float curvatura = 0.35f;

    public bool EstaSegurando { get; private set; }
    public bool EstaConectado { get; private set; }
    public bool EstaLiberado => liberado;

    private bool liberado;
    private int quadroEmQuePegou = -1;
    private InputAction interagirAction;

    private void Awake()
    {
        liberado = liberadoNoInicio;
        interagirAction = InputSystem.actions.FindAction("Interaction/Interact");
    }

    private void Update()
    {
        if (DeveSoltarCabo())
        {
            Soltar();
        }
    }

    private void FixedUpdate()
    {
        if (!EstaSegurando)
        {
            return;
        }

        SeguirPontoDaMao();
    }

    private void LateUpdate()
    {
        DesenharCabo();
    }

    public void Interact()
    {
        if (!liberado)
        {
            Debug.Log("Ligue a energia antes de pegar o cabo.", this);
            return;
        }

        if (EstaConectado)
        {
            return;
        }

        if (EstaSegurando)
        {
            Soltar();
        }
        else
        {
            Pegar();
        }
    }

    public void LiberarCabo()
    {
        if (!EstaConectado)
        {
            liberado = true;
        }
    }

    public void Conectar(Transform snapPoint)
    {
        if (EstaConectado)
        {
            return;
        }

        EstaSegurando = false;
        EstaConectado = true;

        corpo.linearVelocity = Vector3.zero;
        corpo.angularVelocity = Vector3.zero;
        corpo.useGravity = false;
        corpo.isKinematic = true;

        Destroy(junta);
        junta = null;

        transform.SetPositionAndRotation(snapPoint.position, snapPoint.rotation);
        colisao.enabled = false;
    }

    private bool DeveSoltarCabo()
    {
        if (!EstaSegurando || EstaConectado)
        {
            return false;
        }

        // Ignora o mesmo E usado para pegar. Se estiver olhando para a tomada,
        // ela recebe o E e conecta o plugue em vez de soltá-lo.
        return Time.frameCount != quadroEmQuePegou &&
            interagirAction.WasPressedThisFrame() &&
            !EstaMirandoNaTomada();
    }

    private void SeguirPontoDaMao()
    {
        Vector3 erroPosicao = pontoMao.position - corpo.position;
        Vector3 forca =
            erroPosicao * forcaSeguir - corpo.linearVelocity * amortecimento;

        corpo.AddForce(forca, ForceMode.Acceleration);
    }

    private void Pegar()
    {
        EstaSegurando = true;
        quadroEmQuePegou = Time.frameCount;
        corpo.WakeUp();
    }

    private void Soltar()
    {
        EstaSegurando = false;
    }

    private void DesenharCabo()
    {
        if (linha.positionCount < 2)
        {
            return;
        }

        Vector3 inicio = pontoProjetor.position;
        Vector3 fim = transform.position;
        float distancia = Vector3.Distance(inicio, fim);
        float folga = 1f - Mathf.Clamp01(distancia / comprimentoMaximo);
        float queda = curvatura * Mathf.Lerp(0.25f, 1f, folga);

        for (int i = 0; i < linha.positionCount; i++)
        {
            float t = i / (float)(linha.positionCount - 1);
            linha.SetPosition(i, CalcularPontoDaLinha(inicio, fim, t, queda));
        }
    }

    private static Vector3 CalcularPontoDaLinha(
        Vector3 inicio,
        Vector3 fim,
        float t,
        float queda
    )
    {
        Vector3 pontoReto = Vector3.Lerp(inicio, fim, t);
        float curva = Mathf.Sin(Mathf.PI * t) * queda;
        return pontoReto + Vector3.down * curva;
    }

    private bool EstaMirandoNaTomada()
    {
        Camera cameraPrincipal = Camera.main;

        Ray raio = new Ray(
            cameraPrincipal.transform.position,
            cameraPrincipal.transform.forward
        );

        return Physics.Raycast(raio, out RaycastHit hit, 3f) &&
            hit.collider.GetComponentInParent<TomadaProjetor>() != null;
    }

    private void OnDisable()
    {
        EstaSegurando = false;
    }
}
