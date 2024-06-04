using UnityEngine;
using Unity.Netcode;

public abstract class BonusScore : NetworkBehaviour
{
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    protected int  bonusScore;
    protected bool isCollected;

    public abstract int Pick();

    protected void Display(bool display)
    {
        foreach (var spriteRenderer in spriteRenderers)
        {
            spriteRenderer.enabled = display;
        }
    }
}
