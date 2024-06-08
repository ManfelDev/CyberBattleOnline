using TMPro;
using Unity.Netcode;
using UnityEngine;

public class HealingSpace : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject         ActiveModel;
    [SerializeField] private GameObject         InactiveModel;
    [SerializeField] private TextMeshProUGUI    timerText;

    [Header("Settings")]
    [SerializeField] private int                healAmount = 50;
    [SerializeField] private float              healCooldown = 15;

    public NetworkVariable<float> remainingHealCooldown = new NetworkVariable<float>();
    public NetworkVariable<bool> isActive = new NetworkVariable<bool>();

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            isActive.OnValueChanged += OnActiveChanged;
            remainingHealCooldown.OnValueChanged += OnCooldownChanged;
            OnActiveChanged(true, isActive.Value);
            OnCooldownChanged(0, remainingHealCooldown.Value);
        }

        if (IsServer)
        {
            isActive.Value = true;
            remainingHealCooldown.Value = 0;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            isActive.OnValueChanged -= OnActiveChanged;
            remainingHealCooldown.OnValueChanged -= OnCooldownChanged;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out Player player))
        {
            if (isActive.Value)
            {
                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) return;
                
                player.Health.Heal(healAmount);

                isActive.Value = false;

                remainingHealCooldown.Value = healCooldown;
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (!IsServer) return;

        if (other.TryGetComponent(out Player player))
        {
            if (isActive.Value)
            {
                if (player.Health.CurrentHealth.Value == player.Health.MaxHealth) return;
                
                player.Health.Heal(healAmount);

                isActive.Value = false;

                remainingHealCooldown.Value = healCooldown;
            }
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (remainingHealCooldown.Value > 0)
        {
            remainingHealCooldown.Value -= Time.deltaTime;

            if (remainingHealCooldown.Value <= 0)
            {
                isActive.Value = true;
                remainingHealCooldown.Value = 0;
            }
        }
    }

    private void OnCooldownChanged(float previousValue, float newValue)
    {
        timerText.text = newValue > 0 ? $"{newValue:F0}" : string.Empty;
    }

    private void OnActiveChanged(bool previousValue, bool newValue)
    {
        ActiveModel.SetActive(newValue);
        InactiveModel.SetActive(!newValue);
    }

    public void ResetHealingSpace()
    {
        isActive.Value = true;
        remainingHealCooldown.Value = 0;
        OnActiveChanged(false, true);
        OnCooldownChanged(0, 0);
    }
}