using UnityEngine;
using Unity.Netcode;

public class Enemy : Character
{
    [SerializeField] private float shootingRange = 5.0f;
    [SerializeField] private float stopDistance = 2.0f;
    [SerializeField] private float retreatDistance = 1.0f;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float shootingAngle = 45.0f;

    private Transform   target;
    private Vector3     movePosition;
    private float       lastShotTime;

    private void Update()
    {
        FindClosestPlayer();

        if (target != null)
        {
            RotateTowardsTarget();
        }
    }

    private void FixedUpdate()
    {
        if (target != null)
        {
            MoveAndShoot();
        }
    }

    void FindClosestPlayer()
    {
        var players = FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        Player closestPlayer = null;
        float minDistance = float.MaxValue;

        foreach (var player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPlayer = player;
            }
        }

        target = closestPlayer != null ? closestPlayer.transform : null;
    }

    void RotateTowardsTarget()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    void MoveAndShoot()
    {
        if (target == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > stopDistance)
        {
            movePosition = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
            rb.MovePosition(movePosition);
        }
        else if (distanceToTarget < retreatDistance)
        {
            movePosition = Vector3.MoveTowards(transform.position, target.position, -speed * Time.fixedDeltaTime);
            rb.MovePosition(movePosition);
        }

        if (IsAnyPlayerInViewAndRange())
        {
            Shoot();
        }
    }

    private bool IsAnyPlayerInViewAndRange()
    {
        var players = FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);

        foreach (var player in players)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
            if (distanceToPlayer <= shootingRange)
            {
                if (IsInFieldOfView(player.transform))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool IsInFieldOfView(Transform target)
    {
        Vector3 directionToTarget = (target.position - transform.position).normalized;
        float angleToTarget = Vector3.Angle(modelTransform.up, directionToTarget);

        return angleToTarget <= shootingAngle / 2f;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, 0, shootingAngle / 2) * modelTransform.up * shootingRange;
        Vector3 rightBoundary = Quaternion.Euler(0, 0, -shootingAngle / 2) * modelTransform.up * shootingRange;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary);
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary);
    }

    public void IncreaseRotationSpeed(float amount)
    {
        rotationSpeed *= amount;
    }

    private void Shoot()
    {
        if (Time.time - lastShotTime < shotCooldown)
        {
            return;
        }

        lastShotTime = Time.time;

        PrimaryFireServerRpc();
    }

    [ServerRpc]
    private void PrimaryFireServerRpc()
    {
        GameObject projectileInstance = Instantiate(serverLaserPrefab, 
                                                    laserSpawnPoint.position, 
                                                    laserSpawnPoint.rotation);  

        projectileInstance.GetComponent<Projectile>().SetShooter(gameObject);

        SpawnDummyProjectile();
    }

    private void SpawnDummyProjectile()
    {
        Instantiate(clientLaserPrefab, laserSpawnPoint.position, laserSpawnPoint.rotation);
    }
}