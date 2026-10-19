using UnityEngine;
using Unity.Netcode;

public class FinishLineOneWay : NetworkBehaviour
{
    Collider wallCollider; // the solid, non-trigger collider blocking backward movement
    public Collider playerCollider;
    public Collider wheelCollider;
    public float error = 0.1f;

    Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        FinishLine fl = FindObjectOfType<FinishLine>();
        if (fl != null)
        {
            wallCollider = fl.wallCollider;
        }
    }

    void FixedUpdate()
    {
        if (!IsOwner || wallCollider == null) return;

        float dot = Vector3.Dot(rb.velocity, -wallCollider.transform.right);
        bool movingCorrectDirection = dot > error;

        Physics.IgnoreCollision(playerCollider, wallCollider, movingCorrectDirection);
        Physics.IgnoreCollision(wheelCollider, wallCollider, movingCorrectDirection);
    }
}
