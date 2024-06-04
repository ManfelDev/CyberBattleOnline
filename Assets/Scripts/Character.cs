using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Health))]
public class Character : NetworkBehaviour
{
    [SerializeField] protected float      speed = 200;
    [SerializeField] protected float      shotCooldown = 0.5f;
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform  bulletSpawnPoint;
    [SerializeField] protected Transform  modelTransform;

    protected Health      health;
    protected Rigidbody2D rb;
    private float         lastShotTime;

    public override void OnNetworkSpawn() 
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
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

    public void IncreaseSpeed(float amount)
    {
        speed *= amount;
    }
}
