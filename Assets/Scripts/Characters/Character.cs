using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(Health))]
public class Character : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] protected GameObject  clientLaserPrefab;
    [SerializeField] protected GameObject  serverLaserPrefab;
    [SerializeField] protected Transform   laserSpawnPoint;
    [SerializeField] protected Transform   modelTransform;
    [SerializeField] protected Health      health;
    [SerializeField] protected Rigidbody2D rigidBody;

    [Header("Settings")]
    [SerializeField] protected float      speed = 200;
    [SerializeField] protected float      shotCooldown = 0.5f;

    public Health Health => health;

    public void IncreaseSpeed(float amount)
    {
        speed *= amount;
    }
}
