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

    private bool controle = true;

    private void Awake()
    {
        BackAction = InputSystem.actions.FindAction("Interaction/Back");
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
            playerPosition = player.position;
            playerRotation = player.rotation;
        if (controle)
        {
            playerController.SetMovementEnabled(false);
            player.SetPositionAndRotation(tranca.position, tranca.rotation);
            controle = false;
        }
    }

    public void Back()
    {
      
            player.SetPositionAndRotation(playerPosition, playerRotation);
            playerController.SetMovementEnabled(true);
            controle = true;
       
    }

}

