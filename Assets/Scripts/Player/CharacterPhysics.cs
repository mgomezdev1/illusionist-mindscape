using UnityEngine;

public class CharacterPhysics : MonoBehaviour
{
    private CharacterController controller;

    private Vector3 gravity = new(0, -9.8f, 0);
    [SerializeField] private Vector3 velocity = new(0, 0, 0);
    [SerializeField] float physicsRadius = 0.5f;
    [SerializeField] LayerMask groundLayer;
    readonly float physicsEpsilon = 0.0075f;
    readonly float terminalVelocity = 50.0f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (GameController.Instance.IsPaused) return;
        ProcessFalling();
    }

    void ProcessFalling()
    {
        if (controller == null) return;

        if (Physics.SphereCast(GetStartSphereCenter(), physicsRadius, Vector3.down, out RaycastHit _, SphereCastDistance(), groundLayer))
        {
            velocity = Vector3.zero;
            return;
        }
        velocity += gravity * Time.deltaTime;
        if (velocity.magnitude > terminalVelocity)
        {
            velocity = velocity.normalized * terminalVelocity;
        }
        CollisionFlags flags = controller.Move(velocity * Time.deltaTime);
        if (flags.HasFlag(CollisionFlags.CollidedBelow))
        {
            velocity = Vector3.zero;
        }
    }

    private Vector3 GetStartSphereCenter()
    {
        Vector3 result = transform.position + controller.center;
        result.y += physicsRadius + physicsEpsilon;
        return result;
    }
    private Vector3 GetEndSphereCenter()
    {
        Vector3 result = transform.position + controller.center;
        // Such that the sphere lies where the character would lie, extending beneath by physicsEpsilon
        result.y += physicsRadius - controller.bounds.extents.y - physicsEpsilon;
        return result;
    }
    private float SphereCastDistance()
    {
        return controller.bounds.extents.y + 2 * physicsEpsilon;
    }

    private void OnDrawGizmos()
    {
        if (controller == null) return;
        Gizmos.DrawWireSphere(GetStartSphereCenter(), physicsRadius);
        Gizmos.DrawWireSphere(GetEndSphereCenter(), physicsRadius);
    }
}