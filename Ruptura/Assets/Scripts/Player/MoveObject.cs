
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MoveObject : MonoBehaviour, IRaycastInteractable
{
    [Header("Movimento")]
    [SerializeField] private float holdDistance = 2f;
    [SerializeField] private float followSpeed = 12f;

    [Header("Colisão")]
    [SerializeField] private float collisionRadius = 0.35f;
    [SerializeField] private float wallOffset = 0.1f;
    [SerializeField] private LayerMask collisionLayers = ~0;

    private Rigidbody rb;
    private Camera playerCamera;

    private bool isBeingHeld;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        playerCamera = Camera.main;
    }

    private void FixedUpdate()
    {
        if (!isBeingHeld || playerCamera == null)
            return;

        Vector3 cameraPosition = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.forward;

        float allowedDistance = holdDistance;

        // Verifica se existe parede/objeto na frente.
        if (Physics.SphereCast(
            cameraPosition,
            collisionRadius,
            direction,
            out RaycastHit hit,
            holdDistance,
            collisionLayers,
            QueryTriggerInteraction.Ignore))
        {
            allowedDistance = hit.distance - wallOffset;

            allowedDistance = Mathf.Max(
                allowedDistance,
                collisionRadius
            );
        }

        Vector3 targetPosition =
            cameraPosition +
            direction * allowedDistance;

        Vector3 newPosition = Vector3.Lerp(
            rb.position,
            targetPosition,
            followSpeed * Time.fixedDeltaTime
        );

        rb.MovePosition(newPosition);
    }

    public void Interact()
    {
        if (isBeingHeld)
        {
            Drop();
        }
        else
        {
            PickUp();
        }
    }

    public void PickUp()
    {
        isBeingHeld = true;

        rb.useGravity = false;

        // Importante:
        // NÃO usar isKinematic aqui.
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;
    }

    public void Drop()
    {
        isBeingHeld = false;

        rb.useGravity = true;
        rb.isKinematic = false;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.collisionDetectionMode =
            CollisionDetectionMode.Continuous;
    }
}