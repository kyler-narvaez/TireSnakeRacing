using UnityEngine;
using Unity.Netcode;

public class PlayerCameraSetup : NetworkBehaviour
{
    public Camera playerCamera;
    public AudioListener audioListener;

    public override void OnNetworkSpawn()
    {
        playerCamera.gameObject.SetActive(IsOwner);
        audioListener.enabled = IsOwner;
    }
}
