using UnityEngine;

public class SlowMotion : Powerup
{
    [SerializeField] private float timeScale = 0.5f; // Yavaşlatma oranı (0-1 arası, 1 normal hız)
    [SerializeField] private float effectDuration = 3f; // Efektin süresi (saniye)

    protected override void ApplyEffect()
    {
        base.ApplyEffect();
        Debug.Log("SlowMotion ApplyEffect çalışıyor!");
        SlowMotionEffect effect = player.GetComponent<SlowMotionEffect>();
        if (effect == null)
        {
            effect = player.gameObject.AddComponent<SlowMotionEffect>();
            Debug.Log("SlowMotionEffect bileşeni eklendi!");
        }
        effect.ActivateSlowMotion(effectDuration, timeScale);
    }
}