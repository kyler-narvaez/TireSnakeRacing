using Unity.Netcode;

public class HostOnly : NetworkBehaviour
{
    void Start()
    {
        gameObject.SetActive(NetworkManager.Singleton.IsHost);
        // ... rest of existing Start() logic
    }
}
