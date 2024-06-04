using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Health))]
public class Character : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject clientLaserPrefab;
    [SerializeField] protected GameObject serverLaserPrefab;
    [SerializeField] protected Transform  laserSpawnPoint;
    [SerializeField] protected Transform  modelTransform;

    [Header("Stats")]
    [SerializeField] protected float      speed = 200;
    [SerializeField] protected float      shotCooldown = 0.5f;

    protected Health      health;
    protected Rigidbody2D rb;

    public override void OnNetworkSpawn() 
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<Health>();
    }

    public void TakeDamage(int damage)
    {
        health.TakeDamage(damage, this.gameObject);
    }

    public void IncreaseSpeed(float amount)
    {
        speed *= amount;
    }
}
