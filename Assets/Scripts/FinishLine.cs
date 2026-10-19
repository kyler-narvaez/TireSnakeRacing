using UnityEngine;
using Unity.Netcode;

public class FinishLine : NetworkBehaviour
{
    public Collider wallCollider;
    RaceManager raceManager;

    private void Start()
    {
        raceManager = FindObjectOfType<RaceManager>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        NetworkObject netObj = other.transform.root.GetComponent<NetworkObject>();
        if (netObj == null || !netObj.IsOwner) return; // only the owning client reports their own crossing

        ReportLapServerRpc(netObj.OwnerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    void ReportLapServerRpc(ulong clientId)
    {
        for (int i = 0; i < PlayerDataManager.Instance.players.Count; i++)
        {
            PlayerData data = PlayerDataManager.Instance.players[i];
            if (data.clientId != clientId) continue;

            data.currentLap++;
            print(data.currentLap);
            PlayerDataManager.Instance.players[i] = data;

            // Also update the actual vehicle's route-tracking variable
            var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
            var progress = playerObj.GetComponent<PlayerRouteProcess>();
            progress.currentStage.Value = data.currentLap;
            break;
        }
    }
}
