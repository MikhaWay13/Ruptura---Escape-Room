using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public GameObject FINAL;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FINAL.SetActive(false);
    }


    void OnTriggerEnter(Collider other)
    {
        FINAL.SetActive(true);
    }
}
