using UnityEngine;

public class FLO_Movement : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
	float maxAcceleration = 10f, maxAirAcceleration = 1f;

    [SerializeField, Range(0f, 10f)]
	float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
	int maxAirJumps = 0;

    [SerializeField, Range(0f, 90f)]
	float maxGroundAngle = 25f;

    float minGroundDotProduct;

    int jumpPhase;

    bool desiredJump;

    //bool onGround;
	int groundContactCount;

	bool OnGround => groundContactCount > 0;

    Vector3 velocity, desiredVelocity;

    Vector3 contactNormal;

    Vector3 ProjectOnContactPlane (Vector3 vector) {
		return vector - contactNormal * Vector3.Dot(vector, contactNormal);
	}

    Rigidbody body;

    void Awake()
    {
        body = GetComponent<Rigidbody>();
        OnValidate();
    }

    void Update () {
		Vector2 playerInput;
		playerInput.x = Input.GetAxis("Horizontal");
		playerInput.y = Input.GetAxis("Vertical");
		playerInput = Vector2.ClampMagnitude(playerInput, 1f);

		desiredVelocity =
			new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        desiredJump |= Input.GetButtonDown("Jump");
	}

	void FixedUpdate () {
		UpdateState();
		AdjustVelocity();
		//float acceleration = onGround ? maxAcceleration : maxAirAcceleration;
		//float maxSpeedChange = acceleration * Time.deltaTime;

		//velocity.x =
		//	Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
		//velocity.z =
		//	Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

		if (desiredJump) {
			desiredJump = false;
			Jump();
		}
		body.velocity = velocity;
		ClearState();
	}

    void UpdateState () {
		velocity = body.velocity;
		if (OnGround) {
			jumpPhase = 0;
			if (groundContactCount > 1) {
				contactNormal.Normalize();
			}
		}
		else {
			contactNormal = Vector3.up;
		}
	}

    void Jump () {
		if (OnGround || jumpPhase < maxAirJumps) {
			jumpPhase += 1;
			float jumpSpeed = Mathf.Sqrt(-2f * Physics.gravity.y * jumpHeight);
			float alignedSpeed = Vector3.Dot(velocity, contactNormal);
			if (alignedSpeed > 0f) {
				jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			}
			velocity += contactNormal * jumpSpeed;
		}
	}

    void OnCollisionEnter (Collision collision) {
		//onGround = true;
		EvaluateCollision(collision);
	}

	void OnCollisionStay (Collision collision) {
		//onGround = true;
		EvaluateCollision(collision);
	}
	
	void EvaluateCollision (Collision collision) {
		for (int i = 0; i < collision.contactCount; i++) {
			Vector3 normal = collision.GetContact(i).normal;
			if (normal.y >= minGroundDotProduct) {
				//onGround = true;
				groundContactCount += 1;
				contactNormal += normal;
			}
		}
	}

    void OnValidate () {
		minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
	}

    void AdjustVelocity () {
		Vector3 xAxis = ProjectOnContactPlane(Vector3.right).normalized;
		Vector3 zAxis = ProjectOnContactPlane(Vector3.forward).normalized;

        float currentX = Vector3.Dot(velocity, xAxis);
		float currentZ = Vector3.Dot(velocity, zAxis);

        float acceleration = OnGround ? maxAcceleration : maxAirAcceleration;
		float maxSpeedChange = acceleration * Time.deltaTime;

		float newX =
			Mathf.MoveTowards(currentX, desiredVelocity.x, maxSpeedChange);
		float newZ =
			Mathf.MoveTowards(currentZ, desiredVelocity.z, maxSpeedChange);

        velocity += xAxis * (newX - currentX) + zAxis * (newZ - currentZ);
	}

    void ClearState () {
		//onGround = false;
		groundContactCount = 0;
		contactNormal = Vector3.zero;
	}
}
