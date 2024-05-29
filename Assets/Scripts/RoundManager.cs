using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private GameObject             enemyPrefab;
    [SerializeField] private Collider2D             spawnArea;
    [SerializeField] private int                    minPlayers = 1;
    [SerializeField] private int                    maxRound = 5;
    [SerializeField] private float                  timeForNextRound = 3f;
    [SerializeField] private float                  minSpawnDistanceFromPlayer = 5f;
    [SerializeField] private int                    baseEnemyCount = 2;
    [SerializeField] private float                  enemyDifficultyMultiplier = 1.25f;
    [SerializeField] private GameObject             endOfRoundPanel;
    [SerializeField] private TMPro.TextMeshProUGUI  endOfRoundText;
    [SerializeField] private TMPro.TextMeshProUGUI  scoresText;

    private int         currentRound;
    private int         enemiesAlive;
    private int         playersAlive;
    private Player[]    players;
    private bool        isRoundTransitioning;

    public static RoundManager Instance { get; private set; }

    private void Start()
    {
        players = FindObjectsOfType<Player>();
        playersAlive = players.Length;

        if (playersAlive < minPlayers)
        {
            Debug.LogWarning("Not enough players to start spawning enemies.");
            return;
        }

        currentRound = 1;
        SpawnEnemies();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (enemiesAlive == 0 && !isRoundTransitioning)
        {
            StartCoroutine(NextRound());
        }

        if (playersAlive == 0 && !isRoundTransitioning)
        {
            EliminateRemainingEnemies();
            StartCoroutine(NextRound());
        }
    }

    private void SpawnEnemies()
    {
        if (playersAlive < minPlayers)
        {
            return;
        }

        int enemiesToSpawn = baseEnemyCount * currentRound;
        enemiesAlive = enemiesToSpawn;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector2 spawnPosition = GetSpawnPositionAwayFromPlayer(players[Random.Range(0, players.Length)].transform.position);
            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            Enemy enemy = enemyInstance.GetComponent<Enemy>();
            if (enemy != null)
            {
                float speedMultiplier = 1 + ((currentRound - 1) * (enemyDifficultyMultiplier - 1));
                enemy.IncreaseSpeed(speedMultiplier);
                enemy.IncreaseRotationSpeed(speedMultiplier);
            }
        }
    }

    private Vector2 GetSpawnPositionAwayFromPlayer(Vector2 playerPosition)
    {
        Vector2 randomPosition;

        do
        {
            Bounds bounds = spawnArea.bounds;
            randomPosition = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );
        } while (Vector2.Distance(randomPosition, playerPosition) < minSpawnDistanceFromPlayer);

        return randomPosition;
    }

    private IEnumerator NextRound()
    {
        isRoundTransitioning = true;

        if (currentRound >= maxRound)
        {
            Debug.Log("Game Over!");
            yield break;
        }

        StopAllPlayers();
        DisplayEndOfRoundScreen();

        yield return new WaitForSecondsRealtime(timeForNextRound);

        endOfRoundPanel.SetActive(false);
        StartAllPlayers();
        currentRound++;

        RespawnPlayers();
        SpawnEnemies();

        isRoundTransitioning = false;
    }

    public void EnemyDied()
    {
        enemiesAlive--;
    }

    public void PlayerDied()
    {
        playersAlive--;
    }

    private void RespawnPlayers()
    {
        foreach (var player in players)
        {
            player.gameObject.SetActive(true);
            player.GetComponent<Health>().Respawn();
        }
        playersAlive = players.Length;
        Debug.Log("Respawning all players. Players Alive: " + playersAlive);
    }

    private void StopAllPlayers()
    {
        foreach (var player in players)
        {
            player.GetComponent<Player>().enabled = false;
        }
    }

    private void StartAllPlayers()
    {
        foreach (var player in players)
        {
            player.GetComponent<Player>().enabled = true;
        }
    }

    private void EliminateRemainingEnemies()
    {
        Enemy[] remainingEnemies = FindObjectsOfType<Enemy>();
        foreach (var enemy in remainingEnemies)
        {
            Destroy(enemy.gameObject);
        }
        enemiesAlive = 0;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw min spawn distance circle
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, minSpawnDistanceFromPlayer);
    }

    private void DisplayEndOfRoundScreen()
    {
        endOfRoundPanel.SetActive(true);
        endOfRoundText.text = "End of Round " + currentRound;
        scoresText.text = GetScores();
    }

    private string GetScores()
    {
        string scores = "";
        
        foreach (var player in players)
        {
            scores += player.name + ": " + player.GetScore + "\n";
        }
        return scores;
    }
}