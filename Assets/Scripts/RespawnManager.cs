using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class RespawnManager : NetworkBehaviour
{
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private float         respawnDelay = 5.0f;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) { return; }

        Player[] players = FindObjectsOfType<Player>();
        foreach (Player player in players)
        {
            PlayerSpawned(player);
        }

        Player.OnPlayerSpawned += PlayerSpawned;
        Player.OnPlayerDespawned += PlayerDespawned;
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) { return; }

        Player.OnPlayerSpawned -= PlayerSpawned;
        Player.OnPlayerDespawned -= PlayerDespawned;
    }

    private void PlayerSpawned(Player player)
    {
        player.Health.OnDie += (health) => PlayerDie(player);
    }

    private void PlayerDespawned(Player player)
    {
        player.Health.OnDie -= (health) => PlayerDie(player);
    }

    private void PlayerDie(Player player)
    {
        Destroy(player.gameObject);

        StartCoroutine(RespawnPlayer(player.OwnerClientId));
    }

    private IEnumerator RespawnPlayer(ulong ownerClientId)
    {
        yield return new WaitForSeconds(respawnDelay);

        Vector3 spawnPosition = FindObjectOfType<SpawnManager>().GetSpawnPosition();

        NetworkObject playerInstance = Instantiate(
            playerPrefab, new Vector3(0,0,0) , Quaternion.identity);

        playerInstance.SpawnAsPlayerObject(ownerClientId);
    }
}
