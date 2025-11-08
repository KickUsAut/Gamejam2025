using UnityEngine;

public class FixedController : MonoBehaviour
{

    public GameObject Planet;
    public GameObject PlayerPlaceholder;
 
    public float speed = 4;
    public float jumpForce = 15f;

    [SerializeField] float gravity = 30f;
    [SerializeField] float fallGravityMultiplier = 1.4f;
    [SerializeField] float lowJumpGravityMultiplier = 1.1f;
    bool OnGround = false;
 
    float distanceToGround;
    Vector3 Groundnormal;
    Collider playerCollider;

    const float groundCheckBuffer = 0.1f;
    float groundCheckDistance = 0.5f;

    private Rigidbody rb;
    
    // Input-Variablen
    private float horizontalInput;
    private float verticalInput;
    private float rotationInput;
    private bool jumpInput;
    private bool jumpHeld;
 
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = false;

        playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            groundCheckDistance = playerCollider.bounds.extents.y + groundCheckBuffer;
        }

        Groundnormal = transform.up;
    }
 
    void Update()
    {
        // INPUT SAMMELN (in Update für sofortige Reaktion)
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        
        // Rotation Input
        rotationInput = 0f;
        if (Input.GetKey(KeyCode.E))
            rotationInput = 1f;
        else if (Input.GetKey(KeyCode.Q))
            rotationInput = -1f;
        
        // LOCAL ROTATION (direkt in Update)
        if (rotationInput != 0f)
        {
            transform.Rotate(0, rotationInput * 150f * Time.deltaTime, 0);
        }
        
        // Jump Input (GetKeyDown muss in Update bleiben)
        jumpHeld = Input.GetKey(KeyCode.Space);

        if (Input.GetKeyDown(KeyCode.Space) && OnGround)
            jumpInput = true;
        
        // RAYCASTING für Ground Check
        RaycastHit hit;
        Vector3 desiredUp = (Planet != null)
            ? -(Planet.transform.position - transform.position).normalized
            : transform.up;

        if (Physics.Raycast(transform.position, -transform.up, out hit, groundCheckDistance + groundCheckBuffer, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
        {
            distanceToGround = hit.distance;
            Groundnormal = hit.normal;
            OnGround = distanceToGround <= groundCheckDistance + 0.02f;
        }
        else
        {
            Groundnormal = desiredUp;
            OnGround = false;
        }
        
        // ROTATION zur Ground Normal (smooth in Update)
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, Groundnormal) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * 10f);
    }
    
    void FixedUpdate()
    {

        if (Planet == null) return;
        // MOVEMENT (Physik in FixedUpdate)
        Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;

        Vector3 tangentForward = Vector3.ProjectOnPlane(transform.forward, gravDirection).normalized;
        Vector3 tangentRight = Vector3.ProjectOnPlane(transform.right, gravDirection).normalized;

        Vector3 movementInput = tangentRight * horizontalInput + tangentForward * verticalInput;
        Vector3 desiredHorizontalVelocity = Vector3.ClampMagnitude(movementInput, 1f) * speed;

        Vector3 currentVelocity = rb.velocity;
        Vector3 verticalVelocity = Vector3.Project(currentVelocity, transform.up);
        rb.velocity = desiredHorizontalVelocity + verticalVelocity;
        
        // JUMP
        if (jumpInput)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpInput = false; // Reset jump
        }
        
        // GRAVITY (immer aktiv, egal ob OnGround oder nicht!)
        float verticalSpeed = Vector3.Dot(rb.velocity, transform.up);
        float effectiveGravity = gravity;

        if (verticalSpeed < 0f)
        {
            effectiveGravity *= fallGravityMultiplier;
        }
        else if (!jumpHeld)
        {
            effectiveGravity *= lowJumpGravityMultiplier;
        }

        rb.AddForce(gravDirection * -effectiveGravity, ForceMode.Acceleration);
    }
 
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.isTrigger)
            return;

        if (collision.transform != Planet.transform)
        {
            Planet = collision.transform.gameObject;
 
            Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;
 
            Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravDirection) * transform.rotation;
            transform.rotation = toRotation;
 
            rb.velocity = Vector3.zero;
            rb.AddForce(gravDirection * gravity, ForceMode.Acceleration);

            PlayerPlaceholder.GetComponent<FixedPlaceholder>().NewPlanet(Planet);
        }
    }

    public void OnTeleported(GameObject newPlanet)
    {
        Planet = newPlanet;

        // Up-Ausrichtung sofort korrigieren
        Vector3 gravDirection = (transform.position - Planet.transform.position).normalized;
        Quaternion toRotation = Quaternion.FromToRotation(transform.up, gravDirection) * transform.rotation;
        transform.rotation = toRotation;

        // Geschwindigkeit nullen, damit alte Grav/Beschleunigung nicht „nachzieht“
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // Placeholder informieren
        var ph = PlayerPlaceholder != null ? PlayerPlaceholder.GetComponent<FixedPlaceholder>() : null;
        if (ph != null) ph.NewPlanet(Planet);
    }

}