using UnityEngine;
using UnityEngine.SceneManagement;

public class CofreManager : MonoBehaviour, IRaycastInteractable
{
    public static CofreManager instance; //singleton

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform player;
    [SerializeField] private Transform tranca;
    [SerializeField] private Transform cameraPlayer;

    private Vector3 playerPosition;

    private void Start()
    {

    }

    private void Update()
    {
        playerPosition = player.position;
    }

    public void Interact()
    {


        cameraPlayer.transform.position = tranca.position;

    }

    public void Back()
    {

    }

}

