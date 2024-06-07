using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Unity.Collections;

public class Leaderboard : NetworkBehaviour
{
    [SerializeField] private Transform                leaderboardEntitiesHolder;
    [SerializeField] private LeaderBoardEntityDisplay leaderboardEntityPrefab;
    [SerializeField] private int                      maxEntities = 5;

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

        leaderboardEntityDisplays.Sort((a, b) => b.Score.CompareTo(a.Score));

        for (int i = 0; i < leaderboardEntityDisplays.Count; i++)
        {
            leaderboardEntityDisplays[i].transform.SetSiblingIndex(i);
            leaderboardEntityDisplays[i].UpdateName();

            bool show = i <= maxEntities -1;

            leaderboardEntityDisplays[i].gameObject.SetActive(show);
        }
    }

    private void PlayerSpawned(Player player)
    {
        var entityState = new LeaderboardEntityState
        {
            ClientID = player.OwnerClientId,
            PlayerName = player.playerName.Value,
            Score = 0
        };

        player.Score.OnValueChanged += (oldScore, newScore) => OnScoreChange(player.OwnerClientId, newScore);

        leaderboardEntities.Add(entityState);
        UpdatePlayerName(player.OwnerClientId, player.playerName.Value);
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

        player.Score.OnValueChanged -= (oldScore, newScore) => OnScoreChange(player.OwnerClientId, newScore);
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

    private void OnScoreChange(ulong clientId, int newScore)
    {
        for (int i = 0; i < leaderboardEntities.Count; i++)
        {
            if (leaderboardEntities[i].ClientID != clientId) { continue; }

            leaderboardEntities[i] = new LeaderboardEntityState
            {
                ClientID = leaderboardEntities[i].ClientID,
                PlayerName = leaderboardEntities[i].PlayerName,
                Score = newScore
            };
        }
    }
}