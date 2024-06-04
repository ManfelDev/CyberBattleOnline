using UnityEngine;
using Unity.Netcode;

public class Player : Character
{
    private int     score;
    private Vector2 movement;
    private float   lastShotTime;

    public int GetScore => score;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        base.OnNetworkSpawn();
        score = 0;
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
        score += amount;
    }

    public void Respawn()
    {
        health.Respawn();
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

        projectileInstance.GetComponent<Projectile>().SetShooter(gameObject);

        SpawnDummyProjectileClientRpc();
    }

    private void SpawnDummyProjectile()
    {
        Instantiate(clientLaserPrefab, laserSpawnPoint.position, laserSpawnPoint.rotation);
    }
}