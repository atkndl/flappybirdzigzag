using UnityEngine;

public class SlowMotionEffect : MonoBehaviour
{
    private float remainingTime;
    private float targetTimeScale;
    private float originalTimeScale;
    private bool isActive = false;

    public void ActivateSlowMotion(float duration, float timeScale)
    {
        if (isActive)
        {
            remainingTime = Mathf.Max(remainingTime, duration);
        }
        else
        {
            isActive = true;
            remainingTime = duration;
            originalTimeScale = Time.timeScale;
            targetTimeScale = timeScale;
            Time.timeScale = targetTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale; // Fizik güncellemelerini ayarla
            Debug.Log("SlowMotion activated! Time scale set to: " + Time.timeScale);
        }
    }

    void Update()
    {
        if (isActive)
        {
            remainingTime -= Time.unscaledDeltaTime;
            if (remainingTime <= 0) DeactivateSlowMotion();
        }
    }

    void DeactivateSlowMotion()
    {
        isActive = false;
        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = 0.02f; // Varsayılan fizik zaman adımı
        Debug.Log("SlowMotion deactivated! Time scale restored to: " + Time.timeScale);
    }

    void OnDestroy()
    {
        if (isActive) 
        {
            Time.timeScale = 1f;
            Time.fixedDeltaTime = 0.02f;
        }
    }
}