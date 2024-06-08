using System.Collections;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class GameManager : NetworkBehaviour
{
    [SerializeField] private float              gameDuration = 600f;
    [SerializeField] private TextMeshProUGUI    timerText;
    [SerializeField] private GameObject         endGameUI;
    [SerializeField] private TextMeshProUGUI    endGameText;
    [SerializeField] private float              newGameCountdownDuration = 10f;

    private float timer;
    private bool gameRunning = false;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            StartGame();
        }
    }

    private void StartGame()
    {
        timer = gameDuration;
        gameRunning = true;
        StartCoroutine(GameTimerCoroutine());
    }

    private IEnumerator GameTimerCoroutine()
    {
        while (gameRunning && timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
            UpdateTimerClientRpc(timer);
        }

        if (timer <= 0)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        gameRunning = false;

        EndGameServerRpc();
        EndGameClientRpc();

        StartCoroutine(NewGameCountdownCoroutine());
    }

    private string GetPlayerWithHighestScore()
    {
        Player[] players = FindObjectsOfType<Player>();
        string highestScoringPlayer = null;
        int highestScore = int.MinValue;

        foreach (var player in players)
        {
            if (player.GetScore > highestScore)
            {
                highestScore = player.GetScore;
                highestScoringPlayer = player.playerName.Value;
            }
        }
        return highestScoringPlayer;
    }

    private IEnumerator NewGameCountdownCoroutine()
    {
        float countdown = newGameCountdownDuration;

        while (countdown > 0)
        {
            UpdateEndGameTextClientRpc($"Winner: {GetPlayerWithHighestScore()}\n\nNew game starts in: {countdown} seconds");
            yield return new WaitForSeconds(1f);
            countdown--;
        }

        UpdateEndGameTextClientRpc(string.Empty);
        RestartGame();
    }

    private void RestartGame()
    {
        foreach (var player in FindObjectsOfType<Player>())
        {
            player.Health.CurrentHealth.Value = player.Health.MaxHealth;
            player.Score.Value = 0;
            player.EnableMovementAndShooting();
        }

        ResetHealingSpaces();

        StartGame();
        StartNewGameClientRpc();
    }

    private void ResetHealingSpaces()
    {
        foreach (var healingSpace in FindObjectsOfType<HealingSpace>())
        {
            healingSpace.ResetHealingSpace();
        }
    }

    [ClientRpc]
    private void UpdateTimerClientRpc(float currentTime)
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);

            timerText.text = $"Time Left: {minutes:00}:{seconds:00}";
        }
    }

    [ClientRpc]
    private void UpdateEndGameTextClientRpc(string text)
    {
        if (endGameText != null)
        {
            endGameText.text = text;
        }
    }

    [ServerRpc]
    private void EndGameServerRpc()
    {
        foreach (var projectile in FindObjectsOfType<Projectile>())
        {
            projectile.gameObject.SetActive(false);
        }
    }

    [ClientRpc]
    private void EndGameClientRpc()
    {
        endGameUI.SetActive(true);

        foreach (var player in FindObjectsOfType<Player>())
        {
            player.DisableMovementAndShooting();
        }

        foreach (var projectile in FindObjectsOfType<Projectile>())
        {
            projectile.gameObject.SetActive(false);
        }
    }


    [ClientRpc]
    private void StartNewGameClientRpc()
    {
        endGameUI.SetActive(false);

        foreach (var player in FindObjectsOfType<Player>())
        {
            Vector3 randomPos = FindObjectOfType<SpawnManager>().GetSpawnPosition();
            
            player.transform.position = randomPos;
            player.EnableMovementAndShooting();
        }
    }
}