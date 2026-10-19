using Unity.Netcode;

public class PlayerDataManager : NetworkBehaviour
{
    public static PlayerDataManager Instance;
    public NetworkList<PlayerData> players;

    void Awake()
    {
        players = new NetworkList<PlayerData>();
        if (Instance == null) Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += AddNewPlayerData;
        }
    }

    void AddNewPlayerData(ulong clientId)
    {
        // Don't add a duplicate if this client already has an entry
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == clientId) return;
        }

        PlayerData newData = new PlayerData
        {
            clientId = clientId,
            playerName = "Player",
            characterColor = 0,
            characterIndex = 0,
            tireIndex = 0,
            tireColor = 0,
            currentLap = -1
        };
        players.Add(newData);
    }
}
