using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HealingSpace : NetworkBehaviour
{
    [SerializeField] private GameObject      ActiveModel;
    [SerializeField] private GameObject      InactiveModel;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float           healAmount = 50;
    [SerializeField] private float           healCooldown = 15;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out Player player))
        {
            Debug.Log($"Player {player.playerName.Value} entered the healing space");
        }
    }
}
