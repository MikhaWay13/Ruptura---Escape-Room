using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class CaboProjetor : MonoBehaviour, IRaycastInteractable
{
    [Header("Referências")]
    [SerializeField]
    private Transform pontoProjetor;

    [SerializeField]
    private Transform pontoMao;

    [SerializeField]
    private Rigidbody corpo;

    [SerializeField]
    private SpringJoint junta;

    [SerializeField]
    private BoxCollider colisao;

    [SerializeField]
    private LineRenderer linha;

    [Header("Disponibilidade")]
    [SerializeField]
    private bool liberadoNoInicio = true;

    [Header("Física")]
    [SerializeField, Min(0.1f)]
    private float comprimentoMaximo = 4f;

    [SerializeField, Min(0f)]
    private float forcaSeguir = 60f;

    [SerializeField, Min(0f)]
    private float amortecimento = 10f;

    [Header("Visual do cabo")]
    [SerializeField, Min(0f)]
    private float curvatura = 0.35f;

    public bool EstaSegurando { get; private set; }
    public bool EstaConectado { get; private set; }

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
        if (!EstaSegurando || EstaConectado || interagirAction == null)
        {
            return;
        }

        // O primeiro E vem do PlayerInteraction e pega o plugue. Os próximos
        // soltam o plugue, exceto quando o jogador está mirando na tomada.
        if (Time.frameCount != quadroEmQuePegou &&
            interagirAction.WasPressedThisFrame() &&
            !EstaMirandoNaTomada())
        {
            Soltar();
        }
    }

    private void FixedUpdate()
    {
        if (!EstaSegurando || pontoMao == null)
        {
            return;
        }

        Vector3 erroPosicao = pontoMao.position - corpo.position;
        Vector3 forca =
            erroPosicao * forcaSeguir - corpo.linearVelocity * amortecimento;

        corpo.AddForce(forca, ForceMode.Acceleration);
    }

    private void LateUpdate()
    {
        DesenharCabo();
    }

    public void Interact()
    {
        if (!liberado || EstaConectado)
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
        liberado = true;
    }

    public void Conectar(Transform snapPoint)
    {
        if (EstaConectado || snapPoint == null)
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

        transform.SetPositionAndRotation(
            snapPoint.position,
            snapPoint.rotation
        );

        // O encaixe é permanente; o collider deixa de receber interações.
        colisao.enabled = false;
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
        if (linha == null || pontoProjetor == null ||
            linha.positionCount < 2)
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
            Vector3 ponto = Vector3.Lerp(inicio, fim, t);
            ponto += Vector3.down * (Mathf.Sin(Mathf.PI * t) * queda);
            linha.SetPosition(i, ponto);
        }
    }

    private bool EstaMirandoNaTomada()
    {
        Camera cameraPrincipal = Camera.main;

        if (cameraPrincipal == null)
        {
            return false;
        }

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
