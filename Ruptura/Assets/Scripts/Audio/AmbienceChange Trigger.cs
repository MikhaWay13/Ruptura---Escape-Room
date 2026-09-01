using UnityEngine;

public class AmbienceChangeTrigger : MonoBehaviour
{
  [Header("Parâmetros de Ambiência")]

    [SerializeField] private string parameterName;
    [SerializeField] private float parameterValue;

    private void OnTriggerEnter(Collider other)
{
    // Verifica se o objeto que ENTROU no gatilho tem a tag "Player"
    if (other.CompareTag("Player"))
    {
        AudioManager.instance.SetAmbienceParameter(parameterName, parameterValue);
        Debug.Log("Parâmetro de ambiência alterado: " + parameterName + " para o valor: " + parameterValue);
    }
}
}
