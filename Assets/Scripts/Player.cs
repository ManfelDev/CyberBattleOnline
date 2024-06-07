using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;
using System;

public class Player : Character
{
    [SerializeField] private TextMeshProUGUI playerNameText;

    private Vector2 movement;
    private float lastShotTime;

    public FixedString32Bytes playerName;
    public NetworkVariable<int> Score = new NetworkVariable<int>();
    public int GetScore => Score.Value;

    public static event Action<Player> OnPlayerSpawned;
    public static event Action<Player> OnPlayerDespawned;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            OnPlayerSpawned?.Invoke(this);
        }

        if (IsLocalPlayer)
        {
            playerName = JoinManager.playerName;
            SubmitPlayerNameServerRpc(playerName.Value);
            playerNameText.text = playerName.Value;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnPlayerDespawned?.Invoke(this);
        }
    }

    [ServerRpc]
    private void SubmitPlayerNameServerRpc(string name, ServerRpcParams rpcParams = default)
    {
        playerName = name;
    }

    private void Update()
    {
        if (!IsLocalPlayer) return;

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
        if (!IsLocalPlayer) return;

        // Move the player
        rigidBody.MovePosition(rigidBody.position + movement * speed * Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (!IsLocalPlayer) return;

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
        if (IsLocalPlayer) return;

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