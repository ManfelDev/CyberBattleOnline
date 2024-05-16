using UnityEngine;

public class Enemy : Character
{
    [SerializeField] private float shootingRange = 5.0f;
    [SerializeField] private float stopDistance = 2.0f;
    [SerializeField] private float retreatDistance = 1.0f;
    [SerializeField] private float rotationSpeed = 5.0f;

    private Transform target;

    private void Update()
    {
        if (target == null)
        {
            FindClosestPlayer();
        }

        if (target != null)
        {
            MoveAndShoot();
        }
    }

    void FindClosestPlayer()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
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
        Quaternion lookRotation = Quaternion.LookRotation(Vector3.forward, direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.fixedDeltaTime * rotationSpeed);

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

        if (distanceToTarget <= shootingRange)
        {
            Shoot();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, shootingRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);
    }
}