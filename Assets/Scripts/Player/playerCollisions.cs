using UnityEngine;
using Unity.Netcode;

public class playerCollisions : NetworkBehaviour
{
    public float knockbackMultiplier = 150f;
    public float minImpactSpeed = 2f;
    public float upwardLaunchMultiplier = 5f;

    public float superKnockMultiplier = 2f;
    public float superKnockMinSpeed = 15f;

    public float barrierKnockMultiplier = 2f;
    public float barrierKnockMinSpeed = 15f;

    PlayerMovement pm;
    CharacterAnimations ca;
    

    private void Start()
    {
        pm = GetComponent<PlayerMovement>();
        ca = GetComponentInChildren<CharacterAnimations>();
    }

    private float CalculateImpactSpeed(Collision collision)
    {
        Vector3 normal = collision.contacts[0].normal;
        float impactSpeed = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, normal));
        print("Impact Speed: " + impactSpeed);
        return impactSpeed;
    }

    private Vector3 CalculateImpactDirection(Collision collision)
    {
        Vector3 impactDirection = (collision.transform.position - transform.position).normalized;
        impactDirection.y = upwardLaunchMultiplier; // small upward pop for drama
        return impactDirection;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!IsOwner) return; // only the owner's copy has real physics/velocity to measure

        if (collision.gameObject.CompareTag("Player"))
        {
            float impactSpeed = CalculateImpactSpeed(collision);

            if (impactSpeed < minImpactSpeed) return; // ignore gentle bumps

            NetworkObject otherNetObj = collision.transform.root.GetComponent<NetworkObject>();
            if (otherNetObj == null) return;

            bool isSuper = impactSpeed >= superKnockMinSpeed;
            if (isSuper) impactSpeed *= superKnockMultiplier;

            Vector3 knockbackDir = CalculateImpactDirection(collision);

            ca.SetBumped(true); // Bump animation

            RequestKnockbackServerRpc(otherNetObj.NetworkObjectId, knockbackDir.normalized, impactSpeed * knockbackMultiplier, isSuper);
        }
        else if (collision.gameObject.layer == 6) //Barrier layer
        {
            float impactSpeed = CalculateImpactSpeed(collision);
            Vector3 impactDir = collision.contacts[0].normal;
            bool isTumble = impactSpeed >= barrierKnockMinSpeed;
            pm.ApplyKnockBack(impactDir, impactSpeed * barrierKnockMultiplier, isTumble);
            if (impactSpeed >= minImpactSpeed) ca.SetBumped(true); // Bump animation
        }
    }

    [ServerRpc]
    void RequestKnockbackServerRpc(ulong targetNetworkObjectId, Vector3 direction, float force, bool isSuper)
    {
        if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out NetworkObject targetObj))
        {
            playerCollisions targetCollisions = targetObj.GetComponentInChildren<playerCollisions>();
            if (targetCollisions != null)
            {
                targetCollisions.ApplyKnockbackClientRpc(direction, force, isSuper, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { targetObj.OwnerClientId } }
                });
            }
        }
    }

    [ClientRpc]
    public void ApplyKnockbackClientRpc(Vector3 direction, float force, bool isSuper, ClientRpcParams rpcParams = default)
    {
        pm.ApplyKnockBack(direction, force, isSuper);
    }
}
