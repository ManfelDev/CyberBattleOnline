using UnityEngine;

public class Enemy : Character
{
    [SerializeField] private float shootingRange = 5.0f;
    [SerializeField] private float stopDistance = 2.0f;
    [SerializeField] private float retreatDistance = 1.0f;
    [SerializeField] private float rotationSpeed = 5.0f;
    [SerializeField] private float shootingAngle = 45.0f;

    private Transform target;

    private void Update()
    {
        FindClosestPlayer();

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

    void MoveAndShoot()
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f);
        modelTransform.rotation = Quaternion.Slerp(modelTransform.rotation, lookRotation, Time.deltaTime * rotationSpeed);

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget > stopDistance)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
        }
        else if (distanceToTarget < retreatDistance)
        {
            Vector3 newPosition = Vector3.MoveTowards(transform.position, target.position, -speed * Time.fixedDeltaTime);
            rb.MovePosition(newPosition);
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
}
