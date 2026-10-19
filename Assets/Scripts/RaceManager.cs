
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RaceManager : NetworkBehaviour
{
    public int numberOfRoutes;
    public NetworkList<int> routeSequence;
    public int numberOfLaps;
    public bool isRandom;
    bool isSameSequence;

    private void Awake()
    {
        routeSequence = new NetworkList<int>();
    }

    public override void OnNetworkSpawn()
    {
        StartCoroutine(WaitForGameSettings());
    }

    System.Collections.IEnumerator WaitForGameSettings()
    {
        while (GameSettings.Instance == null)
        {
            yield return null;
        }
        SetSettings();
        if (IsHost) GenerateRouteSequence();
    }

    void SetSettings()
    {
        numberOfLaps = GameSettings.Instance.numberOfLaps.Value;
        isRandom = GameSettings.Instance.isRandom.Value;
        isSameSequence = GameSettings.Instance.isSameSequence.Value;
    }

    void GenerateRouteSequence()
    {
        for (int lap = 0; lap < numberOfLaps; ++lap)
        {
            int routeIndex = isRandom
                ? Random.Range(0, numberOfRoutes)
                : lap % numberOfRoutes;

            routeSequence.Add(routeIndex);
        }

    }

    public int GetRouteForLap(int lapIndex)
    {
        if (routeSequence == null || routeSequence.Count == 0) return -1; // Does not exist yet
        if (lapIndex < 0 || lapIndex >= numberOfLaps) return -1; //Race done
        return routeSequence[lapIndex];
    }

    void ResetAllRaceResults()
    {
        for (int i = 0; i < PlayerDataManager.Instance.players.Count; i++)
        {
            PlayerData data = PlayerDataManager.Instance.players[i];
            data.currentLap = 0;
            data.raceStartTime = Time.time;
            data.totalTime = 0f;
            data.finished = false;
            PlayerDataManager.Instance.players[i] = data; // NetworkList requires reassigning the whole struct to trigger sync
        }
    }
}
