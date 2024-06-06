using Unity.Netcode;
using UnityEngine;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform                  leaderboardEntitiesHolder;
    [SerializeField] private LeaderBoardEntityDisplay   leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;

    private void Awake() 
    {
        leaderboardEntities = new NetworkList<LeaderboardEntityState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            leaderboardEntities.OnListChanged += OnLeaderboardEntitiesChanged;

            foreach (LeaderboardEntityState entity in leaderboardEntities)
            {
                OnLeaderboardEntitiesChanged(new NetworkListEvent<LeaderboardEntityState>
                {
                    Type = NetworkListEvent<LeaderboardEntityState>.EventType.Add,
                    Value = entity
                });
            }
        }

        if (IsServer)
        {
            Player[] players = FindObjectsOfType<Player>();
            foreach (Player player in players)
            {
                PlayerSpawned(player);
            }

            Player.OnPlayerSpawned += PlayerSpawned;
            Player.OnPlayerDespawned += PlayerDespawned;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            leaderboardEntities.OnListChanged -= OnLeaderboardEntitiesChanged;
        }

        if (IsServer)
        {
            Player.OnPlayerDespawned -= PlayerDespawned;
            Player.OnPlayerSpawned -= PlayerSpawned;
        }
    }

    private void OnLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                Instantiate(leaderboardEntityPrefab, leaderboardEntitiesHolder);
                break;
        }
    }

    private void PlayerSpawned(Player player)
    {
        leaderboardEntities.Add(new LeaderboardEntityState
        {
            ClientID = player.OwnerClientId,
            PlayerName = player.PlayerName.Value,
            Score = 0
        });
    }

    private void PlayerDespawned(Player player)
    {
        if (leaderboardEntities == null) return;

        foreach (LeaderboardEntityState entity in leaderboardEntities)
        {
            if (entity.ClientID == player.OwnerClientId)
            {
                leaderboardEntities.Remove(entity);
                break;
            }
        }
    }
}
