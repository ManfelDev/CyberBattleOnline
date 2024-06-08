using UnityEngine;
using Unity.Netcode;
using System;

public class Health : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    public NetworkVariable<int> CurrentHealth = new NetworkVariable<int>();

    public bool isDead => (CurrentHealth.Value <= 0);
    public int MaxHealth => maxHealth;

    public Action<Health> OnDie;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        CurrentHealth.Value = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        ModifyHealth(-damage);
    }

    public void Heal(int amount)
    {
        ModifyHealth(amount);
    }

    private void ModifyHealth(int amount)
    {
        if (isDead) return;

        int newHealth = CurrentHealth.Value + amount;
        CurrentHealth.Value = Mathf.Clamp(newHealth, 0, maxHealth);

        if (isDead)
        {
            OnDie?.Invoke(this);
        }
    }

    /*private void Die(GameObject shooter)
    {
        Player player = GetComponent<Player>();

        if (player != null)
        {
            RoundManager.Instance.PlayerDied();
            this.gameObject.SetActive(false);

            if (shooter != null)
            {
                Player playerShooter = shooter.GetComponent<Player>();
                if (playerShooter != null)
                {
                    playerShooter.AddScore(20);
                    Debug.Log("Score: " + playerShooter.GetScore);
                }
            }
        }

        else
        {
            RoundManager.Instance.EnemyDied();
            Destroy(this.gameObject);

            if (shooter != null)
            {
                Player playerShooter = shooter.GetComponent<Player>();
                if (playerShooter != null)
                {
                    playerShooter.AddScore(10);
                    Debug.Log("Score " + playerShooter.name + ": "+ playerShooter.GetScore);
                }
            }
        }
    }*/
}