using System.Collections;
using System.Collections.Generic;
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
}
