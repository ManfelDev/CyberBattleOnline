using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Gradient healthGradient;

    private int currentHealth;

    public bool isDead => (currentHealth <= 0);

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage, GameObject shooter)
    {
        currentHealth -= damage;

        UpdateHealthBar();

        if (isDead)
        {
            Die(shooter);
        }
    }

    private void Die(GameObject shooter)
    {
        Player player = GetComponent<Player>();
        if (player != null)
        {
            player.AddScore(10);
        }

        if (player != null)
        {
            RoundManager.Instance.PlayerDied();
            this.gameObject.SetActive(false);
        }
        else
        {
            RoundManager.Instance.EnemyDied();
            Destroy(this.gameObject);
        }
    }

    public void Respawn()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void UpdateHealthBar()
    {
        float healthPercent = (float)currentHealth / maxHealth;
        healthBarImage.fillAmount = healthPercent;
        healthBarImage.color = healthGradient.Evaluate(healthPercent);
    }
}