using Unity.Netcode;
using UnityEngine;

public class PlayerSpawnFix : NetworkBehaviour
{
    public Quaternion desiredSpawnRotation = Quaternion.Euler(0f, 0f, 90f); // your wheel's correct rotation

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            transform.rotation = desiredSpawnRotation;
        }
    }
}