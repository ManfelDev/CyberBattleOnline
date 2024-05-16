using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;

    private int currentHealth;

    public bool isDead => (currentHealth <= 0);

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (isDead)
        {
            Die();
        }
    }

    private void Die()
    {
        this.gameObject.SetActive(false);
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        this.gameObject.SetActive(true);
    }
}
