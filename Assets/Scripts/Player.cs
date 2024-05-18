using UnityEngine;

public class Player : Character
{
    private int score;

    public int GetScore => score;

    protected override void Start()
    {
        base.Start();
        score = 0;
    }

    void Update()
    {
        Vector2 movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lookDir = mousePos - (Vector2)modelTransform.position;
        float angle = Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg - 90f;
        modelTransform.rotation = Quaternion.Euler(0, 0, angle);

        rb.MovePosition(rb.position + movement * speed * Time.fixedDeltaTime);

        if (Input.GetButton("Fire1"))
        {
            Shoot();
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
    }

    public void Respawn()
    {
        health.Respawn();
    }
}