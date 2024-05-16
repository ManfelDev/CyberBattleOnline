using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float  speed = 1000.0f;
    [SerializeField] private int    damage = 10;
    [SerializeField] private float  lifeTime = 5.0f;

    private GameObject shooter;

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
        // Check if the object collided with is not the shooter
        if (collision.gameObject != shooter)
        {
            var health = collision.GetComponent<Health>();
            if (health != null && !health.isDead)
            {
                health.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
    }

    public void SetShooter(GameObject shooter)
    {
        this.shooter = shooter;
    }
}
