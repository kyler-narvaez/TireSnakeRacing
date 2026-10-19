using Unity.Netcode;

public class LobbySpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            StartCoroutine(WaitForSpawnManagerThenPosition());
        }
    }

    System.Collections.IEnumerator WaitForSpawnManagerThenPosition()
    {
        SpawnManager spawnManager = null;
        while (spawnManager == null)
        {
            spawnManager = FindObjectOfType<SpawnManager>();
            yield return null; // wait a frame, try again — will succeed once Lobby scene finishes loading
        }

        int index = (int)(OwnerClientId % (ulong)spawnManager.spawnPoints.Length);
        transform.position = spawnManager.spawnPoints[index].position;
        transform.rotation = spawnManager.spawnPoints[index].rotation;
    }
}
