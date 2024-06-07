using UnityEngine;
using TMPro;
using Unity.Collections;

public class LeaderBoardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    private ulong               clientID;
    private FixedString32Bytes  playerName;
    private int                 score;

    public ulong    ClientID { get; private set; }
    public int      Score { get; private set;}
    
    public void Initialize(ulong clientID, FixedString32Bytes playerName, int score)
    {
        ClientID = clientID;
        this.playerName = playerName;

        UpdateScore(score);
    }

    private void UpdateName()
    {
        playerNameText.text = $"1. {playerName} - {score}";
    }

    public void UpdateScore(int newScore)
    {
        score = newScore;
        UpdateName();
    }
}
