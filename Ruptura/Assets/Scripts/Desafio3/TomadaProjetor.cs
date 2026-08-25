using UnityEngine;

[DisallowMultipleComponent]
public class TomadaProjetor : MonoBehaviour, IRaycastInteractable
{
    [Header("Referências")]
    [SerializeField] private CaboProjetor cabo;
    [SerializeField] private Transform pontoEncaixe;
    [SerializeField] private Light luzProjetor;
    [SerializeField] private PuzzleSombra puzzleSombra;

    private bool conectado;

    public bool EstaConectada => conectado;

    private void Awake()
    {
        luzProjetor.enabled = false;
    }

    public void Interact()
    {
        if (conectado)
        {
            return;
        }

        if (!cabo.EstaSegurando)
        {
            Debug.Log("Pegue o plugue antes de usar a tomada.", this);
            return;
        }

        // O PlayerInteraction já limita o alcance. Se o jogador estiver
        // segurando o plugue e mirando na tomada, ela encaixa automaticamente.
        conectado = true;
        cabo.Conectar(pontoEncaixe);
        LigarProjetor();
        Debug.Log("Projetor conectado e ligado.", this);
    }

    private void LigarProjetor()
    {
        luzProjetor.enabled = true;
        puzzleSombra.AtivarProjetor();
    }
}
