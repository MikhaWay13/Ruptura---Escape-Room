using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{

    [SerializeField] private GameObject Canvas;
    [SerializeField] private GameObject Gatilho;
    [SerializeField] private PlayableDirector cutsceneInicial;

    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private GameObject painelSair;
    [SerializeField] private GameObject audio;

    public bool PlayGame = false;

    private void Start()
    {
        painelMenuInicial.SetActive(true);
        painelOpcoes.SetActive(false);
        painelCreditos.SetActive(false);
        painelSair.SetActive(false);
        audio.SetActive(true);
        Gatilho.SetActive(false);

        
        cutsceneInicial.stopped += FinalizarCutscene;
    }

    public void Jogar()
    {
        PlayGame = true;
        audio.SetActive(false);


        Canvas.SetActive(false);
        Gatilho.SetActive(true);

        cutsceneInicial.Play();

        Debug.Log("Cutscene iniciada");
    }

    private void FinalizarCutscene(PlayableDirector director)
    {
        SceneManager.LoadScene("Loading");
    }

    // Opções
    public void AbrirOpcoes()
    {
        painelMenuInicial.SetActive(false);
        painelOpcoes.SetActive(true);
    }

    public void FecharOpcoes()
    {
        painelMenuInicial.SetActive(true);
        painelOpcoes.SetActive(false);
    }

    // Créditos
    public void AbrirCreditos()
    {
        painelMenuInicial.SetActive(false);
        painelCreditos.SetActive(true);
    }

    public void FecharCreditos()
    {
        painelMenuInicial.SetActive(true);
        painelCreditos.SetActive(false);
    }

    // Saída
    public void AbrirConfirmacao()
    {
        painelMenuInicial.SetActive(false);
        painelSair.SetActive(true);
    }

    public void Fecharconfirmacao()
    {
        painelMenuInicial.SetActive(true);
        painelSair.SetActive(false);
    }

    public void Menu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void SairJogo()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}