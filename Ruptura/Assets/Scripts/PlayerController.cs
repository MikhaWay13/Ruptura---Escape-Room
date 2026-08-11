using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movimento")]
    public float moveSpeed = 5f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    [Header("Câmera")]
    public Transform cameraTarget;  
    public float lookSensitivity = 0.1f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Interação")]
    [SerializeField, Min(0.1f)]
    private float interactionDistance = 3f;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction jumpAction;
    private InputAction interactAction;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch;
    private bool gameplayControlEnabled = true;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        moveAction = InputSystem.actions.FindAction("Move");
        lookAction = InputSystem.actions.FindAction("Look");
        jumpAction = InputSystem.actions.FindAction("Jump");
        interactAction = InputSystem.actions.FindAction("Interact");

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void OnEnable()
    {
         InputSystem.actions.FindActionMap("Player").Enable();
    }
    private void OnDisable()
    {
         InputSystem.actions.FindActionMap("Player").Disable();
    }
    private void Update()
    {
        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            TryInteract();
        }

        if (!gameplayControlEnabled)
        {
            return;
        }

        Look();
        Move();
    }

    public void SetGameplayControlEnabled(bool isEnabled)
    {
        gameplayControlEnabled = isEnabled;

        if (!isEnabled)
        {
            velocity = Vector3.zero;
        }
    }

    private void TryInteract()
    {
        Camera interactionCamera = Camera.main;

        if (interactionCamera == null)
        {
            Debug.LogWarning("Não foi encontrada uma câmera com a tag MainCamera.", this);
            return;
        }

        float distance = interactionDistance > 0f ? interactionDistance : 3f;
        Ray ray = new Ray(
            interactionCamera.transform.position,
            interactionCamera.transform.forward
        );

        if (!Physics.Raycast(
                ray,
                out RaycastHit hit,
                distance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            return;
        }

        MonoBehaviour[] behaviours =
            hit.collider.GetComponentsInParent<MonoBehaviour>(true);

        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IRaycastInteractable interactable)
            {
                interactable.Interact(this);
                return;
            }
        }
    }

    private void Look()
    {
        Vector2 look = lookAction.ReadValue<Vector2>() * lookSensitivity;

        transform.Rotate(Vector3.up * look.x);

        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraTarget.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

   private void Move()
{
    bool grounded = controller.isGrounded;

    if (grounded && velocity.y < 0f)
        velocity.y = -2f;

    if (jumpAction.WasPressedThisFrame() && grounded)
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

    velocity.y += gravity * Time.deltaTime;

    Vector2 input = moveAction.ReadValue<Vector2>();
    Vector3 move = transform.right * input.x + transform.forward * input.y;

    controller.Move((move * moveSpeed + velocity) * Time.deltaTime);
}
}
