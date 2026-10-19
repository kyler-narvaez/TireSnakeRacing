using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Rigidbody targetRb;       // assign the Player's Rigidbody — used to read turning speed
    public Vector3 offset = new Vector3(0f, 4f, -6f);
    public float positionSmoothTime = 0.3f;
    public float rotationSmoothTime = 0.3f;

    public float lateralSwingAmount = 3f;   // how far left/right the camera shifts at max turn
    public float turnSwingSmoothTime = 0.4f;
    public float maxTurnRateForSwing = 90f; // degrees/sec of yaw that maps to full swing

    Vector3 velocity = Vector3.zero;
    Vector3 rotVelocity = Vector3.zero;
    float currentLateralOffset = 0f;
    float lateralVelocity = 0f;

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 flatForward = target.forward;
        flatForward.y = 0f;
        if (flatForward.sqrMagnitude < 0.001f) flatForward = transform.forward;
        Quaternion yawOnlyRotation = Quaternion.LookRotation(flatForward.normalized, Vector3.up);

        // Turn rate drives lateral swing — negative yaw (turning left) swings camera one way, positive the other
        float turnRate = targetRb != null ? targetRb.angularVelocity.y * Mathf.Rad2Deg : 0f;
        float targetLateralOffset = Mathf.Clamp(turnRate / maxTurnRateForSwing, -1f, 1f) * lateralSwingAmount;
        currentLateralOffset = Mathf.SmoothDamp(currentLateralOffset, targetLateralOffset, ref lateralVelocity, turnSwingSmoothTime);

        Vector3 swungOffset = offset + new Vector3(currentLateralOffset, 0f, 0f);
        Vector3 desiredPosition = target.position + yawOnlyRotation * swungOffset;
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, positionSmoothTime);

        Vector3 desiredLookDir = target.position - transform.position + Vector3.up * 1.5f;
        Quaternion desiredRotation = Quaternion.LookRotation(desiredLookDir, Vector3.up);

        Vector3 currentEuler = transform.rotation.eulerAngles;
        Vector3 desiredEuler = desiredRotation.eulerAngles;
        Vector3 smoothedEuler = new Vector3(
            0f,
            Mathf.SmoothDampAngle(currentEuler.y, desiredEuler.y, ref rotVelocity.y, rotationSmoothTime),
            0f
        );
        transform.rotation = Quaternion.Euler(smoothedEuler);
    }
}