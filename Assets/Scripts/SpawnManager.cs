using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] private List<Transform> playerSpawnLocations;

    public Vector3 GetSpawnPosition()
    {
        var spawnPos = Vector3.zero;
        var currentPlayers = FindObjectsOfType<Player>();
        foreach (var playerSpawnLocation in playerSpawnLocations)
        {
            var closestDist = float.MaxValue;
            foreach (var player in currentPlayers)
            {
                float d = Vector3.Distance(player.transform.position, playerSpawnLocation.position);
                closestDist = Mathf.Min(closestDist, d);
            }
            if (closestDist > 20)
            {
                spawnPos = playerSpawnLocation.position;
                break;
            }
        }
        return spawnPos;
    }
}