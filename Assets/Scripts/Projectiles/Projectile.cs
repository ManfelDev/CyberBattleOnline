using UnityEngine;
using Unity.Netcode;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float  speed = 1000.0f;
    [SerializeField] private int    damage = 10;
    [SerializeField] private float  lifeTime = 1.0f;
    [SerializeField] private bool   isClientLaser = false;

    private ulong shooterId;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.attachedRigidbody == null) return;

        if (collision.attachedRigidbody.TryGetComponent<NetworkObject>(out var networkObject))
        {
            if (shooterId == networkObject.OwnerClientId) return;
        }

        if (!isClientLaser)
        {
            if (collision.attachedRigidbody.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
            }
        }

        if (collision.gameObject.layer != LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }

    public void SetShooter(ulong shooterId)
    {
        this.shooterId = shooterId;
    }

    public void SetClientLaser(bool isClientLaser)
    {
        this.isClientLaser = isClientLaser;
    }
}