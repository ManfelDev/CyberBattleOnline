using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform leaderboardEntitiesHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderboardEntityPrefab;

    private NetworkList<LeaderboardEntityState> leaderboardEntities;
    private List<LeaderBoardEntityDisplay> leaderboardEntityDisplays = new List<LeaderBoardEntityDisplay>();

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
            Player.OnPlayerNameChanged += UpdatePlayerName;
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
            Player.OnPlayerNameChanged -= UpdatePlayerName;
        }
    }

    private void OnLeaderboardEntitiesChanged(NetworkListEvent<LeaderboardEntityState> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<LeaderboardEntityState>.EventType.Add:
                if (!leaderboardEntityDisplays.Any(entity => entity.ClientID == changeEvent.Value.ClientID))
                {
                    LeaderBoardEntityDisplay leaderBoardEntity = Instantiate(leaderboardEntityPrefab, leaderboardEntitiesHolder);
                    leaderBoardEntity.Initialize(changeEvent.Value.ClientID, changeEvent.Value.PlayerName, changeEvent.Value.Score);
                    leaderboardEntityDisplays.Add(leaderBoardEntity);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Remove:
                LeaderBoardEntityDisplay leaderBoardEntityDisplayToRemove = leaderboardEntityDisplays.FirstOrDefault(entity => entity.ClientID == changeEvent.Value.ClientID);
                if (leaderBoardEntityDisplayToRemove != null)
                {
                    leaderBoardEntityDisplayToRemove.transform.SetParent(null);
                    Destroy(leaderBoardEntityDisplayToRemove.gameObject);
                    leaderboardEntityDisplays.Remove(leaderBoardEntityDisplayToRemove);
                }
                break;
            case NetworkListEvent<LeaderboardEntityState>.EventType.Value:
                LeaderBoardEntityDisplay leaderBoardEntityDisplayToUpdate = leaderboardEntityDisplays.FirstOrDefault(entity => entity.ClientID == changeEvent.Value.ClientID);
                if (leaderBoardEntityDisplayToUpdate != null)
                {
                    leaderBoardEntityDisplayToUpdate.UpdateScore(changeEvent.Value.Score);
                }
                break;
        }
    }

    private void PlayerSpawned(Player player)
    {
        var entityState = new LeaderboardEntityState
        {
            ClientID = player.OwnerClientId,
            PlayerName = player.playerName,
            Score = 0
        };

        leaderboardEntities.Add(entityState);
        UpdatePlayerName(player.OwnerClientId, player.playerName);
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

    private void UpdatePlayerName(ulong clientId, FixedString32Bytes playerName)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientID == clientId)
            {
                var updatedEntity = leaderboardEntities[i];
                updatedEntity.PlayerName = playerName;
                leaderboardEntities[i] = updatedEntity;

                // Notify clients of the name change
                UpdatePlayerNameClientRpc(clientId, playerName);
                break;
            }
        }
    }

    [ClientRpc]
    private void UpdatePlayerNameClientRpc(ulong clientId, FixedString32Bytes playerName)
    {
        var display = leaderboardEntityDisplays.FirstOrDefault(entity => entity.ClientID == clientId);
        if (display != null)
        {
            display.Initialize(clientId, playerName, display.Score);
        }
    }
}