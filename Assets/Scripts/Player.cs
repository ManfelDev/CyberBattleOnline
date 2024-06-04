using UnityEngine;

public class Player : Character
{
    private int score;
    private Vector2 movement;

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
}