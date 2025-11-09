using System.Collections;
using UnityEngine;

public class FixedController : MonoBehaviour
{
    [Header("Refs")]
    public GameObject Planet;
    public GameObject PlayerPlaceholder;

    [Header("Move")]
    public float speed = 4f;

    [Header("Gravity")]
    [SerializeField] float gravity = 30f;                 // m/s^2
    [SerializeField] float fallGravityMultiplier = 1.4f;

    [Header("Animation Parameters")]
    public string ANIM_SPEED = "Speed";               // float   0..1
    public string ANIM_GROUNDED = "Grounded";         // bool
    public string ANIM_VERTICAL_VELOCITY = "VerticalVelocity"; // float

    // --- Grounding & Stabilisierung ---
    const float groundCheckBuffer = 0.1f;
    float groundCheckDistance = 0.5f;
    float distanceToGround;
    Vector3 Groundnormal;

    [Header("Ground Tuning")]
    [SerializeField] float maxSlope = 55f;
    [SerializeField] float groundSnapDistance = 0.15f;
    [SerializeField] int   minGroundFrames = 3;
    [SerializeField] float minAirTime = 0.12f;
    [SerializeField] float landHold = 0.12f;

    int groundedFrames = 0, ungroundedFrames = 0;
    float timeSinceGrounded = 0f, timeSinceUngrounded = 999f;
    bool inLandHold = false;
    float landHoldTimer = 0f;

    // State
    bool OnGround = false;
    bool wasOnGround = false;

    // Components
    Rigidbody rb;
    Collider playerCollider;
    Animator animator;

    // Input
    float verticalInput;
    float rotationInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;
        rb.isKinematic = false;

        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
            groundCheckDistance = playerCollider.bounds.extents.y + groundCheckBuffer;

        Groundnormal = transform.up;
        wasOnGround = OnGround;
    }

    void Update()
    {
        // --- Input ---
        verticalInput = Input.GetKey(KeyCode.W) ? 1f : 0f;

        rotationInput = 0f;
        if (Input.GetKey(KeyCode.D)) rotationInput = 1f;
        else if (Input.GetKey(KeyCode.A)) rotationInput = -1f;

        if (rotationInput != 0f)
            transform.Rotate(0f, rotationInput * 150f * Time.deltaTime, 0f);

        // --- Ground Check: SphereCast + Entprellen ---
        RaycastHit hit;
        Vector3 desiredUp = (Planet != null)
            ? -(Planet.transform.position - transform.position).normalized
            : transform.up;

        wasOnGround = OnGround;

        // Sphere-Radius aus Collider
        float radius = 0.2f;
        if (playerCollider is CapsuleCollider cap) radius = cap.radius * 0.95f;
        else if (playerCollider is CharacterController cc) radius = cc.radius * 0.95f;
        else if (playerCollider != null)
            radius = Mathf.Min(playerCollider.bounds.extents.x, playerCollider.bounds.extents.z) * 0.6f;

        bool rawGrounded = false;
        if (Physics.SphereCast(
                transform.position,
                radius,
                -transform.up,
                out hit,
                groundCheckDistance + groundCheckBuffer,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            float angle = Vector3.Angle(hit.normal, transform.up);
            rawGrounded = angle <= maxSlope && hit.distance <= groundCheckDistance + groundCheckBuffer;
            distanceToGround = hit.distance;
            Groundnormal = hit.normal;
        }
        else
        {
            Groundnormal = desiredUp;
            rawGrounded = false;
        }

        // Entprellen
        if (rawGrounded) { groundedFrames++; ungroundedFrames = 0; }
        else { ungroundedFrames++; groundedFrames = 0; }

        OnGround = groundedFrames >= minGroundFrames;

        // Zeiten
        if (OnGround) { timeSinceGrounded = 0f; timeSinceUngrounded += Time.deltaTime; }
        else          { timeSinceUngrounded = 0f; timeSinceGrounded += Time.deltaTime; }

        // Soft-Snap beim leichten Abheben, damit die Figur am Boden bleibt
        if (!OnGround && rawGrounded && hit.distance < groundSnapDistance && rb && !rb.isKinematic)
        {
            float vDotUp = Vector3.Dot(rb.velocity, transform.up);
            if (vDotUp <= 0f)
                rb.AddForce((-transform.up) * gravity * 0.5f, ForceMode.Acceleration);
        }

        // Land-Pause nur wenn wirklich kurz in der Luft
        if (OnGround && !wasOnGround && timeSinceUngrounded >= minAirTime)
        {
            inLandHold = true;
            landHoldTimer = landHold;
        }
        if (inLandHold)
        {
            landHoldTimer -= Time.deltaTime;
            if (landHoldTimer <= 0f) inLandHold = false;
        }

        // Rotationsausrichtung
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, Groundnormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);

        // Anim params
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (Planet == null || rb == null || rb.isKinematic) return;

        // Gravframe
        Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;

        // Tangenten
        Vector3 tangentForward = Vector3.ProjectOnPlane(transform.forward, gravDirection).normalized;

        // Horizontal velocity aus Input
        Vector3 movementInput = tangentForward * verticalInput;
        Vector3 desiredHorizontalVelocity = Vector3.ClampMagnitude(movementInput, 1f) * speed;

        Vector3 currentVelocity = rb.velocity;
        Vector3 verticalVelocity = Vector3.Project(currentVelocity, transform.up);

        // Ersetze horizontal
        rb.velocity = desiredHorizontalVelocity + verticalVelocity;

        // Dämpfe ohne Input
        if (movementInput.sqrMagnitude < 0.0001f)
        {
            Vector3 horizontalVel = rb.velocity - verticalVelocity;
            rb.velocity = verticalVelocity + horizontalVel * 0.5f;
        }

        // Schwerkraft
        float verticalSpeedNow = Vector3.Dot(rb.velocity, transform.up);
        float effectiveGravity = gravity;
        if (verticalSpeedNow < 0f) effectiveGravity *= fallGravityMultiplier;

        rb.AddForce(gravDirection * -effectiveGravity, ForceMode.Acceleration);
    }

    void UpdateAnimations()
    {
        if (!animator) return;

        float inputMagnitude = Mathf.Abs(verticalInput);
        float movementSpeed = Mathf.Clamp01(inputMagnitude);

        // weiches Setzen + Land-Pause
        float speedForAnimator = inLandHold ? 0f : movementSpeed;
        animator.SetFloat(ANIM_SPEED, speedForAnimator, 0.15f, Time.deltaTime);

        animator.SetBool(ANIM_GROUNDED, OnGround);

        float verticalVelocity = rb ? Vector3.Dot(rb.velocity, transform.up) : 0f;
        animator.SetFloat(ANIM_VERTICAL_VELOCITY, verticalVelocity);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.isTrigger) return;

        if (collision.transform != Planet?.transform)
        {
            Planet = collision.transform.gameObject;

            Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;
            Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravDirection) * transform.rotation;
            transform.rotation = toRotation;

            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.AddForce(gravDirection * gravity, ForceMode.Acceleration);
            }

            var ph = PlayerPlaceholder ? PlayerPlaceholder.GetComponent<FixedPlaceholder>() : null;
            if (ph) ph.NewPlanet(Planet);
        }
    }

    public void OnTeleported(GameObject newPlanet)
    {
        Planet = newPlanet;

        Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravDirection) * transform.rotation;
        transform.rotation = toRotation;

        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        if (rb.isKinematic) StartCoroutine(ResetVelocityNextFixed());
        else
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        var ph = PlayerPlaceholder ? PlayerPlaceholder.GetComponent<FixedPlaceholder>() : null;
        if (ph) ph.NewPlanet(Planet);
    }

    IEnumerator ResetVelocityNextFixed()
    {
        yield return new WaitForFixedUpdate();
        if (rb == null) yield break;
        rb.isKinematic = false;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
