using UnityEngine;

public enum PowerupType
{
    SpeedUp,
    SlowDown
}

public class Powerup : MonoBehaviour
{
    protected string powerupName;
    protected float duration;
    [SerializeField] protected ParticleSystem collectParticles;
    [SerializeField] protected AudioClip collectSound;
    [SerializeField] protected AudioClip powerupSound; // PowerUp alındığında çalacak ses
    [SerializeField] private float lifetime = 5f;
    [SerializeField] protected PowerupType type; // Power-up türü

    protected PlayerController player;
    protected bool isCollected = false;

    public PowerupType Type => type; // Power-up türünü dışarıdan erişilebilir yap
    public float Duration => duration; // Süreyi dışarıdan erişilebilir yap

    public void Initialize(string name, float duration, PlayerController playerReference)
    {
        this.powerupName = name;
        this.duration = duration;
        this.player = playerReference;
        Destroy(gameObject, lifetime);
        Debug.Log($"Powerup initialized: {powerupName}, Type: {type}, Duration: {duration}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D tetiklendi! Çarpan obje: " + other.gameObject.name + ", Tag: " + other.tag);
        if (other.CompareTag("Player") && !isCollected)
        {
            isCollected = true;
            Debug.Log("Powerup toplandı: " + powerupName + ", Type: " + type);
            if (collectParticles != null)
            {
                collectParticles.transform.parent = null;
                collectParticles.Play();
                Destroy(collectParticles.gameObject, collectParticles.main.duration);
            }
            if (collectSound != null)
            {
                AudioSource.PlayClipAtPoint(collectSound, transform.position);
            }
            if (powerupSound != null)
            {
                AudioSource playerAudioSource = player.GetComponent<AudioSource>();
                if (playerAudioSource != null)
                {
                    try
                    {
                        playerAudioSource.PlayOneShot(powerupSound);
                        Debug.Log("powerupSound oynatıldı.");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"powerupSound oynatılamadı: {e.Message}");
                    }
                }
            }
            ApplyEffect();
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning("Powerup alınamadı! Tag: " + other.tag + ", isCollected: " + isCollected);
        }
    }

    protected virtual void ApplyEffect()
    {
        Debug.Log(powerupName + " ApplyEffect çağrıldı! Type: " + type);
        // PlayerController'a power-up tipini ve süresini bildir
        if (player != null)
        {
            player.ApplyPowerup(this);
        }
        else
        {
            Debug.LogError("Player referansı null! Powerup uygulanamadı.");
        }
    }
}