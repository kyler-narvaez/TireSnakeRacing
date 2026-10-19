using Unity.Netcode;
using UnityEngine;

public class GameSettings : NetworkBehaviour
{
    public static GameSettings Instance;
    public NetworkVariable<int> numberOfLaps = new NetworkVariable<int>(3);
    public NetworkVariable<bool> isRandom = new NetworkVariable<bool>(false);
    public NetworkVariable<bool> isSameSequence = new NetworkVariable<bool>(true);
    public GameObject player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    //To change spawn prefab
    [ClientRpc]
    public void SwapPlayerPrefabClientRpc()
    {
        NetworkManager.Singleton.NetworkConfig.PlayerPrefab = player;
    }
}