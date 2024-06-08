using System;
using Unity.Collections;
using Unity.Netcode;

public struct LeaderboardEntityState : INetworkSerializable, IEquatable<LeaderboardEntityState>
{
    public ulong              ClientID;
    public FixedString32Bytes PlayerName;
    public int                Score;

    public bool Equals(LeaderboardEntityState other)
    {
        return ClientID == other.ClientID && PlayerName.Equals(other.PlayerName) && Score == other.Score;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientID);
        serializer.SerializeValue(ref PlayerName);
        serializer.SerializeValue(ref Score);
    }
}