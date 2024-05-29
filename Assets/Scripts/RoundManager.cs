using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class RoundManager : MonoBehaviour
{
    [SerializeField] private GameObject       enemyPrefab;
    [SerializeField] private Collider2D       spawnArea;
    [SerializeField] private int              minPlayers = 1;
    [SerializeField] private int              maxRound = 5;
    [SerializeField] private float            timeForNextRound = 3f;
    [SerializeField] private float            minSpawnDistanceFromPlayer = 5f;
    [SerializeField] private int              numberOfStartingEnemies = 2;
    [SerializeField] private float            enemyDifficultyMultiplier = 1.25f;
    [SerializeField] private GameObject       endOfRoundPanel;
    [SerializeField] private TextMeshProUGUI  endOfRoundText;
    [SerializeField] private TextMeshProUGUI  scoresText;
    [SerializeField] private Transform[]      playersSpawnPoints;

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

        for (int i = 0; i < players.Length; i++)
        {
            players[i].transform.position = playersSpawnPoints[i % playersSpawnPoints.Length].position;
        }

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

        int enemiesToSpawn = numberOfStartingEnemies * currentRound;
        enemiesAlive = enemiesToSpawn;

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector2 spawnPosition = GetSpawnPositionAwayFromPlayers();
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

    private Vector2 GetSpawnPositionAwayFromPlayers()
    {
        Vector2 randomPosition;
        bool validPosition;

        do
        {
            Bounds bounds = spawnArea.bounds;
            randomPosition = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            validPosition = true;
            foreach (var player in players)
            {
                if (Vector2.Distance(randomPosition, player.transform.position) < minSpawnDistanceFromPlayer)
                {
                    validPosition = false;
                    break;
                }
            }
        } while (!validPosition);

        return randomPosition;
    }

    private IEnumerator NextRound()
    {
        isRoundTransitioning = true;

        if (currentRound >= maxRound)
        {
            DisplayEndOfRoundScreen();
            StopAllPlayers();
            // GameOver();
            yield break;
        }

        RespawnPlayers();
        StopAllPlayers();
        DisplayEndOfRoundScreen();

        yield return new WaitForSecondsRealtime(timeForNextRound);

        endOfRoundPanel.SetActive(false);
        StartAllPlayers();
        currentRound++;

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
        ShuffleSpawnPoints();

        for (int i = 0; i < players.Length; i++)
        {
            var player = players[i];
            player.transform.position = playersSpawnPoints[i % playersSpawnPoints.Length].position;
            player.gameObject.SetActive(true);
            player.GetComponent<Health>().Respawn();
        }
        playersAlive = players.Length;
        Debug.Log("Respawning all players. Players Alive: " + playersAlive);
    }

    private void ShuffleSpawnPoints()
    {
        for (int i = 0; i < playersSpawnPoints.Length; i++)
        {
            Transform temp = playersSpawnPoints[i];
            int randomIndex = Random.Range(i, playersSpawnPoints.Length);
            playersSpawnPoints[i] = playersSpawnPoints[randomIndex];
            playersSpawnPoints[randomIndex] = temp;
        }
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
        if (currentRound.Equals(maxRound))
        {
            endOfRoundPanel.SetActive(true);
            endOfRoundText.text = "Game Over!";
            scoresText.text     = "Winner: " + GetWinner();
        }
        else
        {
            endOfRoundPanel.SetActive(true);
            endOfRoundText.text = "End of Round " + currentRound;
            scoresText.text     = GetScores();
        }
    }

    private string GetScores()
    {
        var topPlayers = players.OrderByDescending(p => p.GetScore).Take(3);
        string scores = "";
        
        foreach (var player in topPlayers)
        {
            scores += player.name + ": " + player.GetScore + "\n";
        }

        return scores;
    }

    private string GetWinner()
    {
        var topPlayer = players.OrderByDescending(p => p.GetScore).First();

        return topPlayer.name;
    }
}