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

        Vector3 spawnPos = laserSpawnPoint.position;
        Vector3 direction = laserSpawnPoint.up;

        PrimaryFireServerRpc(spawnPos, direction);
        SpawnDummyProjectile(spawnPos, direction);
    }

    [ClientRpc]
    private void SpawnDummyProjectileClientRpc(Vector3 spawnPos, Vector3 direction)
    {
        if (IsOwner) return;

        SpawnDummyProjectile(spawnPos, direction);
    }

    [ServerRpc]
    private void PrimaryFireServerRpc(Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate(serverLaserPrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;

        if (projectileInstance.TryGetComponent<Projectile>(out Projectile projectile))
        {
            projectile.SetShooter(OwnerClientId);
        }

        SpawnDummyProjectileClientRpc(spawnPos, direction);
    }

    private void SpawnDummyProjectile(Vector3 spawnPos, Vector3 direction)
    {
        GameObject projectileInstance = Instantiate(clientLaserPrefab, spawnPos, Quaternion.identity);
        projectileInstance.transform.up = direction;
    }
}