using UnityEngine;
using Unity.Netcode;

public class Player : Character
{
    private Vector2 movement;
    private float   lastShotTime;

    public NetworkVariable<int> Score = new NetworkVariable<int>();
    public int GetScore => Score.Value;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        base.OnNetworkSpawn();
        Score.Value = 0;
    }

    void Update()
    {
        if (!IsOwner) return;

        // Get input for movement
        movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        // Handle shooting
        if (Input.GetButton("Fire1"))
        {
            Shoot();
        }
    }

    private void FixedUpdate()
    {
        if (!IsOwner) return;

        // Move the player
        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);
    }

    private void LateUpdate() 
    {
        if (!IsOwner) return;

        // Get mouse position for rotation
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - (Vector2)modelTransform.position;

        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        modelTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void AddScore(int amount)
    {
        Score.Value += amount;
    }

    public void Respawn()
    {
        health.Respawn();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<BonusScore>(out BonusScore bonusScore)) return;

        int score = bonusScore.Pick();

        if (!IsServer) return;

        Score.Value += score;
    }

    protected void Shoot()
    {
        if (Time.time - lastShotTime < shotCooldown)
        {
            return;
        }

        lastShotTime = Time.time;

        PrimaryFireServerRpc();
        SpawnDummyProjectile();
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc()
    {
        if (IsOwner) return;

        SpawnDummyProjectile();
    }

    [ServerRpc]
    private void PrimaryFireServerRpc()
    {
        GameObject projectileInstance = Instantiate(serverLaserPrefab, 
                                                    laserSpawnPoint.position, 
                                                    laserSpawnPoint.rotation);

        if (projectileInstance.TryGetComponent<Projectile>(out Projectile dealDamage))
        {
            dealDamage.SetShooter(OwnerClientId);
        }

        SpawnDummyProjectileClientRpc();
    }

    private void SpawnDummyProjectile()
    {
        Instantiate(clientLaserPrefab, laserSpawnPoint.position, laserSpawnPoint.rotation);
    }
}