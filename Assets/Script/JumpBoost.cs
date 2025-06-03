using UnityEngine;

public class JumpBoost : Powerup
{
    [SerializeField] private float jumpForceMultiplier = 1.5f; // Zıplama kuvveti çarpanı
    [SerializeField] private float boostDuration = 4f; // Efekt süresi (saniye)

    protected override void ApplyEffect()
    {
        base.ApplyEffect();
        JumpBoostEffect effect = player.GetComponent<JumpBoostEffect>();
        if (effect == null) effect = player.gameObject.AddComponent<JumpBoostEffect>();
        effect.ActivateJumpBoost(boostDuration, jumpForceMultiplier);
    }
}

public class JumpBoostEffect : MonoBehaviour
{
    private float remainingTime;
    private float originalJumpForce;
    private float jumpForceMultiplier;
    private PlayerController player;
    private bool isActive = false;

    void Start()
    {
        player = GetComponent<PlayerController>();
        originalJumpForce = player.GetComponent<Rigidbody2D>().velocity.y; // Orijinal zıplama kuvvetini sakla
    }

    public void ActivateJumpBoost(float duration, float multiplier)
    {
        if (isActive)
        {
            remainingTime = Mathf.Max(remainingTime, duration);
        }
        else
        {
            isActive = true;
            remainingTime = duration;
            jumpForceMultiplier = multiplier;
            player.GetComponent<Rigidbody2D>().velocity = new Vector2(player.GetComponent<Rigidbody2D>().velocity.x, originalJumpForce * jumpForceMultiplier);
        }
    }

    void Update()
    {
        if (isActive)
        {
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0) DeactivateJumpBoost();
        }
    }

    void DeactivateJumpBoost()
    {
        isActive = false;
        player.GetComponent<Rigidbody2D>().velocity = new Vector2(player.GetComponent<Rigidbody2D>().velocity.x, originalJumpForce);
    }
}