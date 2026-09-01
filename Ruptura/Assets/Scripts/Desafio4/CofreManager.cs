using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class CofreManager : MonoBehaviour, IRaycastInteractable
{
    public static CofreManager instance; //singleton

    [SerializeField] private PlayerController playerController;
    [SerializeField] private Transform player;
    [SerializeField] private Transform tranca;

    private Vector3 playerPosition;
    private Quaternion playerRotation;

    private InputAction BackAction;
    private CharacterController characterController;

    private bool controle = true;

    private void Awake()
    {
        BackAction = InputSystem.actions.FindAction("Interaction/Back");

        characterController = player.GetComponent<CharacterController>(); 
    }

    private void Update()
    {
        if(!controle && BackAction != null && BackAction.WasPressedThisFrame())
        {
            Back();
        }
    }

    public void Interact()
    {
      

        if (controle)
        {
            playerPosition = player.position;
            playerRotation = player.rotation;
          

            playerController.SetMovementEnabled(false);
            TeleportarPlayer(tranca.position, tranca.rotation);
            controle = false;
       
        }
    }

    public void Back()
    {

        TeleportarPlayer(playerPosition, playerRotation);
   
        playerController.SetMovementEnabled(true);
            controle = true;
   
       
    }

    private void TeleportarPlayer(Vector3 posicao, Quaternion rotacao)
    {
        characterController.enabled = false;

        player.SetPositionAndRotation(posicao, rotacao);
        Physics.SyncTransforms();

        characterController.enabled = true;
    }

}

