using UnityEngine;

public class ShieldEffect : MonoBehaviour
{
    private float remainingTime;
    private GameObject shieldVisual;
    private bool isActive = false;

    public void ActivateShield(float duration, GameObject visualPrefab)
    {
        if (isActive)
        {
            remainingTime = Mathf.Max(remainingTime, duration);
        }
        else
        {
            isActive = true;
            remainingTime = duration;
            if (visualPrefab != null && shieldVisual == null)
            {
                shieldVisual = Instantiate(visualPrefab, transform.position, Quaternion.identity, transform);
            }
        }
    }

    void Update()
    {
        if (isActive)
        {
            remainingTime -= Time.deltaTime;
            if (shieldVisual != null) shieldVisual.transform.position = transform.position;
            if (remainingTime <= 0) DeactivateShield();
        }
    }

    void DeactivateShield()
    {
        isActive = false;
        if (shieldVisual != null)
        {
            Destroy(shieldVisual);
            shieldVisual = null;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isActive && collision.gameObject.CompareTag("Obstacle"))
        {
            Physics2D.IgnoreCollision(collision.collider, GetComponent<Collider2D>(), true);
            DeactivateShield();
            Invoke("ReenableCollision", 0.5f);
        }
    }

    void ReenableCollision()
    {
        Collider2D[] allColliders = FindObjectsOfType<Collider2D>();
        Collider2D playerCollider = GetComponent<Collider2D>();
        foreach (Collider2D collider in allColliders)
        {
            Physics2D.IgnoreCollision(collider, playerCollider, false);
        }
    }
}