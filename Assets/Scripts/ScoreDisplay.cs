using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    private Player player;

    void Update()
    {
        if (player == null)
        {
            player = FindObjectOfType<Player>();
        }
        if (player != null)
        {
            scoreText.text = $"Score: {player.GetScore}";
        }
    }
}