using UnityEngine;
using Unity.Netcode;
using System;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : NetworkBehaviour
{
    public WheelCollider wheelCollider;
    public Transform wheelVisual;
    public float motorTorque = 150f;
    public float steerAngle = 60f;
    public float brakeTorque = 300f;
    public float turnSpeed = 90f; // degrees per second

    public float boostForce = 5000f;
    public float sprintCooldown = 4f;
    float sprintCooldownRemaining = 0f;

    public float driftStiffness = 0.5f; // lower than your normal Sideways Friction Stiffness
    WheelFrictionCurve normalSidewaysFriction;

    public float jumpForce = 10f;
    public float angleOfInfluence = 0.5f;
    bool jumpClicked = false;

    public float fallenAngleThreshold = 60f;   // tip angle beyond this = considered fallen
    public float uprightAngleThreshold = 5f;   // back under this = considered recovered

    bool isFallen = false;
    float fallenTimer = 0f;

    public float stability = 0.3f;
    public float selfCorrectionSpeed = 2.0f;

    public bool isTumbling = false;
    public float tumbleDuration = 1f;
    public float tumbleForceMultiplier = 500f;
    float tumbleTimer = 0f;

    Rigidbody rb;
    CharacterAnimations ca;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ca = GetComponentInChildren<CharacterAnimations>();
    }

    public override void OnNetworkSpawn()
    {
        contrainPlayer();
        transform.position = new Vector3(3f, 1f, -25f);
    }

    void contrainPlayer()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //FIXME = will unfreeze when hit
    }

    // Start is called before the first frame update
    void Start()
    {
        if (IsOwner)
        {
            wheelVisual.GetComponent<Renderer>().material.color = Color.red; //FIXME - Change to be set color
            normalSidewaysFriction = wheelCollider.sidewaysFriction;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && wheelCollider.isGrounded) jumpClicked = true;
    }

    void FixedUpdate()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        ca.GoingBackwards(vertical < -0.5f);

        if (IsOwner)
        {
            if (isTumbling)
            {
                tumbleTimer -= Time.fixedDeltaTime;
                if (tumbleTimer <= 0f) isTumbling = false;
            }

            if (isFallen)
            {
                fallenTimer -= Time.fixedDeltaTime;
                bool isBackUpright = Vector3.Angle(transform.up, Vector3.up) < uprightAngleThreshold;
                if (fallenTimer <= 0f && isBackUpright)
                {
                    isFallen = false;
                }
            }

            CheckFallen();
            if (!isFallen && !isTumbling)
            {
                boost();
                drift();
                jump();
                fastBreak(horizontal, vertical);
                contrainPlayer();
            }

            selfCorrection();
        }
        updateVisual();
        ca.SetSpeed(rb.velocity.magnitude);
        ca.SetGrounded(wheelCollider.isGrounded);
        ca.SetTrauma(isTumbling);
        ca.SetBumped(false); // Bump animation
    }

    void boost()
    {
        if (sprintCooldownRemaining > 0f) sprintCooldownRemaining -= Time.fixedDeltaTime;

        if (Input.GetKey(KeyCode.LeftShift) && sprintCooldownRemaining <= 0f)
        {
            rb.AddForce(transform.forward * boostForce, ForceMode.Impulse);
            sprintCooldownRemaining = sprintCooldown;
        }
    }

    void drift()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            WheelFrictionCurve drift = normalSidewaysFriction;
            drift.stiffness = driftStiffness;
            wheelCollider.sidewaysFriction = drift;
        }
        else
        {
            wheelCollider.sidewaysFriction = normalSidewaysFriction;
        }
    }

    void jump()
    {
        if (jumpClicked && wheelCollider.GetGroundHit(out WheelHit hit))
        {
            ca.TriggerJump();
            Vector3 jumpDir = Vector3.Lerp(Vector3.up, hit.normal, angleOfInfluence);
            rb.AddForce(jumpDir * jumpForce, ForceMode.Impulse);
            jumpClicked = false;
        }
    }

    void fastBreak(float horizontal, float vertical)
    {
        float forSpeed = Vector3.Dot(rb.velocity, transform.forward);
        bool isReversingInput = (vertical < 0 && forSpeed > 0.1f) || (vertical > 0 && forSpeed < -0.1f);

        if (isReversingInput)
        {
            wheelCollider.brakeTorque = brakeTorque; // strong braking to kill momentum fast
            wheelCollider.motorTorque = 0f;
        }
        else
        {
            wheelCollider.brakeTorque = 0f;
            wheelCollider.motorTorque = vertical * motorTorque;
        }


        // Rotate the whole player's facing directly — works even with no forward input
        float turn = horizontal * turnSpeed * Time.fixedDeltaTime;
        rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turn, 0f));
    }

    void CheckFallen()
    {
        float tipAngle = Vector3.Angle(transform.up, Vector3.up);
        if (tipAngle > fallenAngleThreshold && !isFallen)
        {
            isFallen = true;
            fallenTimer = 0f;
        }
    }

    void selfCorrection()
    {
        if (isTumbling) return;

        // 1. Balance the wheel on the Z-axis
        Vector3 localUp = transform.up;
        Vector3 globalUp = Vector3.up;

        // Cross product calculates the corrective torque vector
        Vector3 torqueVector = Vector3.Cross(localUp, globalUp);

        // Apply corrective force with dampening to prevent continuous oscillation
        Vector3 stabilization = (torqueVector * selfCorrectionSpeed) - (rb.angularVelocity * stability);
        rb.AddTorque(stabilization, ForceMode.Acceleration);
    }

    public void ApplyKnockBack(Vector3 direction, float force, bool isTumble)
    {
        rb.AddForce(direction * force, ForceMode.Impulse);

        float sideAmount = Vector3.Dot(direction, transform.right);
        rb.AddTorque(Vector3.up * sideAmount * force, ForceMode.Impulse);

        if (isTumble)
        {
            isTumbling = true;
            tumbleTimer = tumbleDuration;
            rb.constraints = RigidbodyConstraints.None;
            Vector3 tumbleAxis = Vector3.Cross(direction, Vector3.up).normalized;
            rb.AddTorque(tumbleAxis * tumbleForceMultiplier, ForceMode.Impulse);
        }
    }

    void updateVisual()
    {
        // rolling visual — signed, so backward spins the opposite way
        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
        float spin = (forwardSpeed / wheelCollider.radius) * Mathf.Rad2Deg * Time.fixedDeltaTime;
        wheelVisual.Rotate(Vector3.forward * spin, Space.Self);
    }
}
