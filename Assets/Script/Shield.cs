using UnityEngine;

public class Shield : Powerup
{
    [SerializeField] private GameObject shieldVisual;
    [SerializeField] private float shieldDuration = 5f; // Kalkanın süresi (saniye)

    protected override void ApplyEffect()
    {
        base.ApplyEffect();
        ShieldEffect effect = player.GetComponent<ShieldEffect>();
        if (effect == null) effect = player.gameObject.AddComponent<ShieldEffect>();
        effect.ActivateShield(shieldDuration, shieldVisual);
    }
}