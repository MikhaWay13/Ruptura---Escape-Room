using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [SerializeField] private string nomeDoLevelDeJogo;
    [SerializeField] private GameObject Canvas;

    [SerializeField] private GameObject painelMenuInicial;
    [SerializeField] private GameObject painelOpcoes;
    [SerializeField] private GameObject painelCreditos;
    [SerializeField] private GameObject painelSair;
    public bool PlayGame=false;
    public void Jogar()
    {
        PlayGame=true;
        Canvas.SetActive(false); 
        print("Cutscene iniciada");
    }


void Start(){
    painelMenuInicial.SetActive(true);
        painelOpcoes.SetActive(false);
        painelCreditos.SetActive(false);
        painelSair.SetActive(false);
}

//opções
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

//Créditos
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

    //saida
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
        Debug.Log("Sair");
        painelMenuInicial.SetActive(true);
        painelSair.SetActive(false);
        Application.Quit();
        #endif
        Application.Quit();
    }


}
