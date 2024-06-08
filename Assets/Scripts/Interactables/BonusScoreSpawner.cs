using UnityEngine;
using Unity.Netcode;

public class BonusScoreSpawner : NetworkBehaviour
{
    [SerializeField] private BonusScore     bonusScorePrefab;
    [SerializeField] private int            maxBonusScore = 50;
    [SerializeField] private int            scoreAdded = 5;
    [SerializeField] private Collider2D     spawnArea;
    [SerializeField] private LayerMask[]    avoidLayers;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        for (int i = 0; i < maxBonusScore; i++)
        {
            SpawnBonusScore();
        }
    }

    private Vector2 SpawnInRandomPosition()
    {
        Vector2 spawnPosition;

        do
        {
            spawnPosition = new Vector2(
                Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y)
            );
        } while (IsPositionInAvoidLayers(spawnPosition));

        return spawnPosition;
    }

    private bool IsPositionInAvoidLayers(Vector2 position)
    {
        foreach (var layer in avoidLayers)
        {
            if (Physics2D.OverlapPoint(position, layer))
            {
                return true;
            }
        }
        return false;
    }

    private void SpawnBonusScore()
    {
        BonusScore bonusScoreInstance = Instantiate(bonusScorePrefab, 
                                                    SpawnInRandomPosition(), 
                                                    Quaternion.identity);

        bonusScoreInstance.SetScore(scoreAdded);
        bonusScoreInstance.GetComponent<NetworkObject>().Spawn();
        bonusScoreInstance.OnPicked += OnBonusScorePicked;                         
    }

    private void OnBonusScorePicked(BonusScore bonusScore)
    {
        bonusScore.transform.position = SpawnInRandomPosition();
        bonusScore.Respawn();
    }
}