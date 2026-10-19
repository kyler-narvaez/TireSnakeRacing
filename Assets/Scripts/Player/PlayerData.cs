using Unity.Netcode;
using Unity.Collections;
using UnityEngine;

public struct PlayerData : INetworkSerializable, System.IEquatable<PlayerData>
{
    public ulong clientId;
    public FixedString32Bytes playerName;
    public int characterColor;
    public int characterIndex;
    public int tireIndex;
    public int tireColor;

    // Race-result fields — reset at the start of each race
    public int currentLap;
    public float raceStartTime;
    public float totalTime;
    public bool finished;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref clientId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref characterColor);
        serializer.SerializeValue(ref characterIndex);
        serializer.SerializeValue(ref tireIndex);
        serializer.SerializeValue(ref tireColor);
        serializer.SerializeValue(ref currentLap);
        serializer.SerializeValue(ref raceStartTime);
        serializer.SerializeValue(ref totalTime);
        serializer.SerializeValue(ref finished);
    }

    public bool Equals(PlayerData other) => clientId == other.clientId;
}