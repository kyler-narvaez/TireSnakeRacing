using UnityEngine;
using Unity.Netcode;

public class LobbyCameraFollow : MonoBehaviour
{
    public Transform originalOffset; // reference transform for default lobby-wide framing
    public Vector3 customizeOffset = new Vector3(0f, 0.3f, -4f); // lower height, further back
    public float smoothTime = 0.5f;

    Transform selfTarget;
    bool isCustomizing = false;
    Vector3 velocity = Vector3.zero;

    void Start()
    {
        StartCoroutine(FindSelfPlayer());
    }

    System.Collections.IEnumerator FindSelfPlayer()
    {
        NetworkObject localPlayer = null;
        while (localPlayer == null)
        {
            localPlayer = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            yield return null;
        }
        selfTarget = localPlayer.transform;
    }

    public void SetCustomizing(bool customizing)
    {
        print($"SetCustomizing called: {customizing}");
        isCustomizing = customizing;
    }

    void LateUpdate()
    {
        if (selfTarget == null) return;

        Vector3 desiredPosition = isCustomizing
            ? selfTarget.position + customizeOffset
            : originalOffset.position;

        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

        if (isCustomizing)
        {
            Vector3 lookTarget = new Vector3(selfTarget.position.x, transform.position.y, selfTarget.position.z); // same height, so LookAt doesn't pitch downward
            transform.LookAt(lookTarget);
        }
        else
        {
            transform.rotation = originalOffset.rotation;
        }
    }
}
