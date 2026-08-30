using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Câmera")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private float lookSensitivity = 0.1f;
    [SerializeField] private float minPitch = -80f;
    [SerializeField] private float maxPitch = 80f;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction PlayerInventory; //abrir o inventário
   // private InputAction UIInventory; //fechar o inventário

    private CharacterController controller;

    private Vector3 velocity;
    private float pitch;

    private bool gameplayControlEnabled = true;

    private bool verify;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        PlayerInventory = InputSystem.actions.FindAction("Player/Inventory");

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        InputSystem.actions
            .FindActionMap("Player")
            .Enable();
    }

    private void OnDisable()
    {
        InputSystem.actions
            .FindActionMap("Player")
            .Disable();
    }

    private void Update()
    {
        Inventario();


        if (!gameplayControlEnabled)
        {
            return;
        }

        Move();


    }


    private void LateUpdate()
    {
        if (!gameplayControlEnabled)
            return;
        Look();
    }



private void Inventario()
{
    if (PlayerInventory.WasPressedThisFrame())
    {
        // Se o controle do jogador está ATIVO, significa que o inventário está FECHADO e vamos ABRIR
        if (gameplayControlEnabled)
        {
            SetGameplayControlEnabled(false);      // Desativa movimentação
            UIManager.instance.SetInventory(true); // Abre o inventário visualmente
        }
        // Se o controle está DESATIVADO, significa que o inventário está ABERTO e vamos FECHAR
        else
        {
            SetGameplayControlEnabled(true);        // Reativa movimentação
            UIManager.instance.SetInventory(false); // Fecha o inventário visualmente
        }
    }
}




    public void SetGameplayControlEnabled(bool isEnabled)
    {
        gameplayControlEnabled = isEnabled;

        if (!isEnabled)
        {
            velocity = Vector3.zero;
        }
    }

    private void Look()
    {
        if (lookAction == null)
            return;

        Vector2 look =
            lookAction.ReadValue<Vector2>() *
            lookSensitivity;

        transform.Rotate(Vector3.up * look.x);

        pitch -= look.y;

        pitch = Mathf.Clamp(
            pitch,
            minPitch,
            maxPitch
        );

        if (cameraTarget != null)
        {
            cameraTarget.localRotation =
                Quaternion.Euler(
                    pitch,
                    0f,
                    0f
                );
        }
    }

    private void Move()
    {
        if (controller == null)
            return;

        bool grounded = controller.isGrounded;

        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        if (jumpAction != null &&
            jumpAction.WasPressedThisFrame() &&
            grounded)
        {
            velocity.y =
                Mathf.Sqrt(
                    jumpHeight * -2f * gravity
                );
        }

        velocity.y += gravity * Time.deltaTime;

        Vector2 input =
            moveAction != null
                ? moveAction.ReadValue<Vector2>()
                : Vector2.zero;

        Vector3 move =
            transform.right * input.x +
            transform.forward * input.y;

        controller.Move(
            (move * moveSpeed + velocity) *
            Time.deltaTime
        );
    }
}