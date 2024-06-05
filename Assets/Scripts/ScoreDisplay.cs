using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private Player localPlayer;

    void Update()
    {
        if (localPlayer == null)
        {
            var players = FindObjectsOfType<Player>();
            foreach (var player in players)
            {
                if (player.NetworkObject.IsLocalPlayer)
                {
                    localPlayer = player;
                    break;
                }
            }
        }
        
        if (localPlayer != null)
        {
            scoreText.text = $"Score: {localPlayer.GetScore}";
        }
    }
}