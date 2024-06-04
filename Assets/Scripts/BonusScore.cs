using UnityEngine;
using Unity.Netcode;
using System;

public class BonusScore : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    private int     bonusScore;
    private bool    isCollected;
    private Vector3 previousPosition;
    
    public event Action<BonusScore> OnPicked;

    private void Update()
    {
        if (previousPosition != transform.position)
        {
            Display(true);
        }

        previousPosition = transform.position;
    }

    public void SetScore(int score)
    {
        bonusScore = score;
    }

    protected void Display(bool display)
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = display;
        }
    }

    public int Pick()
    {
        if (!IsServer)
        {
            Display(false);
            return 0;
        }

        if (isCollected) return 0;

        isCollected = true;

        OnPicked?.Invoke(this);

        return bonusScore;
    }

    public void Respawn()
    {
        isCollected = false;
    }
}
