using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CharakterShadow : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 5f;

    [Header("Planet Gravity")]
    public Transform planet;
    public float gravityStrength = 9.81f;
    public float groundStickForce = 2f;
    public float alignSpeed = 10f;
    public bool autoAssignPlanet = true;
    [Tooltip("Tag used when autoAssignPlanet is enabled.")]
    public string planetTag = "Planet";
    [SerializeField] float groundCheckDistance = 0.4f;
    [SerializeField] LayerMask groundMask = ~0;

    private CharacterController controller;
    private Vector3 gravityVelocity;
    private float nextPlanetSearchTime;

    const float PlanetSearchInterval = 1f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        TryAssignPlanet();
    }

    void Update()
    {
        TryAssignPlanet();

        Vector3 gravityDirection = GetGravityDirection();
        Vector3 moveInput = GetMoveInput(gravityDirection);

        controller.Move(moveInput * speed * Time.deltaTime);

        ApplyGravity(gravityDirection);
        AlignToPlanet(gravityDirection);
    }

    Vector3 GetMoveInput(Vector3 gravityDirection)
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 move = transform.right * horizontal + transform.forward * vertical;
        if (planet != null)
        {
            move = Vector3.ProjectOnPlane(move, gravityDirection).normalized * move.magnitude;
        }

        return move;
    }

    void ApplyGravity(Vector3 gravityDirection)
    {
        bool grounded = IsGrounded(gravityDirection);

        if (grounded && Vector3.Dot(gravityVelocity, gravityDirection) > 0f)
        {
            gravityVelocity = gravityDirection * groundStickForce;
        }

        gravityVelocity += gravityDirection * gravityStrength * Time.deltaTime;
        gravityVelocity = Vector3.Project(gravityVelocity, gravityDirection);

        controller.Move(gravityVelocity * Time.deltaTime);
    }

    bool IsGrounded(Vector3 gravityDirection)
    {
        if (controller != null && controller.isGrounded)
        {
            return true;
        }

        if (controller == null)
        {
            return false;
        }

        Vector3 origin = transform.position + controller.center;
        float radius = controller.radius * 0.95f;
        float maxDistance = groundCheckDistance + radius;

        return Physics.SphereCast(origin, radius, gravityDirection, out _, maxDistance, groundMask, QueryTriggerInteraction.Ignore);
    }

    void AlignToPlanet(Vector3 gravityDirection)
    {
        if (planet == null)
        {
            return;
        }

        Vector3 desiredUp = -gravityDirection;
        if (desiredUp.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion toRotation = Quaternion.FromToRotation(transform.up, desiredUp) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, alignSpeed * Time.deltaTime);
    }

    Vector3 GetGravityDirection()
    {
        if (planet != null)
        {
            Vector3 direction = planet.position - transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                return direction.normalized;
            }
        }

        return Vector3.down;
    }

    void TryAssignPlanet()
    {
        if (!autoAssignPlanet || planet != null || Time.time < nextPlanetSearchTime)
        {
            return;
        }

        nextPlanetSearchTime = Time.time + PlanetSearchInterval;

        Transform closest = FindClosestPlanet();
        if (closest != null)
        {
            planet = closest;
        }
    }

    Transform FindClosestPlanet()
    {
        GameObject[] planets = GameObject.FindGameObjectsWithTag(planetTag);
        if (planets == null || planets.Length == 0)
        {
            return null;
        }

        float minDistance = float.MaxValue;
        Transform closest = null;
        Vector3 currentPos = transform.position;

        foreach (GameObject candidate in planets)
        {
            float sqrDistance = (candidate.transform.position - currentPos).sqrMagnitude;
            if (sqrDistance < minDistance)
            {
                minDistance = sqrDistance;
                closest = candidate.transform;
            }
        }

        return closest;
    }

    public void SetPlanet(Transform newPlanet)
    {
        planet = newPlanet;
    }
}
