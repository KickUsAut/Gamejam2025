using System.Collections;
using UnityEngine;

public class FixedController : MonoBehaviour
{
    [Header("Refs")]
    public GameObject Planet;
    public GameObject PlayerPlaceholder;

    [Header("Move")]
    public float speed = 4f;
    public float jumpForce = 15f;

    [Header("Gravity")]
    [SerializeField] float gravity = 30f;
    [SerializeField] float fallGravityMultiplier = 1.4f;
    [SerializeField] float lowJumpGravityMultiplier = 1.1f;

    [Header("Animation Parameters")]
    public string ANIM_SPEED = "Speed";               // float   0..1
    public string ANIM_JUMP = "Jump";                 // trigger
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
    [SerializeField] int   minGroundFrames = 2;   // Entprellen
    [SerializeField] float minAirTime = 0.08f;    // min. Luftzeit vor Landen
    [SerializeField] float landHold = 0.18f;      // kurze Land-Pause

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
    float horizontalInput;
    float verticalInput;
    float rotationInput;
    bool jumpInput;
    bool jumpHeld;

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
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        rotationInput = 0f;
        if (Input.GetKey(KeyCode.E)) rotationInput = 1f;
        else if (Input.GetKey(KeyCode.Q)) rotationInput = -1f;

        if (rotationInput != 0f)
            transform.Rotate(0f, rotationInput * 150f * Time.deltaTime, 0f);

        jumpHeld = Input.GetKey(KeyCode.Space);
        if (Input.GetKeyDown(KeyCode.Space) && OnGround)
        {
            jumpInput = true;
            if (animator) animator.SetTrigger(ANIM_JUMP);
        }

        // --- Ground Check: SphereCast + Entprellen ---
        RaycastHit hit;
        Vector3 desiredUp = (Planet != null)
            ? -(Planet.transform.position - transform.position).normalized
            : transform.up;

        wasOnGround = OnGround;

        // Sphere-Radius aus Collider ermitteln
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

        // Entprellen der Boden-Erkennung
        if (rawGrounded) { groundedFrames++; ungroundedFrames = 0; }
        else { ungroundedFrames++; groundedFrames = 0; }

        OnGround = groundedFrames >= minGroundFrames;

        // Zeiten pflegen
        if (OnGround) { timeSinceGrounded = 0f; timeSinceUngrounded += Time.deltaTime; }
        else          { timeSinceUngrounded = 0f; timeSinceGrounded += Time.deltaTime; }

        // Soft-Snap beim leichten Abheben
        if (!OnGround && rawGrounded && hit.distance < groundSnapDistance && rb && !rb.isKinematic)
        {
            float vDotUp = Vector3.Dot(rb.velocity, transform.up);
            if (vDotUp <= 0f)
                rb.AddForce((-transform.up) * gravity * 0.5f, ForceMode.Acceleration);
        }

        // Land-Pause starten nur wenn wirklich kurz in der Luft
        if (OnGround && !wasOnGround && timeSinceUngrounded >= minAirTime)
        {
            inLandHold = true;
            landHoldTimer = landHold;
            // Optional: animator?.SetTrigger("Land");
        }

        // Land-Pause Timer
        if (inLandHold)
        {
            landHoldTimer -= Time.deltaTime;
            if (landHoldTimer <= 0f) inLandHold = false;
        }

        // --- Rotationsausrichtung ---
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, Groundnormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);

        // --- Animation Params ---
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (Planet == null || rb == null || rb.isKinematic) return;

        // Gravframe
        Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;

        // Tangenten
        Vector3 tangentForward = Vector3.ProjectOnPlane(transform.forward, gravDirection).normalized;
        Vector3 tangentRight = Vector3.ProjectOnPlane(transform.right, gravDirection).normalized;

        // Horizontale Velocity aus Input
        Vector3 movementInput = tangentRight * horizontalInput + tangentForward * verticalInput;
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

        // Jump
        if (jumpInput)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpInput = false;
        }

        // Schwerkraft
        float verticalSpeed = Vector3.Dot(rb.velocity, transform.up);
        float effectiveGravity = gravity;
        if (verticalSpeed < 0f) effectiveGravity *= fallGravityMultiplier;
        else if (!jumpHeld)     effectiveGravity *= lowJumpGravityMultiplier;

        rb.AddForce(gravDirection * -effectiveGravity, ForceMode.Acceleration);
    }

    void UpdateAnimations()
    {
        if (!animator) return;

        float inputMagnitude = new Vector2(horizontalInput, verticalInput).magnitude;
        float movementSpeed = Mathf.Clamp01(inputMagnitude);

        // Während Land-Pause nicht in run pushen
        float speedForAnimator = inLandHold ? 0f : movementSpeed;

        animator.SetFloat(ANIM_SPEED, speedForAnimator);
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
