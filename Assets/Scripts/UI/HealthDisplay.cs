using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class HealthDisplay : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private Health     health;
    [SerializeField] private Image      healthBarImage;
    [SerializeField] private Gradient   healthGradient;

    public override void OnNetworkSpawn()
    {
        health.CurrentHealth.OnValueChanged += UpdateHealth;
        
        UpdateHealth(0, health.CurrentHealth.Value);
    }

    public override void OnNetworkDespawn()
    {
        health.CurrentHealth.OnValueChanged -= UpdateHealth;
    }

    public void UpdateHealth(int oldHealth, int newHealth)
    {
        float healthPercent = (float)health.CurrentHealth.Value / health.MaxHealth;
        healthBarImage.fillAmount = healthPercent;
        healthBarImage.color = healthGradient.Evaluate(healthPercent);
    }
}
