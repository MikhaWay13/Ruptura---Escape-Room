using UnityEngine;

[DisallowMultipleComponent]
public class TomadaProjetor : MonoBehaviour, IRaycastInteractable
{
    [Header("Referências")]
    [SerializeField]
    private CaboProjetor cabo;

    [SerializeField]
    private Transform pontoEncaixe;

    [SerializeField]
    private Light luzProjetor;

    [SerializeField]
    private PuzzleSombra puzzleSombra;

    [Header("Configuração")]
    [SerializeField, Min(0.1f)]
    private float distanciaEncaixe = 0.75f;

    private bool conectado;

    public bool EstaConectada => conectado;

    private void Awake()
    {
        if (luzProjetor != null)
        {
            luzProjetor.enabled = false;
        }
    }

    public void Interact()
    {
        if (conectado || cabo == null || pontoEncaixe == null)
        {
            return;
        }

        if (!cabo.EstaSegurando)
        {
            Debug.Log("Pegue o plugue antes de usar a tomada.", this);
            return;
        }

        float distancia = Vector3.Distance(
            cabo.transform.position,
            pontoEncaixe.position
        );

        if (distancia > distanciaEncaixe)
        {
            Debug.Log("Aproxime o plugue da tomada.", this);
            return;
        }

        conectado = true;
        cabo.Conectar(pontoEncaixe);

        if (luzProjetor != null)
        {
            luzProjetor.enabled = true;
        }

        if (puzzleSombra != null)
        {
            puzzleSombra.AtivarProjetor();
        }

        Debug.Log("Projetor conectado e ligado.", this);
    }
}
