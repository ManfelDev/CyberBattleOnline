using UnityEngine;

[RequireComponent(typeof(Health))]
public class Character : MonoBehaviour
{
    [SerializeField] protected float speed = 100;

    protected Health health;
    protected Rigidbody2D rb;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage)
    {
        health.TakeDamage(damage);
    }
}
