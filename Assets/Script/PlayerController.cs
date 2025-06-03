using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems; // UI kontrolü için

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource; // Ses efektleri için
    private AudioSource themesongSource; // Arka plan müziği için
    [SerializeField] private float flapForce = 5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float maxFlapHeight = 4f;
    [SerializeField] private float screenBoundary = 10f;
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private AudioClip tapSound;
    [SerializeField] private AudioClip pointSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip gameOverUISound;
    [SerializeField] private AudioClip themesong; // Oyun arka plan müziği
    private bool isAlive = false;
    private int score = 0;
    public bool movingRight = true; // GameManager'ın erişebilmesi için public
    private GameManager gameManager;
    [SerializeField] private FeedbackManager feedbackManager;
    [SerializeField] private UIManager uiManager;
    private AudioManager audioManager; // AudioManager referansı
    private float originalMoveSpeed; // Orijinal hızı saklamak için
    private float originalPitch; // Orijinal pitch değerini saklamak için

    public delegate void DirectionChange(bool isMovingRight);
    public static event DirectionChange OnDirectionChange;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource (ses efektleri için) eklendi.");
        }

        // Arka plan müziği için ayrı bir AudioSource ekle
        themesongSource = gameObject.AddComponent<AudioSource>();
        themesongSource.playOnAwake = false;
        themesongSource.loop = true; // Arka plan müziği döngüye alınacak
        themesongSource.clip = themesong;
        themesongSource.volume = 0.5f; // Varsayılan ses seviyesi
        originalPitch = themesongSource.pitch; // Orijinal pitch değerini sakla (genellikle 1.0)
        Debug.Log($"ThemesongSource initialized: Clip = {themesongSource.clip?.name}, Pitch = {originalPitch}");

        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>(); // AudioManager'ı bul
        if (mainCamera == null) mainCamera = Camera.main;
        rb.gravityScale = 0f;

        if (uiManager == null) Debug.LogError("UIManager atanmamış!");
        if (feedbackManager == null) Debug.LogWarning("FeedbackManager atanmamış!");
        if (audioManager == null) Debug.LogError("AudioManager atanmamış!");

        // AudioSource ayarlarını kontrol et ve logla
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.volume = 1f;
            Debug.Log("AudioSource başarıyla ayarlandı.");
        }

        // Ses dosyalarını kontrol et
        if (tapSound == null) Debug.LogWarning("tapSound atanmamış!");
        if (pointSound == null) Debug.LogWarning("pointSound atanmamış!");
        if (deathSound == null) Debug.LogWarning("deathSound atanmamış!");
        if (gameOverUISound == null) Debug.LogWarning("gameOverUISound atanmamış!");
        if (themesong == null) Debug.LogWarning("themesong atanmamış!");

        // Orijinal hızı sakla
        originalMoveSpeed = moveSpeed;

        // EventSystem kontrolü
        if (EventSystem.current == null)
        {
            Debug.LogError("Sahnedeki EventSystem eksik! Lütfen bir EventSystem ekleyin.");
        }
    }

    void Update()
    {
        if (!isAlive)
        {
            bool isInputDetected = false;

            // Dokunmatik giriş kontrolü
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current != null)
                {
                    bool isOverUI = EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
                    Debug.Log($"Dokunmatik giriş algılandı. UI üzerinde mi? {isOverUI}");
                    if (!isOverUI)
                    {
                        isInputDetected = true;
                    }
                }
                else
                {
                    Debug.LogWarning("EventSystem.current null, UI kontrolü yapılamıyor. Giriş kabul ediliyor.");
                    isInputDetected = true;
                }
            }
            // Fare veya klavye giriş kontrolü
            else if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                if (EventSystem.current != null)
                {
                    bool isOverUI = EventSystem.current.IsPointerOverGameObject();
                    Debug.Log($"Fare/Klavye girişi algılandı. UI üzerinde mi? {isOverUI}");
                    if (!isOverUI)
                    {
                        isInputDetected = true;
                    }
                }
                else
                {
                    Debug.LogWarning("EventSystem.current null, UI kontrolü yapılamıyor. Giriş kabul ediliyor.");
                    isInputDetected = true;
                }
            }

            if (isInputDetected)
            {
                Debug.Log("Giriş kabul edildi, oyun başlatılıyor.");
                StartGame();
            }

            CheckScreenBoundaries();
        }
        else
        {
            float moveDirection = movingRight ? 1f : -1f;
            rb.velocity = new Vector2(moveSpeed * moveDirection, rb.velocity.y);

            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began || 
                Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                Flap();
            }

            CheckScreenBoundaries();
        }
    }

    void StartGame()
    {
        score = 0;
        uiManager?.UpdateScore(score);

        isAlive = true;
        rb.gravityScale = 2f;
        rb.velocity = Vector2.zero;
        transform.position = Vector3.zero;
        spriteRenderer.enabled = true;
        Flap();

        // Hızı ve pitch'i normale döndür
        moveSpeed = originalMoveSpeed;
        themesongSource.pitch = originalPitch;
        Debug.Log($"StartGame: Hız ve pitch normale döndürüldü: Hız = {moveSpeed}, Pitch = {themesongSource.pitch}");

        // Arka plan müziğini başlat
        if (themesongSource != null && themesong != null)
        {
            try
            {
                themesongSource.Play();
                Debug.Log("Themesong oynatıldı.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Themesong oynatılamadı: {e.Message}");
            }
        }
        else
        {
            Debug.LogError("ThemesongSource veya themesong null! Müzik çalınamadı.");
        }

        uiManager?.ShowGameScreen();
    }

    void Flap()
    {
        if (isAlive && rb.velocity.y < maxFlapHeight)
        {
            rb.velocity = new Vector2(rb.velocity.x, flapForce);
            if (tapSound != null && audioSource != null)
            {
                try
                {
                    audioSource.PlayOneShot(tapSound);
                    Debug.Log("tapSound oynatıldı.");
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"tapSound oynatılamadı: {e.Message}");
                }
            }
            feedbackManager?.PlayFlapParticles();
        }
    }

    void CheckScreenBoundaries()
    {
        if (!isAlive && (transform.position.y < -screenBoundary || transform.position.y > screenBoundary))
        {
            transform.position = new Vector3(transform.position.x, 0f, 0f);
            rb.velocity = Vector2.zero;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isAlive)
        {
            if (collision.gameObject.CompareTag("ObstacleEdge"))
            {
                movingRight = !movingRight;
                score++;
                uiManager?.UpdateScore(score);
                if (pointSound != null && audioSource != null)
                {
                    try
                    {
                        audioSource.PlayOneShot(pointSound);
                        Debug.Log("pointSound oynatıldı.");
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"pointSound oynatılamadı: {e.Message}");
                    }
                }

                transform.rotation = movingRight ? Quaternion.Euler(0, 0, 0) : Quaternion.Euler(0, -180, 0);
                OnDirectionChange?.Invoke(movingRight);
            }
            else if (collision.gameObject.CompareTag("Obstacle"))
            {
                StartCoroutine(GameOverSequence());
            }
        }
    }

    public void ApplyPowerup(Powerup powerup)
    {
        Debug.Log($"ApplyPowerup çağrıldı! Powerup Type: {powerup.Type}, Duration: {powerup.Duration}");
        if (powerup.Type == PowerupType.SpeedUp)
        {
            StartCoroutine(SpeedUp(powerup.Duration));
        }
        else if (powerup.Type == PowerupType.SlowDown)
        {
            StartCoroutine(SlowDown(powerup.Duration));
        }
        else
        {
            Debug.LogWarning($"Bilinmeyen Powerup Type: {powerup.Type}");
        }
    }

    private IEnumerator SpeedUp(float duration)
    {
        moveSpeed = originalMoveSpeed * 1.5f; // Hızı artır
        themesongSource.pitch = 1.3f; // Themesong hızını artır
        Debug.Log($"SpeedUp uygulandı: Hız = {moveSpeed}, Pitch = {themesongSource.pitch}, Duration = {duration}");

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed; // Hızı normale döndür
        themesongSource.pitch = originalPitch; // Pitch'i normale döndür
        Debug.Log($"SpeedUp süresi bitti: Hız = {moveSpeed}, Pitch = {themesongSource.pitch}");
    }

    private IEnumerator SlowDown(float duration)
    {
        moveSpeed = originalMoveSpeed * 0.5f; // Hızı azalt
        themesongSource.pitch = 0.7f; // Themesong hızını azalt
        Debug.Log($"SlowDown uygulandı: Hız = {moveSpeed}, Pitch = {themesongSource.pitch}, Duration = {duration}");

        yield return new WaitForSeconds(duration);

        moveSpeed = originalMoveSpeed; // Hızı normale döndür
        themesongSource.pitch = originalPitch; // Pitch'i normale döndür
        Debug.Log($"SlowDown süresi bitti: Hız = {moveSpeed}, Pitch = {themesongSource.pitch}");
    }

    private IEnumerator GameOverSequence()
    {
        isAlive = false;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (deathSound != null && audioSource != null)
        {
            try
            {
                audioSource.PlayOneShot(deathSound);
                Debug.Log("deathSound oynatıldı.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"deathSound oynatılamadı: {e.Message}");
            }
        }

        if (mainCamera != null)
        {
            Vector3 originalCamPos = mainCamera.transform.position;
            float elapsedTime = 0f;
            while (elapsedTime < shakeDuration)
            {
                elapsedTime += Time.deltaTime;
                float xOffset = Random.Range(-1f, 1f) * shakeMagnitude;
                float yOffset = Random.Range(-1f, 1f) * shakeMagnitude;
                mainCamera.transform.position = originalCamPos + new Vector3(xOffset, yOffset, 0);
                yield return null;
            }
            mainCamera.transform.position = originalCamPos;
        }

        feedbackManager?.PlayDeathParticles();
        transform.position = Vector3.zero;
        spriteRenderer.enabled = false;

        if (gameOverUISound != null && audioSource != null)
        {
            try
            {
                audioSource.PlayOneShot(gameOverUISound);
                Debug.Log("gameOverUISound oynatıldı.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"gameOverUISound oynatılamadı: {e.Message}");
            }
        }

        // Arka plan müziğini durdur
        if (themesongSource != null)
        {
            themesongSource.Stop();
            Debug.Log("Themesong durduruldu.");
        }

        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            PlayerPrefs.Save();
        }
        uiManager?.UpdateHighScore(highScore);
        uiManager?.ShowGameOverScreen();
    }

    public void RestartGame()
    {
        movingRight = true;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.position = Vector3.zero;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0f;
        isAlive = false;
        spriteRenderer.enabled = true;

        gameManager?.ResetPipes();
        FindObjectOfType<PowerupManager>()?.ClearPowerups();
        uiManager?.ShowStartScreen();

        // Ses ayar panelini kapat
        if (audioManager != null)
        {
            audioManager.ClosePanel();
            Debug.Log("RestartGame: AudioSettingsPanel kapatıldı.");
        }
    }

    public bool IsAlive() => isAlive;
    public int GetScore() => score;
    public Rigidbody2D GetRigidbody() => rb;

    // Ses seviyesini ayarlamak için metodlar
    public void SetThemesongVolume(float volume)
    {
        if (themesongSource != null)
        {
            themesongSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"Themesong ses seviyesi: {themesongSource.volume}");
        }
    }

    public void SetEffectsVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = Mathf.Clamp01(volume);
            Debug.Log($"Efekt ses seviyesi: {audioSource.volume}");
        }
    }
}