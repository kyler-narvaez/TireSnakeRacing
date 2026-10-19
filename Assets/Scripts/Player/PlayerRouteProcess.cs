using UnityEngine;
using Unity.Netcode;

public class PlayerRouteProcess : NetworkBehaviour
{
    public RaceManager raceManager;
    public NetworkVariable<int> currentStage = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Collider playerCollider;
    public Collider wheelCollider;

    public override void OnNetworkSpawn()
    {
        if (raceManager == null)
        {
            raceManager = FindObjectOfType<RaceManager>();
        }
        
        currentStage.OnValueChanged += (oldVal, newVal) => UpdateBarrierCollision();
        if (IsOwner) StartCoroutine(WaitForRouteSequenceThenUpdate());
        currentStage.Value = -1;
    }

    void UpdateBarrierCollision()
    {
        if (!IsOwner) return;

        int myRoute = raceManager.GetRouteForLap(currentStage.Value);
        if (myRoute == -1) return; //Not ready yet 

        foreach (BarrierRoute barrier in FindObjectsOfType<BarrierRoute>())
        {
            bool shouldBlock = barrier.blockedRouteIndex.Contains(myRoute);
            Physics.IgnoreCollision(playerCollider, barrier.barrierCollider, !shouldBlock); // Ignore Physics for barrier
            Physics.IgnoreCollision(wheelCollider, barrier.barrierCollider, !shouldBlock);
            barrier.barrierRender.enabled = shouldBlock;
            barrier.barrierRender.material.color = new Color(1f, 0.5f, 0f); // Orange
        }
    }

    System.Collections.IEnumerator WaitForRouteSequenceThenUpdate()
    {
        while (raceManager.GetRouteForLap(currentStage.Value) == -1)
        {
            yield return null; // wait a frame, try again
        }
        UpdateBarrierCollision();
    }
}
