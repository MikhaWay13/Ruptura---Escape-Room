using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] private Slider LoadingSlider;
    [SerializeField] private TextMeshProUGUI LoadingText;

    private void Start()
    {
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
{
    // Inicia o carregamento
    AsyncOperation operation = SceneManager.LoadSceneAsync("Jogo");
    
    // Proíbe a Unity de mudar de cena automaticamente quando terminar de carregar
    operation.allowSceneActivation = false;

    float progressoVisual = 0f;

    while (!operation.isDone)
    {
        // O progresso real da Unity vai até 0.9
        float progressoReal = Mathf.Clamp01(operation.progress / 0.9f);
        
        // Move o progresso visual aos poucos em direção ao progresso real (cria uma animação fluida)
        // O "1.5f" no final é a velocidade da barra. Mude para deixar mais rápido ou devagar.
        progressoVisual = Mathf.MoveTowards(progressoVisual, progressoReal, Time.deltaTime * 1.5f);

        // Atualiza a UI com o progresso suave
        LoadingSlider.value = progressoVisual;
        LoadingText.text = (progressoVisual * 100f).ToString("F0") + "%";

        // Quando a barra visual finalmente chegar no final (100%)...
        if (progressoVisual >= 1f)
        {
            // Liberamos a Unity para abrir a nova cena
            operation.allowSceneActivation = true;
        }

        yield return null;
    }
}
}