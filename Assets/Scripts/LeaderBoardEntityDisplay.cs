using UnityEngine;
using TMPro;
using Unity.Collections;
using Unity.Netcode;

public class LeaderBoardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI    playerNameText;
    [SerializeField] private Color              playerColor;

    private FixedString32Bytes  playerName;

    public ulong    ClientID { get; private set; }
    public int      Score { get; private set;}
    
    public void Initialize(ulong clientID, FixedString32Bytes playerName, int score)
    {
        ClientID = clientID;
        this.playerName = playerName;

        if (clientID == NetworkManager.Singleton.LocalClientId)
        {
            playerNameText.color = playerColor;
        }

        UpdateScore(score);
    }

    public void UpdateName()
    {
        playerNameText.text = $"{transform.GetSiblingIndex() + 1}. {playerName} - {Score}";
    }

    public void UpdateScore(int newScore)
    {
        Score = newScore;
        UpdateName();
    }
}
