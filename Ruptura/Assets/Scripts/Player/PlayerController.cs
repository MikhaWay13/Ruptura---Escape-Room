using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
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

    [Header("Passos")]
    [Tooltip("Tempo, em segundos, entre um passo e outro.")]
    [SerializeField, Min(0.1f)]
    private float footstepInterval = 0.45f;

    [Tooltip("Velocidade horizontal mínima para o som de passo tocar.")]
    [SerializeField, Min(0f)]
    private float minimumMovementSpeed = 0.1f;

    [Header("Head Bob")]
    [Tooltip("Movimento lateral da câmera durante a caminhada.")]
    [SerializeField, Min(0f)]
    private float headBobHorizontalAmount = 0.025f;

    [Tooltip("Movimento vertical da câmera durante a caminhada.")]
    [SerializeField, Min(0f)]
    private float headBobVerticalAmount = 0.04f;

    [Tooltip("Velocidade com que a câmera acompanha e retorna para a posição normal.")]
    [SerializeField, Min(0.1f)]
    private float headBobSmoothing = 12f;

    // Input System
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction playerInventory;

    // Componentes
    private CharacterController controller;

    // Movimento
    private Vector3 velocity;

    // Câmera
    private float pitch;
    private Vector3 cameraInitialLocalPosition;

    // Passos
    private float footstepTimer;

    // Head Bob
    private float headBobPhase;

    // Controle geral
    private bool gameplayControlEnabled = true;
    private bool inventoryOpen;

    // Cofre Américo
    private bool movementEnabled = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        moveAction =
            InputSystem.actions.FindAction("Move");

        lookAction =
            InputSystem.actions.FindAction("Look");

        jumpAction =
            InputSystem.actions.FindAction("Jump");

        playerInventory =
            InputSystem.actions.FindAction("Player/Inventory");

        // Salva a posição original da câmera.
        if (cameraTarget != null)
        {
            cameraInitialLocalPosition =
                cameraTarget.localPosition;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start()
    {
        if (AudioManager.instance == null)
        {
            Debug.LogError(
                "Não foi encontrado um AudioManager na cena.",
                this
            );
        }

        if (FMODEvent.instance == null)
        {
            Debug.LogError(
                "Não foi encontrado um FMODEvent na cena.",
                this
            );
        }
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

        ResetHeadBob(true);
    }

    private void Update()
    {
        Inventory();

        if (!gameplayControlEnabled)
        {
            ResetFootsteps();
            return;
        }

        if (movementEnabled)
        {
           Move();
           UpdateFootsteps();
        }
        else
        {
            velocity = Vector3.zero;
            ResetFootsteps();
        }
    }

    private void LateUpdate()
    {
        if (!gameplayControlEnabled)
            return;

        Look();

        if (movementEnabled)
        {
            UpdateHeadBob();
        }
    }

    // ==================================================
    // INVENTÁRIO
    // ==================================================

    private void Inventory()
    {
        if (playerInventory == null)
            return;

        if (!playerInventory.WasPressedThisFrame())
            return;

        ToggleInventory();
    }

    private void ToggleInventory()
    {
        if (inventoryOpen)
        {
            inventoryOpen = false;
            SetGameplayControlEnabled(true);
            UIManager.instance.SetInventory(false);
            return;
        }

        // Se outro sistema bloqueou o jogador, como a inspeção,
        // o inventário não pode ser aberto.
        if (!gameplayControlEnabled)
        {
            return;
        }

        inventoryOpen = true;
        SetGameplayControlEnabled(false);
        UIManager.instance.SetInventory(true);
    }

    public void SetGameplayControlEnabled(bool isEnabled)
    {
        gameplayControlEnabled = isEnabled;

        if (!isEnabled)
        {
            velocity = Vector3.zero;

            ResetFootsteps();
            ResetHeadBob(true);
        }
    }

    // ==================================================
    // CÂMERA
    // ==================================================

    private void Look()
    {
        if (lookAction == null)
            return;

        Vector2 look =
            lookAction.ReadValue<Vector2>() *
            lookSensitivity;

        // Rotação horizontal do jogador.
        transform.Rotate(
            Vector3.up * look.x
        );

        // Rotação vertical da câmera.
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

    // ==================================================
    // MOVIMENTO
    // ==================================================

    private void Move()
    {
        if (controller == null)
            return;

        bool grounded =
            controller.isGrounded;

        // Mantém o jogador encostado no chão.
        if (grounded && velocity.y < 0f)
        {
            velocity.y = -2f;
        }

        // Pulo.
        if (jumpAction != null &&
            jumpAction.WasPressedThisFrame() &&
            grounded)
        {
            velocity.y = Mathf.Sqrt(
                jumpHeight * -2f * gravity
            );
        }

        // Gravidade.
        velocity.y +=
            gravity * Time.deltaTime;

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

    // ==================================================
    // SOM DOS PASSOS
    // ==================================================

    private void UpdateFootsteps()
    {
        if (!IsGroundedAndMoving())
        {
            ResetFootsteps();
            return;
        }

        footstepTimer -= Time.deltaTime;

        if (footstepTimer > 0f)
            return;

        PlayFootstep();

        footstepTimer = footstepInterval;
    }

    private void PlayFootstep()
    {
        if (AudioManager.instance == null)
            return;

        if (FMODEvent.instance == null)
            return;

        AudioManager.instance.PlayOneShot(
            FMODEvent.instance.playerFootstepsEvent,
            transform.position
        );
    }

    private void ResetFootsteps()
    {
        // Faz o primeiro passo tocar imediatamente
        // quando o jogador começa a andar.
        footstepTimer = 0f;
    }

    // ==================================================
    // HEAD BOB
    // ==================================================

    private void UpdateHeadBob()
    {
        if (cameraTarget == null)
            return;

        // A posição desejada começa sendo
        // a posição original da câmera.
        Vector3 targetPosition =
            cameraInitialLocalPosition;

        if (IsGroundedAndMoving())
        {
            // Usa o intervalo dos passos para sincronizar
            // o balanço da câmera com o áudio.
            float safeInterval =
                Mathf.Max(
                    footstepInterval,
                    0.1f
                );

            float phaseSpeed =
                (Mathf.PI * 2f) /
                safeInterval;

            headBobPhase +=
                phaseSpeed * Time.deltaTime;

            // Movimento lateral mais lento,
            // alternando entre esquerda e direita.
            float horizontalOffset =
                Mathf.Cos(
                    headBobPhase * 0.5f
                ) *
                headBobHorizontalAmount;

            // Movimento vertical a cada passo.
            float verticalOffset =
                -Mathf.Cos(
                    headBobPhase
                ) *
                headBobVerticalAmount;

            targetPosition +=
                new Vector3(
                    horizontalOffset,
                    verticalOffset,
                    0f
                );
        }
        else
        {
            headBobPhase = 0f;
        }

        // Suavização independente da taxa de quadros.
        float smoothFactor =
            1f - Mathf.Exp(
                -headBobSmoothing *
                Time.deltaTime
            );

        cameraTarget.localPosition =
            Vector3.Lerp(
                cameraTarget.localPosition,
                targetPosition,
                smoothFactor
            );
    }

    private void ResetHeadBob(bool instantly)
    {
        headBobPhase = 0f;

        if (instantly &&
            cameraTarget != null)
        {
            cameraTarget.localPosition =
                cameraInitialLocalPosition;
        }
    }

    // ==================================================
    // VERIFICAÇÃO DE MOVIMENTO
    // ==================================================

    private bool IsGroundedAndMoving()
    {
        if (controller == null ||
            moveAction == null)
        {
            return false;
        }

        Vector2 movementInput =
            moveAction.ReadValue<Vector2>();

        Vector3 horizontalVelocity =
            controller.velocity;

        horizontalVelocity.y = 0f;

        bool hasMovementInput =
            movementInput.sqrMagnitude >
            0.01f;

        bool isActuallyMoving =
            horizontalVelocity.sqrMagnitude >=
            minimumMovementSpeed *
            minimumMovementSpeed;

        return
            controller.isGrounded &&
            hasMovementInput &&
            isActuallyMoving;
    }

    public void SetMovementEnabled(bool isEnabled)
    {
        movementEnabled = isEnabled;

        if (!isEnabled)
        {
            velocity = Vector3.zero;
            ResetFootsteps();
            ResetHeadBob(true);
        }
    }
}
