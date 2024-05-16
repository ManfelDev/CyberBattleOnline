using UnityEngine;

[RequireComponent(typeof(Health))]
public class Character : MonoBehaviour
{
    [SerializeField] protected float speed = 200;
    [SerializeField] protected float shotCooldown = 0.5f;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform  bulletSpawnPoint;

    protected Health      health;
    protected Rigidbody2D rb;
    private float         lastShotTime;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage)
    {
        health.TakeDamage(damage, this.gameObject);
    }

    protected void Shoot()
    {
        if (Time.time - lastShotTime < shotCooldown)
        {
            return;
        }

        lastShotTime = Time.time;

        GameObject projectile = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        Projectile projectileScript = projectile.GetComponent<Projectile>();
        projectileScript.SetShooter(gameObject);
    }
}
