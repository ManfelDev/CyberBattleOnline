using UnityEngine;

public class Player : Character
{
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform  bulletSpawnPoint;

    private int score;

    protected override void Start()
    {
        base.Start();
        
        score = 0;
    }

    void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - rb.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        rb.rotation = angle;

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
    }

    public void Respawn()
    {
        health.Respawn();
    }

    public void AddScore(int amount)
    {
        score += amount;
    }
}